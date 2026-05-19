if read1($00FFD5) == $23
    if read1($00FFD7) == $0D ; full 6/8 mb sa-1 rom
		fullsa1rom
		!fullsa1 = 1
	else
		!fullsa1 = 0
		sa1rom
	endif
    !sa1 = 1
else
    lorom
    !sa1 = 0
endif

incsrc "./ExtraDefines/DXDefines.asm"
incsrc "DefinesFix.asm"

macro CleanOrg(address)
org <address>
    if read1(<address>) == $5c
        autoclean read3(<address>+1)
    endif
endmacro

incsrc "Features/Hijacks/ControllerOptimizationHijack.asm"
incsrc "Features/Hijacks/FixedColorOptimizationHijack.asm"
incsrc "Features/Hijacks/Free7E2000Hijack.asm"
incsrc "Features/Hijacks/MainHijacks.asm"
incsrc "Features/Hijacks/PlayerFeaturesHijack.asm"
incsrc "Features/Hijacks/StatusBarOptimizationHijack.asm"
incsrc "Features/Hijacks/VRAMOptimizerHijack.asm"
incsrc "Features/Hijacks/YoshiFeaturesHijack.asm"

reset freespaceuse
freecode
if !GraphicsChange || !PalettesChange 
prot Data_BufferTable
endif
if !Free7E2000
prot Data_GFX32,Data_GFX33
endif
if !DynamicPoses
prot Data_PoseSize
endif
if !DrawingSystem
prot Data_TableOffset
incsrc "GraphicRoutines/GraphicRoutineProts.asm"
endif
if !PalettesChange
prot Data_PaletteTable
endif
if !PalettesEffects
prot Data_PaletteEffectsTable
endif

incsrc "DynamicXtremeVector.asm"

GameModeTable:
    db $00,$00,$00,$00,$00,$00,$01,$01
    ;  g00,g01,g02,g03,g04,g05,g06,g07
    db $01,$01,$01,$00,$00,$02,$02,$00
    ;  g08,g09,g0A,g0B,g0C,g0D,g0E,g0F
    db $00,$00,$01,$01,$01,$01,$00,$00
    ;  g10,g11,g12,g13,g14,g15,g16,g17
    db $00,$00,$00,$00,$01,$01,$01,$01
    ;  g18,g19,g1A,g1B,g1C,g1D,g1E,g1F
    db $00,$00,$00,$00,$01,$01,$00,$00
    ;  g20,g21,g22,g23,g24,g25,g26,g27
    db $00,$00,$00,$00,$00,$00,$00,$00
    ;  g28,g29,g2A,g2B,g2C,g2D,g2E,g2F

if !GraphicsChange || !PalettesChange
    incsrc "Init.asm"
    incsrc "Main.asm"
    incsrc "NMI/NMIMainRoutine.asm"
endif

if !GraphicsChange || !PalettesChange || !ControllerOptimization || !KevinLMVRAMOptimization
    incsrc "GameMode/GameMode.asm"
endif

namespace nested on
namespace Routines
    incsrc "Features/Routines/AllowedGameMode.asm"
if !DrawingSystem
    incsrc "Features/Routines/DrawingSystemRoutines.asm"
endif
if !DynamicPoses
    incsrc "Features/Routines/DynamicPosesRoutines.asm"
endif
if !PalettesChange
    incsrc "Features/Routines/PalettesChangeRoutines.asm"
endif
if !PalettesEffects
    incsrc "Features/Routines/PaletteEffectsRoutines.asm"
endif
namespace off
namespace nested off

if !PlayerFeatures
incsrc "Features/Implementation/PlayerFeatures.asm"
endif
if !YoshiFeatures
incsrc "Features/Implementation/YoshiFeatures.asm"
endif
if !Free7E2000
incsrc "Features/Implementation/Free7E2000Features.asm"
endif
if !DrawingSystem
namespace GraphicRoutines
incsrc "Features/Implementation/DrawingSystem.asm"
incsrc "GraphicRoutines/GraphicRoutineIncludes.asm"
namespace off
endif

namespace Data
if !GraphicsChange || !PalettesChange
    incsrc "Data/BufferData.asm"
endif
if !Free7E2000
freedata
GFX32:
	incbin "Data/GFX32.bin"
		
freedata
GFX33:
	incbin "Data/GFX33.bin"
endif
if !DynamicPoses
freedata
    incsrc "Data/DynamicPoseData.asm"
endif
if !DrawingSystem
freedata
    incsrc "Data/PoseData.asm"
endif
if !PalettesChange
    incsrc "Data/PaletteData.asm"
endif
if !PalettesEffects
    incsrc "Data/PaletteEffectsData.asm"
endif
namespace off

print freespaceuse
