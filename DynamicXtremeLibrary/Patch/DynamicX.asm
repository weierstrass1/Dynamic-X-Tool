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

incsrc "./ExtraDefines/DynamicXDefines.asm"

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
if !Free7E2000 == 1
prot Data_GFX32,Data_GFX33
endif
if !DynamicPoses == 1
prot Data_PoseSize
endif
if !DrawingSystem == 1
prot Data_TableOffset
endif

incsrc "DynamicXVector.asm"

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

Start:

    PHP
    PHX
    PHY

    JSR Init

    PLY
    PLX
    PLP

    LDA #$03                  ;\ Set OAM name base to #$03, clear the name and allow 8x8 and 16x16 sprites
    STA $2101                 ;/
    INC $10 
    JML $00806B

if !GraphicsChange || !PalettesChange || !ControllerOptimization || !KevinLMVRAMOptimization
    incsrc "GameMode/GameMode.asm"
endif

namespace Routines
    incsrc "Routines/AllowedGameMode.asm"
    incsrc "Routines/DrawingSystemRoutines.asm"
    incsrc "Routines/DynamicPosesRoutines.asm"
    incsrc "Routines/PalettesChangeRoutines.asm"
    incsrc "Routines/PaletteEffectsRoutines.asm"
namespace off

if !PlayerFeatures == 1
incsrc "Features/Implementation/PlayerFeatures.asm"
endif
if !YoshiFeatures == 1
incsrc "Features/Implementation/YoshiFeatures.asm"
endif
if !Free7E2000 == 1
incsrc "Features/Implementation/Free7E2000Features.asm"
endif

namespace Data
if !Free7E2000 == 1
freedata
GFX32:
	incbin "GFX32.bin"
		
freedata
GFX33:
	incbin "GFX33.bin"
endif
if !DynamicPoses == 1
freedata
    incsrc "Data/DynamicPosesData.asm"
endif
if !DrawingSystem == 1
freedata
    incsrc "Data/PoseData.asm"
endif
namespace off
