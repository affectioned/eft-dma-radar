# Lone's EFT DMA Radar (affectioned fork)

> **Educational Disclaimer:** This project is intended solely for educational and research purposes — to study DMA (Direct Memory Access) hardware, memory reading techniques, and game overlay systems. It is not intended to be used in live online gameplay or in any way that violates the terms of service of Escape from Tarkov or any other software. The authors do not condone cheating and accept no responsibility for misuse of this software.

<img width="512" height="512" alt="image" src="https://github.com/user-attachments/assets/e2bb2567-539c-43c7-87c5-1ffe59e97d4d" />

## What is this?
- This is an up-to-date build of Lone DMA EFT/Arena Radar. This is a standalone copy of the software with *No Restrictions*.

## Fork Changes

This fork makes the following changes on top of the upstream project:

### Hardware Aimbot (Makcu)
A hardware-based aimbot has been added using [makcu-cpp](https://github.com/K4HVH/makcu-cpp). Mouse movement is delivered physically through the Makcu USB HID device — no software mouse injection, no kernel drivers. Requires `makcu-cpp.dll` — see **Required DLLs** below.

### Memory Writes Removed — ESP Only (non-hardware)
All in-process memory writing features have been completely removed. This includes no-recoil, speed hacks, chams, and every other feature that wrote to game memory. The only remaining memory interaction is reading game state for the radar overlay, plus the hardware aimbot above which operates entirely through the Makcu device.

### Privacy Improvements
- **eft-api.tech removed** — All calls to `eft-api.tech` have been eliminated. This endpoint was used by the upstream project for various lookups and telemetry.
- **Analytics removed** — Any other analytics or tracking calls have been stripped out.
- **tarkov.dev only** — Item/loot data is fetched anonymously from [tarkov.dev](https://tarkov.dev), a community-run open API. No account, key, or identifying information is sent.

### Dependency & Build Improvements
- **No bundled DLLs** — All native DLLs (MemProcFS, LeechCore, etc.) have been removed from the repository. Users download them directly from their official sources, meaning you always get unmodified, verifiable binaries rather than trusting pre-bundled copies.
- **VmmSharp via NuGet** — The VmmSharp managed wrapper is now referenced as a NuGet package instead of a bundled/compiled binary. This keeps the dependency versioned, auditable, and automatically resolvable by the .NET toolchain.

## Required DLLs

The following native DLLs are **not included** in the repository and must be placed in the same directory as the compiled `.exe` before running.

### MemProcFS / LeechCore
Download the latest release from [MemProcFS Releases](https://github.com/ufrisk/MemProcFS/releases) and copy these files next to the exe:

| File | Description |
|------|-------------|
| `vmm.dll` | MemProcFS core |
| `leechcore.dll` | LeechCore memory acquisition |
| `leechcore_driver.dll` | LeechCore driver |
| `tinylz4.dll` | LZ4 compression |
| `FTD3XX.dll` | FTDI FT60x USB 3.0 bridge driver |
| `dbghelp.dll` | Windows Debug Help Library |

All of the above are included in the MemProcFS release archive.

### Makcu (hardware aimbot)
If you intend to use the hardware aimbot, download `makcu-cpp.dll` from the [makcu-cpp releases page](https://github.com/K4HVH/makcu-cpp/releases) and place it next to the exe. Without this file the aimbot tab will report "not connected" and the feature is fully disabled — the rest of the radar works normally.

| File | Description |
|------|-------------|
| `makcu-cpp.dll` | Makcu C API — physical HID mouse control |

### Visual C++ Runtime
Download **Visual C++ Redistributable 2015–2022 (x64)** from [Microsoft](https://aka.ms/vs/17/release/vc_redist.x64.exe).
Installing it system-wide is sufficient; copying `vcruntime140.dll` manually is only needed for a fully portable setup.

---

## How do I start using this?
1. Download & extract the solution
2. Open solution with visual studio
3. Publish the `eft-dma-radar` or `arena-dma-radar` project
4. Place all required DLLs listed above next to the published `.exe`
5. Run `eft-dma-radar.exe` or `arena-dma-radar.exe` depending on what you've published

## Arena
- Arena is supported and will be maintained going forward.

## Donations
- If you would like to donate to x0m:
  - Paypal: https://www.paypal.me/eftx0m?locale.x=en_NZ
  - BTC: `1AzMqjpjaN5fyGQgZTByRqA2CzKHQSXkMr`
  - LTC: `LWi2mP6GaDQbhDAzs4swiSEEowETRqCcLZ`
  - ETH: `0x6fe7aee467b63fde7dbbf478dce6a0d7695ae496`
  - USDT: `TYNZr9FL5dVtk1K5D5AwbiWt4UMbu9A7E3`

## Special Thanks
- Lone Survivor
  - Open sourcing his `lone-dma` project & allowing us to extend its functionality significantly, none of this was possible without him
   - [cant confirm if your crypto addresses are correct so i don't wanna add them]
- Mambo
  - Helping with quite a lot of things & especially stuff I didnt wanna do
   - Paypal: https://paypal.me/MamboNoob?country.x=CA&locale.x=en_US
   - BTC: `bc1qgw9v6xtwxqhtsuuge720lr5vrhfv596wqml2mk`
- Marazm
  - Keeping EFT & Arena maps updated
   - https://boosty.to/dma_maps/donate
   - USDT: `TWeRAuxsCFa8BHLZZbkz9aUHJcZwnGkiJx`
   - BTC: `bc1q32enxjvfvzp30rpm39uzgwpdxcl57l264reevu`
- xiaopaoliaofwen
  - Various contributions & actively maintaining the 'main' fork
    - https://buymeacoffee.com/kekmate

## Contact
- For any inquiries or assistance, feel free to join the [EFT Educational Resources Discord Server](https://discord.gg/hxUhJHWuap). Please confine discussion to the [Lone DMA Section](https://discord.com/channels/1218731239599767632/1342207117704036382).
- Report any major outages (after a game update,etc.) by opening an [Issue](https://github.com/Lone83427/lone-eft-dma-radar/issues). Please be *as detailed as possible*, don't just say "it's broke".
  - This is **NOT** for feature requests or suggestions.
  - Misuse of the Issues category will result in the privilege being revoked for that user.
  - If misuse is rampant, the ability to open Issues may be disabled for all users..
