# DynamicXTool

## Compatibility

Dynamic Xtreme features without a compatibility warning generally do not cause
incompatibility problems. These features are:

- `GraphicsChange`
- `PalettesChange`
- `DynamicPoses`
- `DrawingSystem`
- `PalettesEffects`
- `Free7E2000`
- `ControllerOptimization`
- `FixedColorOptimization`
- `YoshiFeatures`

This is not a guarantee that every combination of patches will work: another
resource can still use the same RAM, VRAM, DMA channels, or hooks. `YoshiFeatures`
also requires `PlayerFeatures`, whose compatibility restrictions apply below.

### Features with specific warnings

| Optional feature | Compatibility considerations |
| --- | --- |
| `PlayerFeatures` | Can conflict with LX5's powerups, Player 8x8 tiles DMAer, Player 32x32, and other systems that replace player graphics or palette handling. Player Revolution is compatible. Disable this feature if another player system requires those routines; `YoshiFeatures`, which depends on it, must also be disabled. |
| `StatusBarOptimization` | Optimizes the original status bar and conflicts with replacements such as LX5's DKC Style Status Bar, Minimalistic Status Bar, and Nuke Status Bar. Leave it disabled when using a custom status bar. |
| `KevinLMVRAMOptimization` | Integrates Kevin's Lunar Magic VRAM patch and should be used instead of installing the original patch separately. Its warning states that it cannot safely be uninstalled once installed: removing it may break the game. Decide whether to enable it before installation; to undo it, restore a backup from before it was installed. |

These features are optional, so you can leave the conflicting feature disabled
and continue using the other Dynamic Xtreme systems. Disabling an option does
not undo changes made by another patch, and the uninstall restriction for
`KevinLMVRAMOptimization` still applies.

### Free RAM usage

The source layout is defined in
[`DynamicXtremeLibrary/ASM/DXDefines.asm`](DynamicXtremeLibrary/ASM/DXDefines.asm).
The two main configurable bases are `!Variables` and `!Variables2`. Their actual
usage depends on enabled features and the mapper. The following are inclusive
ranges with all RAM-consuming features enabled and both transfer capacities at
their default value of 64:

| ROM configuration | `!Variables` block | Size | `!Variables2` block | Size | Total |
| --- | --- | --- | --- | --- | --- |
| LoROM, `Free7E2000` enabled | `$7E2000–$7E2500` | `$0501` / 1,281 bytes | `$7E2800–$7E3608` | `$0E09` / 3,593 bytes | 4,874 bytes |
| LoROM, `Free7E2000` disabled | `$7FB440–$7FB940` | `$0501` / 1,281 bytes | `$7F9C80–$7FAA88` | `$0E09` / 3,593 bytes | 4,874 bytes |
| SA-1 | `$40C000–$40C47E` | `$047F` / 1,151 bytes | `$418AFF–$419907` | `$0E09` / 3,593 bytes | 4,744 bytes |

The ranges above describe the bytes occupied by the two layouts, not every byte
between their bases. They were checked with Asar against the current source.
`Free7E2000` relocates vanilla graphics storage so Dynamic Xtreme can use the
LoROM `$7E2000` area. With it disabled, the fallback locations may overlap RAM
used by other common resources. SA-1 selects its own bases and forces
`Free7E2000` off.

The allocation breakdown makes configurations with fewer features calculable:

| Allocation | First block | Second block |
| --- | --- | --- |
| Timers and dynamic-transfer counters, always declared | 8 bytes | — |
| `DrawingSystem`, LoROM only | 130 bytes | — |
| `DynamicPoses` | 1,025 bytes | — |
| `PalettesChange` | 103 bytes | `1 + 7 × CGRAMTransferSize` = 449 bytes by default |
| `PalettesEffects`, with `PalettesChange` | — | 2,885 bytes |
| `PlayerFeatures` | 11 bytes | 2 bytes when `PalettesChange` is enabled |
| `YoshiFeatures` | 4 bytes | — |
| `GraphicsChange` | — | `1 + 4 × VRAMTransferSize` = 257 bytes by default |

Disabling features removes their conditional allocations and shifts later
labels. Enabling palette effects requires palette runtime support even when
there are no imported palettes. Increasing `!VRAMTransferSize` or
`!CGRAMTransferSize` increases the corresponding transfer arrays.

`DX_FreeRams` marks the end of the first block. `DX_FreeRams2` normally marks
the end of the second block, but in the LoROM fallback configuration it is
explicitly positioned at `!Variables3 = $7FAC82`; that label is not an additional
allocated block and does not indicate the actual end of the preceding arrays.

The file also references the existing fixed-color mirror at `$7E0701` (2 bytes)
and palette-write mirror at `$7E0703` (512 bytes) in LoROM. In SA-1 the declared
addresses are `$406701` and `$006703`, respectively. These are mirrors used by
the system, not additional `skip` allocations in the two free-RAM blocks above.

If a RAM conflict occurs, edit **`ASM/DXDefines.asm` in the Dynamic Xtreme
distribution** to relocate the appropriate bases to sufficiently large,
available regions in your ROM hack. In this repository that source is
`DynamicXtremeLibrary/ASM/DXDefines.asm`. Adjust the correct mapper/`Free7E2000`
branch, keep each layout within a valid address range for that mapper, and
preserve access requirements such as the bank and direct-page assumptions of
the routines using it. Then reinstall Dynamic Xtreme and reinsert its consumers
so they all use the updated addresses. Do not rely on edits to generated
`ExtraDefines/DXDefines.asm` or `Patch/ExtraDefines/DXDefines.asm`; those copies
are regenerated during installation.
