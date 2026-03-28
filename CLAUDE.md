# Claude Code Instructions — affectioned/eft-dma-radar fork

## README is the law

`README.md` Fork Changes section is the authoritative contract for what this fork contains.
Before finishing any upstream sync, cherry-pick, or merge, verify the codebase matches every claim in that section.

---

## Features that must NEVER be present

These were intentionally removed from the fork. Upstream may re-add them — always reject or revert them.

### eft-api.tech profile lookup
- Delete `src/Web/ProfileApi/` entirely (`EFTProfileService.cs`, `ApiKeyWizard.cs`, `ApiKeyStore.cs`, `DogtagDatabase.cs`)
- Remove `using eft_dma_radar.Tarkov.API` and `using static eft_dma_radar.Tarkov.API.EFTProfileService` from all files
- Remove `Config.AlternateProfileService`, `Config.RequestsPerMin`, `ProfileApiCache` class from `Config.cs`
- Remove "Player API Service" expander from `GeneralSettingsControl.xaml`

### Memory writes
- Delete `src/Tarkov/Features/MemoryWrites/` and anything named `*MemWrite*` or `*MemoryWrite*`
- Delete `src/UI/Pages/MemoryWritingControl.xaml` and `.xaml.cs`
- Delete `IMemWriteFeature.cs`, `MemWriteFeature.cs`
- `Config.Aimbot` must be a direct `[JsonInclude]` property — never `=> MemWrites.Aimbot`

### WebRadar
- Delete `src/Web/WebRadar/` and anything named `*WebRadar*`

### HideoutStash / HideoutManager
- Delete `src/Tarkov/Hideout/`, `HideoutStashControl.xaml/.cs`, `StashDogtagDumper.cs`

### QuestPlanner
- Delete `src/Tarkov/QuestPlanner/`

---

## Files that must always be preserved

These are fork-specific. If upstream deletes them (UD conflict), use `git add` to keep our version.

| Path | What it is |
|------|-----------|
| `src/Misc/Makcu/` (all 3 files) | Hardware aimbot via Makcu USB HID device |
| `src/Tarkov/Features/Aimbot.cs` | Makcu aimbot logic |
| `src/UI/Pages/AimbotControl.xaml` + `.cs` | Aimbot settings UI |
| `.github/workflows/release.yml` | CI/CD pipeline |
| `README.md` | Fork-specific readme — always revert to our version after cherry-picking |

---

## AimbotConfig shape (Makcu version)

`AimbotConfig` in `Config.cs` must have these fields — **not** upstream's `TargetingMode`/`SilentAim`/`RandomBone`:

```csharp
public bool Enabled
public string MakcuPort      // e.g. "COM3"
public bool AutoConnect
public float FovDegrees      // degrees, NOT pixel radius
public Bones AimBone
public float AlphaX
public float AlphaY
public float Deadzone
public float GaussianNoise
public bool AimAI
```

FOV circle radius = `halfWidth * MathF.Tan(FovDegrees * MathF.PI / 180f)` — never a flat pixel value.

---

## Post-merge verification checklist

Run after any upstream cherry-pick or sync:

1. `grep -r "eft-api.tech" src/` → must return nothing in real source files
2. `src/Web/ProfileApi/` → must not exist
3. `grep -r "MemoryWrites\|MemWriteFeature\|MemoryWritingControl" src/` → must return nothing
4. `src/Web/WebRadar/` → must not exist
5. `src/Tarkov/Hideout/` and `HideoutStashControl` → must not exist
6. `src/Misc/Makcu/` → must contain MakcuDevice.cs, MakcuManager.cs, MakcuNative.cs
7. `AimbotConfig` in `Config.cs` → must have `MakcuPort` and `FovDegrees`, not `TargetingMode`
8. `Config.Aimbot` → must be a direct property, not a shortcut to `MemWrites.Aimbot`
