# EFT DMA Optic ESP — Implementation Reference

> **Audience:** LLM / developer implementing optic-aware ESP in an EFT DMA radar from scratch.
> **Game version context:** IL2CPP (post-Unity 2021 migration). All offsets are for March 2026 unless noted. Mono-era offsets commented where they differ.

---

## Table of Contents
1. [System Overview](#1-system-overview)
2. [Memory Structures and Offsets](#2-memory-structures-and-offsets)
3. [Finding the Cameras at Startup](#3-finding-the-cameras-at-startup)
4. [Scope Detection at Runtime](#4-scope-detection-at-runtime)
5. [View Matrix and Per-Frame Reads](#5-view-matrix-and-per-frame-reads)
6. [WorldToScreen with Scope Zoom](#6-worldtoscreen-with-scope-zoom)
7. [Class Architecture](#7-class-architecture)
8. [Full Data Flow Diagram](#8-full-data-flow-diagram)
9. [Common Pitfalls](#9-common-pitfalls)
10. [Offset Quick Reference](#10-offset-quick-reference)

---

## 1. System Overview

EFT uses **two separate Unity cameras** for rendering:

| Camera | Purpose | When active |
|--------|---------|-------------|
| `FPS Camera` | Standard first-person view | Always |
| `Optic Camera` | Scope/optic magnified view | When ADS through a magnifying optic |

The radar's ESP layer mirrors this: when the local player is aiming through a magnifying optic, the ESP switches its view matrix source from the FPS camera to the Optic camera. The `WorldToScreen` projection also applies the optic's actual FOV so that ESP elements remain correctly placed over game objects as seen through the scope.

There is **no separate rendering pipeline** for the scope — just a camera swap and an FOV-based coordinate transform. The same ESP draw calls are used for both states.

---

## 2. Memory Structures and Offsets

### 2.1 EFT.CameraControl.CameraManager (IL2CPP singleton)

This is the game's own camera manager class, accessed via its `get_Instance` static method. It holds both cameras.

```
EFTCameraManager instance
  +0x10  OpticCameraManager*    → OpticCameraManager object
  +0x60  Camera* ref            → FPS camera object reference (NOT the camera directly)
```

**Both `+0x10` and `+0x60` store a wrapper object pointer, not a Unity Camera directly.**
Each wrapper object must be dereferenced again at `+0x10` to get the actual Unity Camera:

```
fpsCameraRef   = ReadPtr(eftCamMgr + 0x60)
fpsCamera      = ReadPtr(fpsCameraRef + 0x10)   ← CameraDerefOffset

opticCamMgr    = ReadPtr(eftCamMgr + 0x10)
opticCameraRef = ReadPtr(opticCamMgr + 0x70)    ← OpticCameraManager.Camera
opticCamera    = ReadPtr(opticCameraRef + 0x10)  ← CameraDerefOffset
```

### 2.2 Unity Camera object (once resolved)

```
UnityEngine.Camera object
  +0x128  Matrix4x4   ViewMatrix    (IL2CPP; was 0x100 in Mono)
  +0x1A8  float       FOV           (IL2CPP; was 0x180 in Mono)
  +0x518  float       AspectRatio   (IL2CPP; was 0x4F0 in Mono)
  +0x35   bool        IsAdded       (after +0x10 dereference — DerefIsAddedOffset)
```

### 2.3 ProceduralWeaponAnimation (ADS detection)

```
LocalPlayer object
  +0x338  ProceduralWeaponAnimation*

ProceduralWeaponAnimation
  +0x145  bool   _isAiming    ← ADS flag (true = any ADS, iron sights or optics)
  +0x180  List*  _optics      ← list of active optic components
```

### 2.4 _optics list → SightNBone → SightComponent (scope zoom)

```
_optics (List<SightNBone*>)
  → element[0]
       +0x10  SightComponent*   ← SightNBone.Mod

SightComponent
  +0x20  SightInterface*   _template       ← pointer to the scope template
  +0x30  int[]*            ScopesSelectedModes
  +0x38  int               SelectedScope   ← which scope slot is active (0-based)
  +0x3C  float             ScopeZoomValue  ← direct zoom float (use this first)

SightInterface (via SightComponent._template)
  +0x1B8  ulong[]   Zooms    ← array of per-scope zoom float arrays
```

### 2.5 AllCameras fallback (UnityPlayer.dll)

If the EFT singleton path fails, Unity exposes an `AllCameras` static list inside `UnityPlayer.dll`:

```
UnityPlayer.dll + 0x19F3080  →  AllCameras static pointer

AllCameras list layout:
  +0x00  Camera*[]   items   (pointer to native array of camera pointers)
  +0x08  int         count
```

Each entry is a direct camera pointer (no extra dereference needed in the AllCameras path).

---

## 3. Finding the Cameras at Startup

Camera resolution is done **once per game session** during startup. Two paths are tried in order.

### 3.1 Primary path: EFT CameraManager singleton

**Step 1: Find `CameraManager.get_Instance`**

The singleton is accessed through a compiled IL2CPP static method. Find it via:
- **Signature scan** of `GameAssembly.dll` for the `get_Instance` method prologue (preferred — survives patches)
- **Hardcoded RVA fallback** (`GetInstance_RVA = 0x2CF8AB0` as of March 2026)

Once the method address is found, read its first 128 bytes and decode two patterns:

**Pattern 1 — `lea rcx, [rip+offset]`** (class metadata indirect):
```
Bytes: 48 8D 0D [disp32]
classMetadataAddr = methodAddr + i + 7 + disp32
classPtr = ReadValue<ulong>(classMetadataAddr)
// Try static fields offsets: 0xB8, 0xC0, 0xC8, 0xD0, 0xA8, 0xB0
staticFieldsPtr = ReadValue<ulong>(classPtr + offset)
instancePtr = ReadValue<ulong>(staticFieldsPtr)
// Validate: ReadValue<ulong>(instancePtr + 0x60) must be a valid VA
```

**Pattern 2 — `mov rax, [rip+offset]`** (direct static field):
```
Bytes: 48 8B 05 [disp32]
staticFieldAddr = methodAddr + i + 7 + disp32
instancePtr = ReadValue<ulong>(staticFieldAddr)
// Validate: ReadValue<ulong>(instancePtr + 0x60) must be a valid VA
```

**Step 2: Walk the pointer chain**

```csharp
// Validate by reading the name of the object at the camera ref address
var fpsCameraRef = ReadPtr(eftCamMgrInstance + 0x60);
AssertName(fpsCameraRef, "Camera");  // ObjectClass.ReadName must equal "Camera"
var fpsCamera = ReadPtr(fpsCameraRef + 0x10);
ValidateCameraMatrix(fpsCamera);     // matrix diagonal must be non-zero, non-NaN

var opticCamMgr  = ReadPtr(eftCamMgrInstance + 0x10);
var opticCamRef  = ReadPtr(opticCamMgr + 0x70);
AssertName(opticCamRef, "Camera");
var opticCamera  = ReadPtr(opticCamRef + 0x10);
```

Camera matrix validation:
```csharp
bool ValidateCameraMatrix(ulong cameraPtr)
{
    var vm = ReadValue<Matrix4x4>(cameraPtr + 0x128);
    if (float.IsNaN(vm.M11) || float.IsInfinity(vm.M11)) return false;
    if (vm.M11 == 0 && vm.M22 == 0 && vm.M33 == 0 && vm.M44 == 0) return false;
    if (Math.Abs(vm.M41) > 5000f || Math.Abs(vm.M42) > 5000f) return false;
    return true;
}
```

### 3.2 Fallback path: Unity AllCameras + name search

If the EFT singleton is unavailable (new patch broke the RVA, game is loading, etc.):

```csharp
ulong allCamerasStatic = unityPlayerBase + 0x19F3080;
ulong allCamerasPtr    = ReadPtr(allCamerasStatic);
ulong itemsPtr         = ReadPtr(allCamerasPtr + 0x00);
int   count            = ReadValue<int>(allCamerasPtr + 0x08);

for (int i = 0; i < Math.Min(count, 100); i++)
{
    ulong camPtr    = ReadPtr(itemsPtr + (ulong)(i * 8));
    ulong gameObj   = ReadPtr(camPtr + GameObject.ObjectClassOffset);
    ulong namePtr   = ReadPtr(gameObj + GameObject.NameOffset);
    string name     = ReadUnityString(namePtr, 64);

    bool isFPS   = name contains "FPS" and "Camera" (case-insensitive)
    bool isOptic = name contains ("Optic" or "BaseOptic") and "Camera"
}
```

Both cameras are stored for the remainder of the session. The AllCameras path returns the cameras without the extra `+0x10` dereference — `itemsPtr[i]` is already the Unity Camera object.

---

## 4. Scope Detection at Runtime

Scope detection runs **every frame** (or every ESP tick) before selecting which camera to use for the view matrix read.

### 4.1 ADS check (fast path)

```csharp
// LocalPlayer.PWA is the cached ProceduralWeaponAnimation address
bool isADS = ReadValue<bool>(localPlayer.PWA + 0x145);  // _isAiming
```

If `isADS` is false, the player is not even aiming. Skip scope detection entirely — use FPS camera.

### 4.2 Scope/optic check (only when ADS)

```csharp
bool CheckIfScoped(LocalPlayer localPlayer)
{
    if (!OpticCamera.IsValidVirtualAddress()) return false;

    // Read the active optics list
    ulong opticsPtr = ReadPtr(localPlayer.PWA + 0x180);   // _optics
    // opticsPtr → List<SightNBone>
    // read as a MemList<MemPointer> — standard IL2CPP managed list
    if (optics.Count == 0) return false;

    // Get SightComponent from first optic entry
    ulong pSightComponent = ReadPtr(optics[0] + 0x10);    // SightNBone.Mod
    var sightComponent = ReadValue<SightComponent>(pSightComponent);

    // Fast path: ScopeZoomValue field is set directly by the game
    if (sightComponent.ScopeZoomValue != 0f)
        return sightComponent.ScopeZoomValue > 1.0f;

    // Fallback: decode zoom from the Zooms array (handles multi-zoom scopes)
    float zoom = sightComponent.GetZoomLevel();
    return zoom > 1.0f;
}
```

### 4.3 GetZoomLevel() — multi-zoom scope support

```csharp
float GetZoomLevel()
{
    // SightInterface.Zooms = array of per-scope zoom float arrays
    // SelectedScope = which scope slot is active
    // ScopesSelectedModes[SelectedScope] = which zoom mode within that scope

    var zoomArray = ReadArray<ulong>(pZooms);  // Zooms field at +0x1B8 in SightInterface

    if (SelectedScope >= zoomArray.Length || SelectedScope < 0) return -1f;

    // selectedScopeModes[SelectedScope] gives the current zoom mode index
    var selectedScopeModes = ReadArray<int>(pScopeSelectedModes);  // ScopesSelectedModes
    int selectedMode = (SelectedScope < selectedScopeModes.Length)
                     ? selectedScopeModes[SelectedScope] : 0;

    // Zooms[SelectedScope] is itself an array of floats — one per zoom level
    ulong zoomAddr = zoomArray[SelectedScope]
                   + MemArray<float>.ArrBaseOffset   // typically 0x20
                   + (uint)selectedMode * 0x4;

    float zoomLevel = ReadValue<float>(zoomAddr);
    return (zoomLevel >= 0f && zoomLevel < 100f) ? zoomLevel : -1f;
}
```

### 4.4 State summary

After the per-frame check:

```csharp
IsADS    = localPlayer?.CheckIfADS() ?? false;
IsScoped = IsADS && CheckIfScoped(localPlayer);

// Camera selection for view matrix read
ulong activeCamera = (IsADS && IsScoped && OpticCamera.IsValidVirtualAddress())
    ? OpticCamera
    : FPSCamera;
```

---

## 5. View Matrix and Per-Frame Reads

View matrix reads are queued as **scatter reads** to minimize DMA round-trips.

### 5.1 What is read each frame

```csharp
void OnRealtimeLoop(ScatterReadIndex index, LocalPlayer localPlayer)
{
    IsADS    = localPlayer?.CheckIfADS() ?? false;
    IsScoped = IsADS && CheckIfScoped(localPlayer);

    ulong camera = (IsScoped && IsADS) ? OpticCamera : FPSCamera;

    // Queue: view matrix from active camera
    index.AddEntry<Matrix4x4>(0, camera + 0x128);   // Camera.ViewMatrix

    // Queue: FOV and aspect ratio always from FPS camera
    // (optic camera FOV is irrelevant — WorldToScreen derives zoom from _fov + IsScoped flag)
    index.AddEntry<float>(1, FPSCamera + 0x1A8);    // Camera.FOV
    index.AddEntry<float>(2, FPSCamera + 0x518);    // Camera.AspectRatio

    index.Callbacks += results =>
    {
        if (results.TryGetRef<Matrix4x4>(0, out ref var vm))
            _viewMatrix.Update(ref vm);

        if (results.TryGetResult<float>(1, out var fov) && fov > 1f && fov < 180f)
            _fov = fov;

        if (results.TryGetResult<float>(2, out var aspect) && aspect > 0.1f && aspect < 5f)
            _aspect = aspect;
    };
}
```

**Key decision:** The view matrix comes from the **optic camera** when scoped (gives the optic's actual world-to-clip transform). FOV and aspect ratio always come from the **FPS camera** — the base FOV is used as the unscoped reference for the zoom calculation in `WorldToScreen`.

### 5.2 ViewMatrix transposition

Unity stores its camera matrix in column-major order. EFT's IL2CPP matrix is read directly as a raw `Matrix4x4` from memory. The `ViewMatrix` helper class transposes the relevant fields:

```csharp
// Raw Matrix4x4 from memory (column-major Unity layout)
// Transposed fields needed for dot-product projection:
M44         = matrix.M44;   // w homogeneous component
M14         = matrix.M41;   // transposed: right-row w
M24         = matrix.M42;   // transposed: up-row w
Translation = (matrix.M14, matrix.M24, matrix.M34);  // camera forward/translation row
Right       = (matrix.M11, matrix.M21, matrix.M31);  // camera right row
Up          = (matrix.M12, matrix.M22, matrix.M32);  // camera up row
```

This transposition converts the column-major storage into the dot-product form used in `WorldToScreen`.

---

## 6. WorldToScreen with Scope Zoom

The core projection transforms a world-space `Vector3` to a 2D screen `SKPoint`.

### 6.1 Base projection

```csharp
static bool WorldToScreen(ref Vector3 worldPos, out SKPoint scrPos)
{
    // Reject origin-stuck positions (failed bone reads stay at 0,0,0)
    if (worldPos.LengthSquared() < 1f) { scrPos = default; return false; }

    // Homogeneous w — behind-camera rejection
    float w = Vector3.Dot(_viewMatrix.Translation, worldPos) + _viewMatrix.M44;
    if (w < 0.098f) { scrPos = default; return false; }

    // Screen-space x/y in clip space (-1..+1)
    float x = Vector3.Dot(_viewMatrix.Right, worldPos) + _viewMatrix.M14;
    float y = Vector3.Dot(_viewMatrix.Up,    worldPos) + _viewMatrix.M24;

    // ... (scope zoom applied here — see 6.2) ...

    var center = ViewportCenter;  // (screenWidth/2, screenHeight/2)
    scrPos = new SKPoint(
        center.X * (1f + x / w),
        center.Y * (1f - y / w)
    );
    return true;
}
```

### 6.2 Scope zoom correction

When `IsScoped` is true the optic camera's view matrix is already in use, but `x` and `y` still reflect the optic's wider clip-space scale. The FPS camera's stored FOV is used to undo the angular scale difference:

```csharp
if (IsScoped)
{
    float angleRadHalf = (MathF.PI / 180f) * _fov * 0.5f;
    float angleCtg     = MathF.Cos(angleRadHalf) / MathF.Sin(angleRadHalf);  // cot(half-FOV)

    x /= angleCtg * _aspect * 0.5f;
    y /= angleCtg * 0.5f;
}
```

**Why this works:** The optic camera's projection matrix already encodes the magnified FOV. The clip-space coordinates `x/w` and `y/w` are in the optic camera's space. Dividing by `cot(FPS_FOV/2) * 0.5` scales them back into normalized screen space relative to the base FOV, giving correct pixel placement on the monitor overlay.

Without this correction, ESP elements appear zoomed/offset when looking through a scope.

### 6.3 On-screen check

```csharp
if (onScreenCheck)
{
    int tolerance = useTolerance ? 800 : 0;  // 800px padding for off-screen entities
    if (scrPos.X < Viewport.Left - tolerance || scrPos.X > Viewport.Right + tolerance ||
        scrPos.Y < Viewport.Top  - tolerance || scrPos.Y > Viewport.Bottom + tolerance)
    {
        scrPos = default;
        return false;
    }
}
```

The tolerance mode (`useTolerance = true`) allows entities slightly off-screen to still receive labels or distance calculations. Used for name tags that may clip the edge.

---

## 7. Class Architecture

```
CameraManagerBase  (abstract, Common.Unity)
│
│  Static state:
│    IsScoped, IsADS
│    _fov, _aspect, _viewMatrix
│    Viewport, ViewportCenter
│
│  Static methods:
│    WorldToScreen()
│    UpdateViewportRes()
│    GetFovMagnitude()
│
└── CameraManager  (sealed, Tarkov.GameWorld)
      │
      │  Instance state:
      │    FPSCamera    (ulong address)
      │    OpticCamera  (ulong address)
      │    _eftCameraManagerInstance  (static, reset on GameStopped)
      │
      │  Startup (called once per game session):
      │    static Initialize()            → calls FindCameraManagerInstance()
      │    ctor → TryResolveCameras()
      │         → TryResolveViaCameraManagerInstance()   (primary)
      │         → TryResolveViaAllCamerasByName()        (fallback)
      │
      │  Per-frame:
      │    OnRealtimeLoop(ScatterReadIndex, LocalPlayer)
      │       → IsADS = localPlayer.CheckIfADS()
      │       → IsScoped = IsADS && CheckIfScoped(localPlayer)
      │       → queue scatter: ViewMatrix, FOV, AspectRatio
      │
      └── Inner structs (ref struct, stack-only):
            SightComponent   (LayoutKind.Explicit)
            SightInterface   (LayoutKind.Explicit)

ViewMatrix  (UI.ESP)
  Holds transposed fields from the raw Matrix4x4.
  Updated each frame via Update(ref Matrix4x4).
```

### 7.1 Initialization sequence

```
GameStartup
  └─ CameraManager.Initialize()
       └─ FindCameraManagerInstance()     // static — resolves singleton address
            ├─ signature scan or RVA fallback → methodAddr
            ├─ read method bytes → decode Pattern 1 or 2 → instancePtr
            └─ stores _eftCameraManagerInstance

  └─ new CameraManager()                 // instance — resolves actual camera addresses
       └─ TryResolveCameras()
            ├─ TryResolveViaCameraManagerInstance()
            │    └─ walk pointer chain:
            │         fpsCameraRef → fpsCamera (validated)
            │         opticCamMgr → opticCamRef → opticCamera
            └─ or TryResolveViaAllCamerasByName()
                 └─ scan AllCameras list by name

  └─ new LocalGameWorld(cameraManager)
       └─ ESP realtime loop calls:
            cameraManager.OnRealtimeLoop(scatterIndex, localPlayer)
```

### 7.2 GameStopped cleanup

```csharp
static CameraManager()
{
    MemDMABase.GameStopped += (s, e) => _eftCameraManagerInstance = 0;
}
```

When the game process exits, the static instance pointer is cleared. The next `Initialize()` call will re-resolve it fresh.

---

## 8. Full Data Flow Diagram

```
Every ESP tick (~8ms):
┌──────────────────────────────────────────────────────────────┐
│  1. SCOPE STATE                                              │
│     localPlayer.PWA + 0x145 → _isAiming       → IsADS      │
│     localPlayer.PWA + 0x180 → _optics[0]                   │
│                              → SightNBone + 0x10            │
│                              → SightComponent               │
│                                +0x3C ScopeZoomValue > 1?    │
│                                or GetZoomLevel() > 1?       │
│                              → IsScoped                      │
├──────────────────────────────────────────────────────────────┤
│  2. CAMERA SELECTION                                         │
│     IsScoped && IsADS → OpticCamera                         │
│     else              → FPSCamera                           │
├──────────────────────────────────────────────────────────────┤
│  3. SCATTER READ                                             │
│     activeCamera + 0x128 → Matrix4x4  → _viewMatrix.Update()│
│     fpsCamera    + 0x1A8 → float      → _fov               │
│     fpsCamera    + 0x518 → float      → _aspect            │
├──────────────────────────────────────────────────────────────┤
│  4. WORLD TO SCREEN  (per entity, per draw call)            │
│     w = dot(Translation, worldPos) + M44                    │
│     x = dot(Right, worldPos) + M14                         │
│     y = dot(Up, worldPos) + M24                            │
│     if IsScoped:                                            │
│         x /= cot(fov/2) * aspect * 0.5                     │
│         y /= cot(fov/2) * 0.5                              │
│     scrX = viewW/2 * (1 + x/w)                             │
│     scrY = viewH/2 * (1 - y/w)                             │
└──────────────────────────────────────────────────────────────┘
```

---

## 9. Common Pitfalls

### 9.1 `+0x10` double-dereference on camera pointers
Both `EFTCameraManager.Camera` (`+0x60`) and `OpticCameraManager.Camera` (`+0x70`) store a **wrapper object reference**, not the Unity Camera directly. You must dereference again at `+0x10` (`CameraDerefOffset`) to get the actual Camera. Reading the view matrix directly from the wrapper gives garbage.

### 9.2 Validating the camera pointer before using it
Always call `ValidateCameraMatrix()` on the FPS camera after resolving. If the game hasn't fully loaded (e.g., main menu), the camera object may exist but its matrix will be all zeros or NaN. Using an invalid matrix causes every ESP element to project to `(0,0)`.

### 9.3 `IsScoped` must be false for iron sights
`_isAiming` is true for all ADS — iron sights, red dots, magnified scopes. Only `ScopeZoomValue > 1` (or zoom level > 1) means a magnified optic. Setting `IsScoped = true` based on `_isAiming` alone causes incorrect FOV scaling through iron sights.

### 9.4 Read FOV/Aspect from FPS camera, not OpticCamera
The optic camera's FOV in memory reflects the magnified view — do not use it for the `WorldToScreen` zoom correction. The formula needs the **base** (unscoped) FOV. Always read `FOV` and `AspectRatio` from `FPSCamera`, even when the view matrix comes from `OpticCamera`.

### 9.5 `ScopeZoomValue` is zero for some sights
`ScopeZoomValue` (`+0x3C`) is populated by EFT's scope system but may be `0.0f` for simple scopes or when transitioning. Always fall back to `GetZoomLevel()` via the `SightInterface.Zooms` array when `ScopeZoomValue == 0f`.

### 9.6 `_eftCameraManagerInstance` stales between sessions
The singleton address changes every time the game process restarts. Clear it on `GameStopped` and re-resolve on `GameStarted`. Do not persist it across sessions.

### 9.7 AllCameras fallback has no extra dereference
In the AllCameras fallback path, `itemsPtr[i]` is the **camera object directly** — there is no `+0x10` wrapper dereference. Only the EFT CameraManager path requires that extra step. Mixing the two causes invalid camera addresses.

### 9.8 Behind-camera rejection (`w < 0.098f`)
If `w` is negative, the world point is behind the camera. If it is near-zero, it would cause a division-by-near-zero. The `0.098f` threshold prevents both — do not lower it.

### 9.9 Origin-stuck bone positions (`LengthSquared < 1f`)
IL2CPP bone reads sometimes return `(0, 0, 0)` on failure. Always reject world positions near the origin to avoid projecting all failed reads to the screen center.

---

## 10. Offset Quick Reference

```csharp
// EFT CameraManager singleton
EFTCameraManager.OpticCameraManager = 0x10   // → OpticCameraManager object
EFTCameraManager.Camera             = 0x60   // → FPS Camera wrapper object (NOT camera directly)
EFTCameraManager.CameraDerefOffset  = 0x10   // → apply to wrapper to get actual Camera
EFTCameraManager.GetInstance_RVA    = 0x2CF8AB0  // March 2026 — fallback if signature fails

// OpticCameraManager
OpticCameraManager.Camera           = 0x70   // → Optic Camera wrapper object

// Unity Camera object (after CameraDerefOffset applied)
Camera.ViewMatrix    = 0x128   // Matrix4x4   (Mono: 0x100)
Camera.FOV           = 0x1A8   // float       (Mono: 0x180)
Camera.AspectRatio   = 0x518   // float       (Mono: 0x4F0)
Camera.DerefIsAddedOffset = 0x35  // bool IsAdded (after +0x10 deref)

// AllCameras static (UnityPlayer.dll)
ModuleBase.AllCameras = 0x19F3080  // pointer to AllCameras list (March 2026)
// AllCameras list: +0x00 → items[], +0x08 → int count

// Player ADS / optics
ProceduralWeaponAnimation._isAiming = 0x145  // bool
ProceduralWeaponAnimation._optics   = 0x180  // List<SightNBone>
SightNBone.Mod                      = 0x10   // SightComponent*
Player.ProceduralWeaponAnimation    = 0x338  // ProceduralWeaponAnimation*

// SightComponent (read as LayoutKind.Explicit ref struct)
SightComponent._template            = 0x20   // SightInterface*
SightComponent.ScopesSelectedModes  = 0x30   // int[]*
SightComponent.SelectedScope        = 0x38   // int
SightComponent.ScopeZoomValue       = 0x3C   // float  ← use this first

// SightInterface (via SightComponent._template)
SightInterface.Zooms                = 0x1B8  // ulong[] (array of per-scope zoom float arrays)
```

---

## Implementation Checklist

- [ ] Add all offsets in `Offsets` / `UnityOffsets` structs
- [ ] Implement `CameraManagerBase` with static `IsScoped`, `IsADS`, `_fov`, `_aspect`, `_viewMatrix`
- [ ] Implement `WorldToScreen()` with `w < 0.098f` rejection, scope zoom correction, viewport check
- [ ] Implement `ViewMatrix` helper with correct transposition of column-major Unity matrix
- [ ] Implement `CameraManager.Initialize()` — find `get_Instance` via signature scan + RVA fallback
- [ ] Implement `FindCameraManagerInstance()` — decode Pattern 1 (lea rcx) and Pattern 2 (mov rax) from method bytes
- [ ] Implement `TryResolveViaCameraManagerInstance()` — walk pointer chain with name validation and matrix validation
- [ ] Implement `TryResolveViaAllCamerasByName()` — AllCameras list scan (no `+0x10` dereference)
- [ ] Implement `LocalPlayer.CheckIfADS()` — read `_isAiming` from PWA
- [ ] Implement `CheckIfScoped()` — read `_optics` list, `SightComponent.ScopeZoomValue`, fallback to `GetZoomLevel()`
- [ ] Implement `GetZoomLevel()` — decode `SightInterface.Zooms` array for multi-zoom scopes
- [ ] Implement `OnRealtimeLoop()` — select camera, queue scatter reads for matrix + FOV + aspect
- [ ] Hook `GameStopped` event to clear `_eftCameraManagerInstance`
- [ ] Call `UpdateViewportRes()` when ESP window is created or monitor changes
- [ ] Reject `worldPos.LengthSquared() < 1f` in WorldToScreen
