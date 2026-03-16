# Il2CppDumper — Technical Documentation

**File:** `src/Tarkov/Unity/IL2CPP/Il2CppDumper.cs`
**Namespace:** `eft_dma_radar.Tarkov.Unity.IL2CPP`
**Class:** `Il2CppDumper` (static)

---

## Overview

`Il2CppDumper` is a runtime IL2CPP metadata resolver. It reads the `GameAssembly.dll` process memory via DMA (Direct Memory Access) scatter-reads to enumerate every IL2CPP class registered in the game's type-info table, then populates the static fields in `Offsets.*` with the actual field offsets and method RVAs for the current game version.

This allows the radar to remain compatible across EFT updates without manually patching hardcoded offsets — the dumper resolves offsets at startup and only falls back to the hardcoded values in `SDK.cs` if a class or field cannot be located.

---

## Architecture

```
Dump()
 ├─ ResolveTypeInfoTableRva()      ← sig-scan GameAssembly for the table
 ├─ ReadAllClassesFromTable()      ← bulk + scatter read all Il2CppClass*
 ├─ BuildSchema()                  ← declarative list of what to look up
 └─ per-schema-class loop
      ├─ ReadClassFields()         ← read FieldInfo[] for the class
      ├─ ReadClassMethods()        ← read MethodInfo*[] for the class (if needed)
      └─ TrySetField()             ← write result into Offsets.* via reflection
```

---

## IL2CPP Memory Layout Constants

These constants describe the internal layout of IL2CPP runtime structs inside `GameAssembly.dll`:

### `Il2CppClass` offsets
| Constant       | Value  | Meaning                                      |
|----------------|--------|----------------------------------------------|
| `K_Name`       | `0x10` | `char* name` — plain class name              |
| `K_Namespace`  | `0x18` | `char* namespaze` — namespace string         |
| `K_Fields`     | `0x80` | `FieldInfo*` — direct array of field entries |
| `K_Methods`    | `0x98` | `MethodInfo**` — array of method pointers    |
| `K_MethodCount`| `0x120`| `uint16_t` method count                      |
| `K_FieldCount` | `0x124`| `uint16_t` field count                       |

### `FieldInfo` offsets (stride = `0x20`)
| Constant    | Value  | Meaning                              |
|-------------|--------|--------------------------------------|
| `FI_Name`   | `0x00` | `char* name`                         |
| `FI_Offset` | `0x18` | `int32 offset` (signed — can be -1 for statics) |
| `FI_Stride` | `0x20` | Size of one `FieldInfo` entry        |

### `MethodInfo` offsets
| Constant     | Value  | Meaning                 |
|--------------|--------|-------------------------|
| `MI_Pointer` | `0x00` | `void* methodPointer`   |
| `MI_Name`    | `0x18` | `char* name`            |

---

## Scatter-Read Structs

Two blittable structs are used to batch DMA reads efficiently:

### `ClassNamePtrs`
Read 16 bytes from `Il2CppClass + 0x10` to capture both name pointers in a single scatter entry:
```csharp
[StructLayout(LayoutKind.Sequential)]
private struct ClassNamePtrs {
    public ulong NamePtr;      // 0x10 char* name
    public ulong NamespacePtr; // 0x18 char* namespaze
}
```

### `RawFieldInfo`
Captures name pointer and field offset for one field entry:
```csharp
[StructLayout(LayoutKind.Explicit, Size = 0x20)]
private struct RawFieldInfo {
    [FieldOffset(0x00)] public ulong NamePtr;
    [FieldOffset(0x18)] public int Offset;   // signed
}
```

### `RawMethodInfo`
Captures method function pointer and name pointer:
```csharp
[StructLayout(LayoutKind.Explicit, Size = 0x20)]
private struct RawMethodInfo {
    [FieldOffset(0x00)] public ulong MethodPointer;
    [FieldOffset(0x18)] public ulong NamePtr;
}
```

---

## Schema System

The schema is a declarative description of which classes and fields the dumper should resolve. It is built once per `Dump()` call via `BuildSchema()`.

### `SchemaField`
Describes a single field or method to resolve:
- `Il2CppName` — the name as it appears in IL2CPP metadata (e.g. `<MovementContext>k__BackingField`)
- `CsName` — the name of the corresponding field in the `Offsets.*` struct
- `Kind` — `Normal` (field offset) or `MethodRva` (method RVA relative to `GameAssembly` base)

### `SchemaClass`
Describes a single class to look up:
- `Il2CppName` — plain class name for name-based lookup
- `CsName` — which `Offsets.*` nested struct to populate
- `IsStatic` — whether the class is a singleton static (informational)
- `Fields` — array of `SchemaField` entries
- `TypeIndex` — optional O(1) direct lookup index for obfuscated classes (see below)

### Helper shorthand methods
```csharp
F("fieldName")           // Normal field
F("il2cpp_name", "cs")  // Field with alias
M("methodName")          // Method → emitted as methodName_RVA
C("ClassName", [...])    // Create a SchemaClass
```

### TypeIndex vs. Name-based Lookup

EFT obfuscates some class names with Unicode escapes (`\uXXXX`). For these, name-string matching is unreliable. The schema supports a `TypeIndex` (the MDToken index) for O(1) direct lookup:

```
TypeIndex = (MDToken & 0x00FFFFFF) - 1
```

With a `TypeIndex`, the dumper reads `tablePtr + TypeIndex * 8` directly — no string scan needed.

---

## Execution Flow

### 1. `Dump()` — Entry point

Called once at radar startup (after `GameAssemblyBase` is known).

1. Validates `GameAssemblyBase != 0`.
2. Calls `ResolveTypeInfoTableRva()` to locate the `s_Il2CppMetadataRegistration.typeInfoTable` global.
3. Reads the table pointer: `tablePtr = *(gaBase + TypeInfoTableRva)`.
4. Calls `ReadAllClassesFromTable(tablePtr)` to get all class names, namespaces, pointers, and indices.
5. Builds a `nameLookup` dictionary and `nameToIndex` dictionary.
6. Builds the schema via `BuildSchema()`.
7. Calls `CheckSchemaCompleteness()` to warn about uncovered `Offsets.*` structs.
8. Iterates over every `SchemaClass`, resolving it either by `TypeIndex` or by name.
9. For each class, calls `ReadClassFields()` and optionally `ReadClassMethods()`.
10. For each `SchemaField`, looks up the resolved offset and calls `TrySetField()` to write it into `Offsets.*` via reflection.
11. Logs a summary: `X offsets updated, Y using hardcoded fallback, Z classes skipped`.

---

### 2. `ResolveTypeInfoTableRva()` — Sig-scan for the type table

Tries up to 4 byte-pattern signatures (in priority order) to locate the `mov [rip+rel32], rax` instruction that writes the `typeInfoTable` global in `GameAssembly.dll`:

| # | Pattern | Description |
|---|---------|-------------|
| 1 | `48 89 05 ? ? ? ? 48 8B 05 ? ? ? ? 8B 50` | Strict: followed by `mov edx, [rax+30]` |
| 2 | `48 89 05 ? ? ? ? 48 8B 05 ? ? ? ?` | Relaxed tail |
| 3 | `48 89 05 ? ? ? ? 33 C9` | Followed by `xor ecx,ecx` |
| 4 | `48 89 05 ? ? ? ? 48 85` | Followed by `test reg,reg` |

For each match:
1. Reads the RIP-relative `int32` displacement at `sigAddr + relOffset`.
2. Computes `globalVA = sigAddr + instrLen + displacement`.
3. Converts to RVA: `rva = globalVA - gaBase`.
4. Calls `ValidateTypeInfoTable()` to confirm it points at real class data.
5. Updates `Offsets.Special.TypeInfoTableRva` and returns `true` on success.

If all signatures fail, falls back to the hardcoded value in `Offsets.Special.TypeInfoTableRva` and validates that too.

---

### 3. `ValidateTypeInfoTable()` — Sanity check

Probes the first 8 entries of a candidate table. A table is valid if at least 3 entries:
- Are non-null valid virtual addresses.
- Have a readable `Il2CppClass::name` pointer.
- Pass `IsPlausibleClassName()` (printable ASCII, no control characters).

---

### 4. `ReadAllClassesFromTable()` — Bulk class scan

Efficiently enumerates all classes using batched DMA operations:

**Round 1 — Single bulk read:**
`Memory.ReadArray<ulong>(tablePtr, 80_000)` — reads up to 80,000 class pointers at once.

**Round 2 — Scatter read (16 bytes each):**
For every valid pointer, scatter-reads `ClassNamePtrs` (name ptr + namespace ptr) in one batched DMA call.

**Round 3 — Scatter read strings:**
Scatter-reads all name and namespace UTF-8 strings in a single batched DMA call.

Returns `List<(string Name, string Namespace, ulong KlassPtr, int Index)>`.

---

### 5. `ReadClassFields()` — Field offset resolution

For a given `Il2CppClass*`:
1. Reads `fieldCount` from `klassPtr + 0x124`.
2. Reads `fieldsBase` pointer from `klassPtr + 0x80`.
3. Bulk-reads all `RawFieldInfo[fieldCount]` entries in one DMA call.
4. Scatter-reads all field name strings in one batch.
5. Returns `Dictionary<string, int>` mapping field name → signed offset.

---

### 6. `ReadClassMethods()` — Method RVA resolution

For a given `Il2CppClass*`:
1. Reads `methodCount` from `klassPtr + 0x120`.
2. Reads `methodsBase` from `klassPtr + 0x98`.
3. Bulk-reads all `MethodInfo*` pointers.
4. Scatter-reads `RawMethodInfo` (pointer + name ptr) for all valid entries in one batch.
5. Scatter-reads all method name strings in one batch.
6. Returns `Dictionary<string, ulong>` mapping method name → RVA (relative to `gaBase`).

Only called for `SchemaClass` entries that contain at least one `MethodRva` field.

---

### 7. `TrySetField()` — Reflection-based offset injection

Sets a static field on an `Offsets.*` nested type:
- Supports `uint`, `ulong`, `int` field types (with automatic conversion).
- For `uint[]` fields (deref chains), updates only `arr[0]` in-place.
- Silently skips `const` / literal fields.
- Logs a warning for unsupported field types.

---

### 8. `CheckOffsetDelta()` — Delta detection

Before writing a new offset, compares it against the current (hardcoded or previously resolved) value. If the delta exceeds `0x200` (512 bytes), logs a warning:

```
[Il2CppDumper] DELTA WARN: Player._characterController: 0xA0 → 0x2B0 (delta 0x210)
```

A large delta usually indicates the wrong field was matched, or the game received a major structural update.

---

### 9. `CheckSchemaCompleteness()` — Coverage audit

At the start of each `Dump()`, iterates all nested types inside `Offsets` and warns about any that have no corresponding `SchemaClass` entry:

```
[Il2CppDumper] COVERAGE WARN: Offsets.SomeMissingClass has no schema entry — using hardcoded fallback.
```

The `Special` struct is exempt from this check.

---

## Backing Field Name Flipping

IL2CPP auto-properties can appear under two naming conventions depending on the game build:
- `<Name>k__BackingField`
- `_Name_k__BackingField`

`FlipBackingFieldConvention()` converts between them automatically. If the first lookup fails, the alternative convention is tried before falling back.

---

## Logging

All diagnostic output goes through `XMLogging.WriteLine(...)`. Key log lines:

| Message | Meaning |
|---------|---------|
| `[Il2CppDumper] Dump starting...` | Entry point reached |
| `[Il2CppDumper] TypeInfoTable resolved via: <desc>` | Sig scan succeeded |
| `[Il2CppDumper] Type table: N classes found.` | Class table size |
| `[Il2CppDumper] SKIP 'X': not found in type table.` | Class name not in metadata |
| `[Il2CppDumper] WARN: field 'X' not found in 'Y'` | Field not in class — fallback used |
| `[Il2CppDumper] DELTA WARN: ...` | Offset changed by > 0x200 from previous value |
| `[Il2CppDumper] COVERAGE WARN: Offsets.X has no schema entry` | Schema gap detected |
| `[Il2CppDumper] Done. X updated, Y fallback, Z skipped.` | Summary |

To enable per-class verbose field/method logging, uncomment the `LogClassDump(...)` call at line ~691.

---

## Adding a New Class to the Schema

1. Identify the IL2CPP class name (e.g. `MyClass`) and find its fields in a dump or via Il2CppInspector.
2. Add the corresponding `Offsets.MyClass` nested struct to `SDK.cs` with hardcoded fallback values.
3. Add a schema entry to `BuildSchema()`:

```csharp
C("MyClass", [
    F("_myField"),
    F("<MyProp>k__BackingField", "MyProp"),
    M("get_Instance"),  // method RVA
]),
```

4. For obfuscated classes, add the `TypeIndex` (`ti:` param):

```csharp
C("\u0041\u0042", [F("_field")], cs: "MyClass", ti: 12345),
```

---

## Limits and Guards

| Guard | Value | Reason |
|-------|-------|--------|
| `MaxClasses` | `80,000` | Upper bound for type table scan |
| `MaxNameLen` | `256` | Max UTF-8 string read length |
| `OffsetDeltaThreshold` | `0x200` | Triggers delta warning |
| Field count guard | `> 4096` | Prevents reading garbage data |
| Method count guard | `> 4096` | Prevents reading garbage data |
| `requiredValid` (validation) | `3` of first `8` | TypeInfoTable probe threshold |
