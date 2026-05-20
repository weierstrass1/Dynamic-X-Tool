if !GraphicsChange || !PalettesChange

org $00821D
	BRA MarioGFX_continue
	dl Vector
org $00823A
MarioGFX:
if !PlayerFeatures == 0
.continue
	if read1($00823C) == $58
		JSR $A300
	endif
else
	BRA .continue
	db "X"
.continue
endif

org $00823D
	autoclean JML DXBaseHijack1
	db "DX"

org $0082D7
	autoclean JML DXBaseHijack2
elseif read2($008241) == $5844
org $00821D
	db $20,$00,$A3,$80,$1B
org $00823A
	db $20,$00,$A3
%CleanOrg($00823D)
	db $20,$D2,$85,$20,$49,$84,$20,$50,$86
%CleanOrg($0082D7)
	db $20,$00,$A3,$2C,$9B,$0D
endif

if !GraphicsChange || !PalettesChange || !ControllerOptimization || !KevinLMVRAMOptimization
org $00806F
	autoclean JML DXGameMode
	db "DX"
elseif read2($008073) == $5844
%CleanOrg($00806F)
	db $58,$E6,$13,$20,$22,$93
endif
