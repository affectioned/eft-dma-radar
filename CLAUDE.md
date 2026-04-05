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
- Delete `IMemWriteFeature.cs`, `MemWriteFeature.cs`, `ScatterWriteHandle.cs`
- `Config.Aimbot` must be a direct `[JsonInclude]` property — never `=> MemWrites.Aimbot`
- Remove `MemWritesConfig` class and `Config.MemWrites` property entirely from `Config.cs`
- Remove any "Memory Writing" tab/button from the toolbar and all panels

### WebRadar
- Delete `src/Web/WebRadar/` and anything named `*WebRadar*`
- Remove "Web Radar Server" expander from `GeneralSettingsControl.xaml`
- Remove all `btnWebRadarStart`, `chkWebRadarUPnP`, `lblWebRadarLink`, `txtWebRadarPort` event handlers from `GeneralSettingsControl.xaml.cs`

### HideoutStash / HideoutManager
- Delete `src/Tarkov/Hideout/`, `HideoutStashControl.xaml/.cs`, `StashDogtagDumper.cs`
- Remove `btnHideoutStash` toolbar button from `MainWindow.xaml`

### QuestPlanner
- Delete `src/Tarkov/QuestPlanner/`
- Remove `btnQuestPlanner` toolbar button from `MainWindow.xaml`
- Remove `UpdateQuestPlannerRaidState()` and related fields from `MainWindow.xaml.cs`

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

### Aimbot toolbar tab wiring (must survive every merge)

After any merge, verify all three pieces are present or restore them:

1. **`MainWindow.xaml`** — toolbar button and panel canvas:
   ```xml
   <Button x:Name="btnAimbot" ... hc:IconElement.Geometry="{StaticResource CrosshairGeometry}" Click="btnAimbot_Click"/>
   ```
   ```xml
   <Canvas Name="AimbotCanvas" Panel.ZIndex="1000">
       <Border x:Name="AimbotPanel" Width="350" Height="450" ...>
           <local:AimbotControl x:Name="AimbotControl"/>
       </Border>
   </Canvas>
   ```

2. **`MainWindow.xaml.cs`** — `_panels` registration and click handler:
   ```csharp
   ["Aimbot"] = new PanelInfo(AimbotPanel, AimbotCanvas, "Aimbot", 200, 200)
   ```
   ```csharp
   private void btnAimbot_Click(object sender, RoutedEventArgs e) => TogglePanelVisibility("Aimbot");
   ```

3. **`src/UI/Resources/IconResources.xaml`** — `CrosshairGeometry` key must exist.

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
9. `MainWindow.xaml` → must have `btnAimbot` toolbar button and `AimbotCanvas`/`AimbotPanel` with `<local:AimbotControl>`
10. `MainWindow.xaml.cs` → `_panels` must contain `["Aimbot"]` entry and `btnAimbot_Click` handler must exist
11. `MainWindow.xaml` → must NOT have `btnQuestPlanner` or `btnHideoutStash` toolbar buttons
