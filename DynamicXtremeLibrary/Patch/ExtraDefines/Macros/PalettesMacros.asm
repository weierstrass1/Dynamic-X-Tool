macro LoadPaletteIDAndAddress()
;Input:
;   A = version, 8 bits
;   $0E = Palette Context ID, 16 bits
;Output:
;   $00 = Palette ID
;   $02-$04 = Palette Address
    REP #$31
    AND #$00FF
    ADC $0E
    STA $00
    ASL
    CLC
    ADC $00
    TAX

    LDA.l !PaletteAddrTables,x
    STA $02
    LDA.l !PaletteAddrTables+1,x
    STA $03

    LDA $00
    ASL
    TAX
    LDA.l !PaletteIDTables,x
    STA $00
    SEP #$30
endmacro

macro PaletteAssignment(sprite, palette, paletteAssignment, paletteOption, paletteVersion, paletteLastVersion, manualIDGroup, manualID)
?DXPaletteAssignment:
if !PalettesChange == 0
    LDA <palette>
    BPL ?+

    LDA <manualIDGroup>
    STA $01
    LDA <manualID>
    STA $00
    LDA <paletteOption>
    AND #$70
    XBA
    LDA #$20
    JSL !AssignPalette
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    STA <palette>
?+
    SEC
RTL
else
    STA $0E
    SEP #$20

    LDA <paletteAssignment>
    AND #$30
    CMP #$20
    BNE ?.AutoOrManualAssignment
?.NoAssignment
    XBA
    LDA <palette>
    BPL ?+

    LDA <manualIDGroup>
    STA $01
    LDA <manualID>
    STA $00
    LDA <paletteOption>
    AND #$70
    XBA
    JSL !AssignPalette
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    STA <palette>
?+
    SEC
RTL
?.AutoOrManualAssignment
    JSL !AllowedGameMode
    BNE ?.Start
?.CanStart
    LDA <palette>
	BPL ?..AlreadyLoaded
?..NotLoaded
    CLC
RTL
?..AlreadyLoaded
    AND #$0E
    LSR
    TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
if !PalettesEffects == 1
    LDA <paletteOption>
    AND #$80
    BEQ ?+

    LDA <palette>
    LSR
    TAX

    LDA DX_PPU_CGRAM_SPEffectLoaded
    AND.l ?.BitSetter,x
    BNE ?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    CLC
RTL
?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
?+
endif
	SEC
RTL
?.Start

    LDA <paletteVersion>
    if <sprite>
    %DXLoadPaletteIDAndAddress()
    LDX !SpriteIndex
    else
    %LoadPaletteIDAndAddress()
    endif

    LDA <palette>
    BMI ?..Assign

    LDA <paletteVersion>
    CMP <paletteLastVersion>
    BNE ?..Assign

    LDA <palette>
    LSR
    TAX

    LDA DX_Dynamic_Palettes_DisableTimer+$08,x
    BEQ ?..Assign

    if <sprite> != 0
    LDX !SpriteIndex
    endif

?..PaletteLoaded

    LDA <palette>
    AND #$0E
    LSR
    TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
if !PalettesEffects == 1
    LDA <paletteOption>
    AND #$80
    BEQ ?+

    LDA <palette>
    LSR
    TAX

    LDA DX_PPU_CGRAM_SPEffectLoaded
    AND.l ?.BitSetter,x
    BNE ?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    CLC
RTL
?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
?+
endif
	SEC
RTL

?..Assign
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteAssignment>
    AND #$30
    CMP #$10
    BNE ?+

    LDA $00
    STA $0E
    LDA $01
    STA $0F

    LDA <manualIDGroup>
    STA $01
    LDA <manualID>
    STA $00
?+
    STZ $D5

    LDA <paletteOption>
    BIT #$80
    BEQ ?+
    DEC $D5
?+
    AND #$70
    XBA
    LDA <paletteAssignment>
    JSL !AssignPalette
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    BCS ?...Success
RTL
?...Success
    STA $0C

    CPY #$00
    BNE ?+
    STA <palette>
    LDA <paletteVersion>
    STA <paletteLastVersion>
    SEC
RTL
?+
    LDA <paletteAssignment>
    AND #$30
    CMP #$10
    BNE ?+

    LDA $0E
    STA $00
    LDA $0F
    STA $01
?+
    LDA $0C
    LSR
    TAX

if !PalettesEffects == 1
    LDA DX_Dynamic_Palettes_GlobalSPEnable
	AND.l ?.BitClearer,x
	STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
	AND.l ?.BitClearer,x
	STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded

    LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
    AND.l ?.BitClearer,x
    STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded

    LDA DX_PPU_CGRAM_SPEffectLoaded
    AND.l ?.BitClearer,x
    STA DX_PPU_CGRAM_SPEffectLoaded

    LDA DX_Dynamic_Palettes_EffectEnabled
    AND.l ?.BitClearer,x
    STA DX_Dynamic_Palettes_EffectEnabled

    TXA
    ASL
    TAX

    LDA #$FF
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$11,x
endif

?..Update
    if <sprite> != 0
    LDX !SpriteIndex
    endif

if !PalettesEffects == 1
    LDA <paletteOption>
    AND #$80
    BEQ ?.NoPalEffect

    LDA $0C
    STA <palette>
    LSR
    TAX

    ASL
    ASL
    ASL
    ASL
    INC A
    ASL
    STA $00
    STZ $01

    LDA DX_Dynamic_Palettes_GlobalSPEnable
	ORA.l ?.BitSetter,x
	STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_Dynamic_Palettes_EffectEnabled
    ORA.l ?.BitSetter,x
    STA DX_Dynamic_Palettes_EffectEnabled

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    LDA $00
    CLC
    ADC #$1C
    TAX
    LDY #$1C
    REP #$21
?-
    LDA [$02],y
    STA.w DX_PPU_CGRAM_PaletteCopy+$0100,x
    DEX
    DEX
    DEY
    DEY
    BPL ?-
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteVersion>
    STA <paletteLastVersion>
    CLC
RTL

?.NoPalEffect
endif
    REP #$21
    LDA DX_Dynamic_CurrentDataSend
    ADC #$001E
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BEQ ?+
    BCC ?+

    LDA #$80
    CMP <palette>
RTL
?+
    LDA $0C
    STA <palette>
    ASL
    ASL
    ASL
    ORA #$81
    STA $00

    LDA <palette>
    LSR
    TAX
    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    %ForcedTransferToCGRAM("$00", "$02", "$04", "#$001E")

    LDA $00
    AND #$7F
    ASL
    CLC
    ADC #$1C
    TAX
    LDY #$1C
    REP #$21
?-
    LDA [$02],y
    STA.w DX_PPU_CGRAM_PaletteCopy+$0100,x
    STA.w $0A05|!addr,x
    DEX
    DEX
    DEY
    DEY
    BPL ?-
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteVersion>
    STA <paletteLastVersion>
    SEC
RTL

if !PalettesEffects == 1
?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
?.BitClearer
    db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F
endif
endif
endmacro

macro StandardSpritePaletteAssignment(type, manualIDGroup, manualID)
    %PaletteAssignment(1, "!<type>Palette,x", "!<type>PaletteAssignment,x", "!<type>PaletteOption,x", "!<type>Version,x", "!<type>LastVersion,x", "<manualIDGroup>", "<manualID>")
endmacro

macro PaletteAssignmentNoUpload(sprite, palette, paletteAssignment, paletteOption, paletteVersion, paletteLastVersion, manualIDGroup, manualID)
?DXPaletteAssignment:
if !PalettesChange == 0
    LDA <palette>
    BPL ?+

    LDA <manualIDGroup>
    STA $01
    LDA <manualID>
    STA $00
    LDA <paletteOption>
    AND #$70
    XBA
    LDA #$20
    JSL !AssignPalette
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    STA <palette>
?+
    LDY #$00
    SEC
RTL
else
    STA $0E
    SEP #$20

    LDA <paletteAssignment>
    AND #$30
    CMP #$20
    BNE ?.AutoOrManualAssignment
?.NoAssignment
    XBA
    LDA <palette>
    BPL ?+

    LDA <manualIDGroup>
    STA $01
    LDA <manualID>
    STA $00
    LDA <paletteOption>
    AND #$70
    XBA
    JSL !AssignPalette
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    STA <palette>
?+
    LDY #$00
    SEC
RTL
?.AutoOrManualAssignment
    JSL !AllowedGameMode
    BNE ?.Start
?.CanStart
    LDA <palette>
	BPL ?..AlreadyLoaded
?..NotLoaded
    LDY #$00
    CLC
RTL
?..AlreadyLoaded
    AND #$0E
    LSR
    TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
if !PalettesEffects == 1
    LDA <paletteOption>
    AND #$80
    BEQ ?+

    LDA <palette>
    LSR
    TAX

    LDA DX_PPU_CGRAM_SPEffectLoaded
    AND.l ?.BitSetter,x
    BNE ?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDY #$00
    CLC
RTL
?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
?+
endif
    LDY #$00
	SEC
RTL
?.Start

    LDA <paletteVersion>
    if <sprite>
    %DXLoadPaletteIDAndAddress()
    LDX !SpriteIndex
    else
    %LoadPaletteIDAndAddress()
    endif

    LDA <palette>
    BMI ?..Assign

    LDA <paletteVersion>
    CMP <paletteLastVersion>
    BNE ?..Assign

    LDA <palette>
    LSR
    TAX

    LDA DX_Dynamic_Palettes_DisableTimer+$08,x
    BEQ ?..Assign

    if <sprite> != 0
    LDX !SpriteIndex
    endif

?..PaletteLoaded

    LDA <palette>
    AND #$0E
    LSR
    TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
if !PalettesEffects == 1
    LDA <paletteOption>
    AND #$80
    BEQ ?+

    LDA <palette>
    LSR
    TAX

    LDA DX_PPU_CGRAM_SPEffectLoaded
    AND.l ?.BitSetter,x
    BNE ?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDY #$00
    CLC
RTL
?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
?+
endif
    LDY #$00
	SEC
RTL

?..Assign

    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteAssignment>
    AND #$30
    CMP #$10
    BNE ?+

    LDA $00
    STA $0E
    LDA $01
    STA $0F

    LDA <manualIDGroup>
    STA $01
    LDA <manualID>
    STA $00
?+
    STZ $D5

    LDA <paletteOption>
    BIT #$80
    BEQ ?+
    DEC $D5
?+
    AND #$70
    XBA
    LDA <paletteAssignment>
    JSL !AssignPalette
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    BCS ?...Success
    LDY #$00
RTL
?...Success
    STA $0C
    STA <palette>

    LDA <paletteVersion>
    STA <paletteLastVersion>

    CPY #$00
    BNE ?+
    LDY #$00
    SEC
RTL
?+
    LDA <paletteAssignment>
    AND #$30
    CMP #$10
    BNE ?+

    LDA $0E
    STA $00
    LDA $0F
    STA $01
?+
    LDA $0C
    LSR
    TAX

if !PalettesEffects == 1
    LDA DX_Dynamic_Palettes_GlobalSPEnable
	AND.l ?.BitClearer,x
	STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
	AND.l ?.BitClearer,x
	STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded

    LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
    AND.l ?.BitClearer,x
    STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded

    LDA DX_PPU_CGRAM_SPEffectLoaded
    AND.l ?.BitClearer,x
    STA DX_PPU_CGRAM_SPEffectLoaded

    LDA DX_Dynamic_Palettes_EffectEnabled
    AND.l ?.BitClearer,x
    STA DX_Dynamic_Palettes_EffectEnabled

    TXA
    ASL
    TAX

    LDA #$FF
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$11,x
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$10,x
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$11,x
endif

    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDY #$01
    SEC
RTL
if !PalettesEffects == 1
?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
?.BitClearer
    db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F
endif
endif
endmacro

macro StandardSpritePaletteAssignmentNoUpload(type, manualIDGroup, manualID)
    %PaletteAssignmentNoUpload(1, "!<type>Palette,x", "!<type>PaletteAssignment,x", "!<type>PaletteOption,x", "!<type>Version,x", "!<type>LastVersion,x", "<manualIDGroup>", "<manualID>")
endmacro

;X = Palette x2
;Y = 1 force do the effect from the beggining, 0 do the effect only if it is needed.
;A 16 bits = Palette Effect
;$02-$04 = Base Palette Address
macro ConcatenateRGBPaletteEffectToGlobal(sprite, paletteOption, palette)
?ConcatenatePaletteEffectToGlobal:
	STA $F2
	CMP DX_PPU_CGRAM_LocalEffectID+$10,x
    BEQ ?+
    SEP #$20
    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
    AND.l ?.BitChecker,x
    REP #$20
	BEQ ?.DoEffectWithBase
	JMP ?.DoEffect 
?+
	REP #$10
	LDA DX_Dynamic_Palettes_LastGlobalEffectID
	BEQ ?+
	DEC A
	TAX

	LDA !PaletteEffectsTypes,x
	AND #$0001
	BEQ ?+
	SEP #$10
	LDA DX_Dynamic_Palettes_GlobalEffectID
	PHP
	LDA $F2
	PLP
	BEQ ?.DoEffect
	SEP #$20
    if <sprite> != 0
    LDX !SpriteIndex
    endif
	SEC
RTL
?+
	LDA DX_Dynamic_Palettes_GlobalEffectID
	BEQ ?+
	DEC A
	TAX

	LDA !PaletteEffectsTypes,x
	SEP #$10
	AND #$0001
	BEQ ?+ 
	LDA $F2
	BRA ?.DoEffect
?+
	SEP #$30
    if <sprite> != 0
    LDX !SpriteIndex
    endif
	SEC
RTL
?.DoEffectWithBase
	STA DX_PPU_CGRAM_LocalEffectID+$10,x
	SEP #$20

    LDA.l ?.BitChecker,x
    ORA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
    STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded

    TXA
    ASL
    ASL
    ASL
	REP #$21
	AND #$00FF
    ADC #$0081
	STA $F0
	ASL
	CLC
	ADC $F0
	CLC
	ADC #DX_PPU_CGRAM_LocalEffectBuffer
	STA $05
	SEP #$20

	%SetRGBBase($02,$04,$05,"#DX_PPU_CGRAM_LocalEffectBuffer>>16","s",#$000F)

	BRA ?+
?.DoEffect
	STA DX_PPU_CGRAM_LocalEffectID+$10,x
	SEP #$20

    TXA
    ASL
    ASL
    ASL
    CLC
    ADC #$81
	STA $F0
    STZ $F1
?+
    if <sprite> != 0
    LDX !SpriteIndex
    endif
	LDA <palette>
	LSR
	TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

	LDA DX_Dynamic_Palettes_Updated+$08,x
	BEQ ?+
    if <sprite> != 0
    LDX !SpriteIndex
    endif
	SEC
RTL
?+
	LDA #$01
	STA DX_Dynamic_Palettes_Updated+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
	LDA <paletteOption>
	AND #$80
	BNE ?..Onlyeffect
	JMP ?..EffectAndMerge
?..Onlyeffect
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	SEP #$20
	BNE ?+


	%DoEffectAndMerge($F2,"#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_PaletteCopy","#DX_PPU_CGRAM_PaletteCopy>>16",$F0,#$000F)

	JMP ?...Finish
?+

	%DoEffect($F2,"#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16",$F0,#$000F)

	REP #$30
	LDA DX_Dynamic_Palettes_GlobalEffectID
	DEC A
	TAX

	SEP #$20
	LDA !PaletteEffectsTypes,x
	SEP #$30
	AND #$01
	BEQ ?...Finish

	%RGBToHSL("#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16",$F0,#$000F)

    if <sprite> != 0
    LDX !SpriteIndex
    endif
	LDA <palette>
	LSR
	TAX

    LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
    ORA.l ?.BitSetter,x
    STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded

?...Finish

    if <sprite> != 0
    LDX !SpriteIndex
    endif
	LDA <palette>
	LSR
	TAX

    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
    ORA.l ?.BitSetter,x
    STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded

    LDA DX_Dynamic_Palettes_GlobalSPEnable
	ORA.l ?.BitSetter,x
	STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_PPU_CGRAM_SPEffectLoaded
    AND.l ?.BitClearer,x
    STA DX_PPU_CGRAM_SPEffectLoaded

    LDA DX_Dynamic_Palettes_EffectEnabled
    ORA.l ?.BitSetter,x
    STA DX_Dynamic_Palettes_EffectEnabled

    TXA
    ASL
    TAX

    LDA #$FF
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$11,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
	SEC
RTL

?..EffectAndMerge
	%DoEffectAndMerge($F2,"#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16",$F0,#$000F)

	LDA $F0
	REP #$21
	AND #$00FF
	ASL
	ADC #DX_PPU_CGRAM_PaletteWriteMirror
	STA $05
	SEP #$20

	%TransferToCGRAM($F0,$05,"#DX_PPU_CGRAM_PaletteWriteMirror>>16", #$001E)
	BCS ?+
RTL
?+
    LDA $F0
    AND #$7F
    ASL
    CLC
    ADC #$1C
    TAY
    LDX #$1C
    REP #$21
?-
    LDA DX_PPU_CGRAM_PaletteWriteMirror,x
    STA.w DX_PPU_CGRAM_PaletteCopy+$0100,y
    STA.w $0A05|!addr,y
    DEY
    DEY
    DEX
    DEX
    BPL ?-
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif
	SEC
RTL
?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
?.BitClearer
    db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F
?.BitChecker
    db $01,$01,$02,$02,$04,$04,$08,$08,$10,$10,$20,$20,$40,$40,$80,$80
endmacro

macro ReplacePalette(sprite, paletteOption, palette)
?ReplacePalette:
    if <sprite> != 0
    LDX !SpriteIndex
    endif

if !PalettesEffects == 1
    LDA <paletteOption>
    AND #$80
    BEQ ?.NoPalEffect

    LDA <palette>
    LSR
    TAX

    ASL
    ASL
    ASL
    ASL
    INC A
    ASL
    STA $00
    STZ $01

    LDA DX_Dynamic_Palettes_GlobalSPEnable
	ORA.l ?.BitSetter,x
	STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_Dynamic_Palettes_EffectEnabled
    ORA.l ?.BitSetter,x
    STA DX_Dynamic_Palettes_EffectEnabled

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    LDA $00
    CLC
    ADC #$1C
    TAX
    LDY #$1C
    REP #$21
?-
    LDA [$02],y
    STA.w DX_PPU_CGRAM_PaletteCopy+$0100,x
    DEX
    DEX
    DEY
    DEY
    BPL ?-
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    CLC
RTL

?.NoPalEffect
endif
    REP #$21
    LDA DX_Dynamic_CurrentDataSend
    ADC #$001E
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BEQ ?+
    BCC ?+

    LDA #$80
    CMP <palette>
RTL
?+
    LDA <palette>
    ASL
    ASL
    ASL
    ORA #$81
    STA $00

    LDA <palette>
    LSR
    TAX
    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    %ForcedTransferToCGRAM("$00", "$02", "$04", "#$001E")

    LDA $00
    AND #$7F
    ASL
    CLC
    ADC #$1C
    TAX
    LDY #$1C
    REP #$21
?-
    LDA [$02],y
    STA.w DX_PPU_CGRAM_PaletteCopy+$0100,x
    STA.w $0A05|!addr,x
    DEX
    DEX
    DEY
    DEY
    BPL ?-
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    SEC
RTL

if !PalettesEffects == 1
?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
?.BitClearer
    db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F
endif
endmacro
