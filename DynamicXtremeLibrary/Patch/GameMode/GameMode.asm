DXGameMode:
    CLI                       ; Enable IRQ 
    INC $13                   ; Increment frame number

if !ControllerOptimization
    JSL $008650|!rom
endif

    LDX $0100|!addr
    LDA.l GameModeTable,x
    BNE +
    JSR Init
+

	PHK
	PEA.w .jslrtsreturn-1
	PEA.w $0084CE|!rom
	JML $009322|!rom
.jslrtsreturn

if !KevinLMVRAMOptimization
    JSR VRAMOptimizer
endif

if !PaletteEffects
    incsrc "PalettesChangeGameMode.asm"
    incsrc "PaletteEffectsGameMode.asm"
elseif !PalettesChange
    incsrc "PalettesChangeGameMode.asm"
if !sa1
RTL
else
JML $008075|!rom
endif
else
JML $008075|!rom
endif

if !KevinLMVRAMOptimization
incsrc "VRAMOptimizerGameMode.asm"
endif
