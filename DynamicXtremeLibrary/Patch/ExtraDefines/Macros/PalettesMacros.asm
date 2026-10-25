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

macro PaletteAssignmentCommon(noUpload, sprite, palette, paletteAssignment, paletteOption, paletteVersion, paletteLastVersion, manualIDGroup, manualID)
?DXPaletteAssignmentCommon:
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
    if <noUpload> != 0
    LDY #$00
    endif
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
    if <noUpload> != 0
    LDY #$00
    endif
    SEC
RTL
?.AutoOrManualAssignment
    JSL !AllowedGameMode
    BNE ?.Start
?.CanStart
    LDA <palette>
	BPL ?..AlreadyLoaded
?..NotLoaded
    if <noUpload> != 0
    LDY #$00
    endif
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

    TXA
    ASL
    TAX

    REP #$20
    LDA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
    INC A
    CMP #$0001
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    if <noUpload> != 0
    LDY #$00
    endif
RTL
?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
?+
endif
    if <noUpload> != 0
    LDY #$00
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

    TXA
    ASL
    TAX

    REP #$20
    LDA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
    INC A
    CMP #$0001
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    if <noUpload> != 0
    LDY #$00
    endif
RTL
?++
    if <sprite> != 0
    LDX !SpriteIndex
    endif
?+
endif
    if <noUpload> != 0
    LDY #$00
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
    if <noUpload> != 0
    LDY #$00
    endif
RTL
?...Success
    STA $0C

    CPY #$00
    BNE ?+
    STA <palette>
    LDA <paletteVersion>
    STA <paletteLastVersion>
    if <noUpload> != 0
    LDY #$00
    endif
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

    LDA DX_PPU_CGRAM_SPBaseLoaded
    AND.l ?.BitClearer,x
    STA DX_PPU_CGRAM_SPBaseLoaded

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

if <noUpload> == 0
    %PaletteAssignmentUploadTail(<sprite>, "<palette>", "<paletteOption>", "<paletteVersion>", "<paletteLastVersion>")
else
    %PaletteAssignmentNoUploadTail(<sprite>, "<palette>", "<paletteVersion>", "<paletteLastVersion>")
endif

if !PalettesEffects == 1
?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
?.BitClearer
    db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F
endif
endif
endmacro

macro PaletteAssignmentUploadTail(sprite, palette, paletteOption, paletteVersion, paletteLastVersion)
?DXPaletteAssignmentUploadTail:
    if <sprite> != 0
    LDX !SpriteIndex
    endif

if !PalettesEffects
    LDA <paletteOption>
    AND #$80
    BEQ ?.NoPalEffect

    LDA $0C
    STA <palette>
    ASL
    ASL
    ASL
    ORA #$81
    STA $00

    %TransferPaletteData($02, "$00", $1E, 1)
    SEP #$30

    LDA $0C
    LSR
    TAX

    LDA DX_Dynamic_Palettes_GlobalSPEnable
    ORA.l ?.BitSetter,x
    STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_Dynamic_Palettes_EffectEnabled
    ORA.l ?.BitSetter,x
    STA DX_Dynamic_Palettes_EffectEnabled

    LDA DX_PPU_CGRAM_SPBaseLoaded
    ORA.l ?.BitSetter,x
    STA DX_PPU_CGRAM_SPBaseLoaded

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteVersion>
    STA <paletteLastVersion>
    CLC
RTL

?.NoPalEffect
endif
    LDA $0C
    ASL
    ASL
    ASL
    ORA #$81
    STA $00

    %TransferPaletteData($02, "$00", $1E, 0)
    BCS ?.Uploaded

    LDA #$80
    CMP <palette>
RTL

?.Uploaded
    SEP #$30
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA $0C
    STA <palette>
    LSR
    TAX

if !PalettesEffects
    LDA DX_PPU_CGRAM_SPBaseLoaded
    ORA.l ?.BitSetter,x
    STA DX_PPU_CGRAM_SPBaseLoaded
endif

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteVersion>
    STA <paletteLastVersion>
    SEC
RTL

if !PalettesEffects
?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
endif
endmacro
macro PaletteAssignmentNoUploadTail(sprite, palette, paletteVersion, paletteLastVersion)
?DXPaletteAssignmentNoUploadTail:
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA $0C
    STA <palette>
    LDA <paletteVersion>
    STA <paletteLastVersion>

    LDY #$01
    SEC
RTL
endmacro

macro PaletteAssignment(sprite, palette, paletteAssignment, paletteOption, paletteVersion, paletteLastVersion, manualIDGroup, manualID)
    %PaletteAssignmentCommon(0, <sprite>, "<palette>", "<paletteAssignment>", "<paletteOption>", "<paletteVersion>", "<paletteLastVersion>", "<manualIDGroup>", "<manualID>")
endmacro

macro StandardSpritePaletteAssignment(type, manualIDGroup, manualID)
    %PaletteAssignment(1, "!<type>Palette,x", "!<type>PaletteAssignment,x", "!<type>PaletteOption,x", "!<type>Version,x", "!<type>LastVersion,x", "<manualIDGroup>", "<manualID>")
endmacro

macro PaletteAssignmentNoUpload(sprite, palette, paletteAssignment, paletteOption, paletteVersion, paletteLastVersion, manualIDGroup, manualID)
    %PaletteAssignmentCommon(1, <sprite>, "<palette>", "<paletteAssignment>", "<paletteOption>", "<paletteVersion>", "<paletteLastVersion>", "<manualIDGroup>", "<manualID>")
endmacro

macro StandardSpritePaletteAssignmentNoUpload(type, manualIDGroup, manualID)
    %PaletteAssignmentNoUpload(1, "!<type>Palette,x", "!<type>PaletteAssignment,x", "!<type>PaletteOption,x", "!<type>Version,x", "!<type>LastVersion,x", "<manualIDGroup>", "<manualID>")
endmacro

;Input:
;X 8 bits = Palette x2
;Y 8 bits = 1 force rebuilding the effect, 0 rebuild only if needed
;A 16 bits = Palette Effect
;$02-$04 = Base Palette Address
;
;Output:
;Carry set = The palette has a usable effect, either the requested one or a fallback
;Carry clear = The palette has never had a usable effect
;M and X 8 bits
;
;LastGlobalEffectIDPerPal values:
;$FFFF = No usable effect has been loaded yet
;$FFFE = A new effect is pending, but the previous effect is still usable
;$0000-$FFFD = Last global effect successfully loaded
macro ConcatenatePaletteEffectToGlobal(sprite, paletteOption, palette, hsl)
?ConcatenatePaletteEffectToGlobal:
	STA $F2

	CPY #$00
	BNE ?.PrepareEffect

	CMP DX_PPU_CGRAM_LocalEffectID+$10,x
	BEQ ?+

?.PrepareEffect
	SEP #$20
if <hsl> == 0
	LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
else
	LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
endif
	AND.l ?.BitChecker,x
	BEQ ?.DoEffectWithBase

	REP #$20
	LDA $F2
	JMP ?.DoEffect
?+
	; X contains Palette x2. Preserve it while X is used to index
	; !PaletteEffectsTypes.
	STX $F1

	REP #$10
	LDA DX_Dynamic_Palettes_LastGlobalEffectID
	BEQ ?..LastGlobalEffectWasZero

	DEC A
	TAX

	LDA !PaletteEffectsTypes,x
	AND #$0001

if <hsl> == 0
	BNE ?..LastGlobalEffectUsedOtherBase
else
	BEQ ?..LastGlobalEffectUsedOtherBase
endif

	; The previous global effect used the same color space as the local
	; effect. Reuse the cache only if the current effect uses that same
	; color space. A zero or opposite-space effect needs a new result.
	LDA DX_Dynamic_Palettes_GlobalEffectID
	BEQ ?..RecalculateEffect

	DEC A
	TAX

	LDA !PaletteEffectsTypes,x
	AND #$0001

if <hsl> == 0
	BEQ ?..ReuseEffect
else
	BNE ?..ReuseEffect
endif

	BRA ?..RecalculateEffect

?..LastGlobalEffectUsedOtherBase
	; Reaching the opposite color space previously required both bases,
	; so any nonzero global effect can reuse them. Removing the global
	; effect needs the local-only result in PaletteCopy.
	LDA DX_Dynamic_Palettes_GlobalEffectID
	BEQ ?..RecalculateEffect

	BRA ?..ReuseEffect

?..LastGlobalEffectWasZero
	; With no previous global effect, PaletteCopy is valid, but the RGB/HSL
	; bases needed by a new global effect are not necessarily generated.
	LDA DX_Dynamic_Palettes_GlobalEffectID
	BEQ ?..ReuseEffect

?..RecalculateEffect
	SEP #$10
	LDX $F1
	LDA $F2
	BRA ?.DoEffect

?..ReuseEffect
	SEP #$30
	JMP ?.ReturnPaletteAvailability

?.DoEffectWithBase
	LDA.l ?.BitChecker,x
if <hsl> == 0
	ORA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
	STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
else
	ORA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
	STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
endif

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

if <hsl> == 0
	%SetRGBBase($02,$04,$05,"#DX_PPU_CGRAM_LocalEffectBuffer>>16","s",#$000F)
else
	%SetHSLBase($02,$04,$05,"#DX_PPU_CGRAM_LocalEffectBuffer>>16","s",#$000F)
endif

	BRA ?+

?.DoEffect
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

	TXA
	ASL
	TAX

	REP #$20
	LDA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
	CMP #$FFFF
	BEQ ?..KeepNeverLoadedState

	LDA #$FFFE
	STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
?..KeepNeverLoadedState
	SEP #$20

	TXA
	LSR
	TAX

	LDA DX_Dynamic_Palettes_Updated+$08,x
	BEQ ?+
	JMP ?.ReturnPaletteAvailability
?+
	LDA #$01
	STA DX_Dynamic_Palettes_Updated+$08,x

if <sprite> != 0
	LDX !SpriteIndex
endif
	LDA <paletteOption>
	AND #$80
	BNE ?..OnlyEffect
	JMP ?..EffectAndMerge

?..OnlyEffect
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	SEP #$20
if <hsl> == 0
	BNE ?...ApplyGlobalEffect
else
	BEQ ?+
	JMP ?...ApplyGlobalEffect
?+
endif

if <hsl> != 0
	REP #$20
	LDA $F2
	BNE ?++
	SEP #$20

	%HSLToRGB("#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16",$F0,#$000F)
	%DoEffectAndMerge($F2,"#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteCopy","#DX_PPU_CGRAM_PaletteCopy>>16",$F0,#$000F)

	JMP ?...LocalBaseReady
?++
	SEP #$20
endif
	%DoEffectAndMerge($F2,"#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_PaletteCopy","#DX_PPU_CGRAM_PaletteCopy>>16",$F0,#$000F)

?...LocalBaseReady
if <sprite> != 0
	LDX !SpriteIndex
endif
	LDA <palette>
	LSR
	TAX
	LDA DX_PPU_CGRAM_SPBaseLoaded
	ORA.l ?.BitSetter,x
	STA DX_PPU_CGRAM_SPBaseLoaded
	JMP ?...Finish

?...ApplyGlobalEffect
if <hsl> == 0
	%DoEffect($F2,"#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16",$F0,#$000F)
else
	%DoEffect($F2,"#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16",$F0,#$000F)
endif

	REP #$30
	LDA DX_Dynamic_Palettes_GlobalEffectID
	DEC A
	TAX

	SEP #$20
	LDA !PaletteEffectsTypes,x
	SEP #$30
	AND #$01
if <hsl> == 0
	BEQ ?...ConvertedBaseReady

	%RGBToHSL("#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16",$F0,#$000F)
else
	BNE ?...ConvertedBaseReady

	%HSLToRGB("#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16",$F0,#$000F)
endif

if <sprite> != 0
	LDX !SpriteIndex
endif
	LDA <palette>
	LSR
	TAX

if <hsl> == 0
	LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
	ORA.l ?.BitSetter,x
	STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
else
	LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
	ORA.l ?.BitSetter,x
	STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
endif

?...ConvertedBaseReady
if <sprite> != 0
	LDX !SpriteIndex
endif
	LDA <palette>
	LSR
	TAX
	LDA DX_PPU_CGRAM_SPBaseLoaded
	AND.l ?.BitClearer,x
	STA DX_PPU_CGRAM_SPBaseLoaded

?...Finish
if <sprite> != 0
	LDX !SpriteIndex
endif
	LDA <palette>
	LSR
	TAX

if <hsl> == 0
	LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
	ORA.l ?.BitSetter,x
	STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
else
	LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
	ORA.l ?.BitSetter,x
	STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
endif

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

	REP #$20
	LDA $F2
	STA DX_PPU_CGRAM_LocalEffectID+$10,x
	SEP #$20

	JMP ?.ReturnPaletteAvailability

?..EffectAndMerge
if <hsl> != 0
	REP #$20
	LDA $F2
	BNE ?+
	SEP #$20

	%HSLToRGB("#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16",$F0,#$000F)
	%DoEffectAndMerge($F2,"#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16",$F0,#$000F)

	JMP ?...EffectReady
?+
	SEP #$20
endif
	%DoEffectAndMerge($F2,"#DX_PPU_CGRAM_LocalEffectBuffer","#DX_PPU_CGRAM_LocalEffectBuffer>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16",$F0,#$000F)
?...EffectReady

	LDA $F0
	REP #$21
	AND #$00FF
	ASL
	ADC #DX_PPU_CGRAM_PaletteWriteMirror
	STA $05
	SEP #$20

	%TransferToCGRAM($F0,$05,"#DX_PPU_CGRAM_PaletteWriteMirror>>16",#$001E)
	BCS ?+
	JMP ?.ReturnPaletteAvailability
?+
	LDA $F0
	AND #$7F
	ASL
	CLC
	ADC #$1C
	TAX
	LDY #$1C
	REP #$21
?-
	LDA.w DX_PPU_CGRAM_PaletteWriteMirror+$0100,x
	STA.l DX_PPU_CGRAM_PaletteCopy+$0100,x
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

	LDA <palette>
	LSR
	TAX
	LDA DX_PPU_CGRAM_SPBaseLoaded
	ORA.l ?.BitSetter,x
	STA DX_PPU_CGRAM_SPBaseLoaded
	TXA
	ASL
	TAX

	REP #$20
	LDA $F2
	STA DX_PPU_CGRAM_LocalEffectID+$10,x

	LDA #$0000
	STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
	SEP #$20

?.ReturnPaletteAvailability
if <sprite> != 0
	LDX !SpriteIndex
endif

	LDA <palette>
	AND #$0E
	TAX

	REP #$20
	LDA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
	INC A
	CMP #$0001
	SEP #$20

if <sprite> != 0
	LDX !SpriteIndex
endif
RTL

?.BitSetter
	db $01,$02,$04,$08,$10,$20,$40,$80

?.BitClearer
	db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F

?.BitChecker
	db $01,$01,$02,$02,$04,$04,$08,$08,$10,$10,$20,$20,$40,$40,$80,$80
endmacro


macro ConcatenateRGBPaletteEffectToGlobal(sprite, paletteOption, palette)
    %ConcatenatePaletteEffectToGlobal(<sprite>, "<paletteOption>", "<palette>", 0)
endmacro

macro ConcatenateHSLPaletteEffectToGlobal(sprite, paletteOption, palette)
    %ConcatenatePaletteEffectToGlobal(<sprite>, "<paletteOption>", "<palette>", 1)
endmacro

; Replaces the contents of an already assigned shared palette.
; The caller must invoke this routine only when a replacement is required.
; Calling it every frame will force unnecessary cache invalidation and uploads.
;
; DX_Dynamic_Palettes_ID is intentionally not changed: it identifies the
; shared effect palette slot, not the temporary palette used as its contents.
macro ReplacePalette(sprite, paletteOption, palette)
?ReplacePalette:
    if <sprite> != 0
    LDX !SpriteIndex
    endif

if !PalettesEffects
    LDA <paletteOption>
    AND #$80
    BEQ ?.NoPalEffect

    LDA <palette>
    ASL
    ASL
    ASL
    ORA #$81
    STA $00

    %TransferPaletteData($02, "$00", $1E, 1)
    SEP #$30

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
    LSR
    TAX
    JSR ?.InvalidatePaletteState

    LDA DX_Dynamic_Palettes_GlobalSPEnable
    ORA.l ?.BitSetter,x
    STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_Dynamic_Palettes_EffectEnabled
    ORA.l ?.BitSetter,x
    STA DX_Dynamic_Palettes_EffectEnabled

    LDA DX_PPU_CGRAM_SPBaseLoaded
    ORA.l ?.BitSetter,x
    STA DX_PPU_CGRAM_SPBaseLoaded

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    JMP ?.ReturnPaletteAvailability

?.NoPalEffect
endif
    LDA <palette>
    ASL
    ASL
    ASL
    ORA #$81
    STA $00

    %TransferPaletteData($02, "$00", $1E, 0)
    BCS ?.Uploaded

    LDA #$80
    CMP <palette>
RTL

?.Uploaded
    SEP #$30
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <palette>
    LSR
    TAX

if !PalettesEffects
    JSR ?.InvalidatePaletteState

    LDA DX_PPU_CGRAM_SPBaseLoaded
    ORA.l ?.BitSetter,x
    STA DX_PPU_CGRAM_SPBaseLoaded
endif

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    SEC
RTL

if !PalettesEffects == 1
?.ReturnPaletteAvailability
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <palette>
    AND #$0E
    TAX

    REP #$20
    LDA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
    INC A
    CMP #$0001
    SEP #$20

    if <sprite> != 0
    LDX !SpriteIndex
    endif
RTL

; Input:
; X = Palette index between 0 and 7
;
; Output:
; X = Palette index between 0 and 7
; A = Palette index between 0 and 7
; M = 8 bits
?.InvalidatePaletteState
    LDA DX_Dynamic_Palettes_GlobalSPEnable
	AND.l ?.BitClearer,x
	STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_PPU_CGRAM_SPBaseLoaded
    AND.l ?.BitClearer,x
    STA DX_PPU_CGRAM_SPBaseLoaded

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

    REP #$20
    LDA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
    CMP #$FFFF
    BEQ ?+

    LDA #$FFFE
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
?+
    SEP #$20

    TXA
    LSR
    TAX
RTS

?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
?.BitClearer
    db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F
endif
endmacro

macro TransferPaletteFromAddress(Address, CGRAMOffset, Length, GlobalEffect)
    REP #$20
    LDA #(<Address>&$FFFF)
    STA $65
    SEP #$20
    LDA.b #(<Address>>>16)
    STA $67
    %TransferPaletteFromPointer($65, "<CGRAMOffset>", <Length>, <GlobalEffect>)
endmacro

macro TransferPaletteFromPointer(Pointer, CGRAMOffset, Length, GlobalEffect)
    %TransferPaletteCommon("<Pointer>", "<CGRAMOffset>", <Length>, <GlobalEffect>)
endmacro

macro TransferPaletteFromTable(TableAddr, TableBNK, CGRAMOffset, Length, GlobalEffect)
    REP #$20
    LDA <TableAddr>
    STA $65
    SEP #$20
    LDA <TableBNK>
    STA $67
    %TransferPaletteFromPointer($65, "<CGRAMOffset>", <Length>, <GlobalEffect>)
endmacro

macro TransferPaletteResource(Name, CGRAMOffset, GlobalEffect)
    %TransferPaletteFromAddress(!Palette<Name>, "<CGRAMOffset>", !Palette<Name>Size, <GlobalEffect>)
endmacro

macro TransferPaletteResourceWithLength(Name, CGRAMOffset, Length, GlobalEffect)
    %TransferPaletteFromAddress(!Palette<Name>, "<CGRAMOffset>", <Length>, <GlobalEffect>)
endmacro

macro TransferPaletteData(Source, CGRAMOffset, Length, GlobalEffect)
?TransferPaletteData:
if !PalettesEffects && <GlobalEffect> != 0
    LDA <CGRAMOffset>
    REP #$30
    AND #$00FF
    ASL
    TAX
    LDY #$0000
?.CopyBase
    LDA [<Source>],y
    STA.l DX_PPU_CGRAM_PaletteCopy,x
    INX
    INX
    INY
    INY
    CPY.w #<Length>
    BCC ?.CopyBase
else
    %TransferToCGRAM("<CGRAMOffset>", "<Source>", "<Source>+2", "#(<Length>|$0000)")
    BCS ?.CopyPalette
    JMP ?.End

?.CopyPalette
    LDA <CGRAMOffset>
    REP #$30
    AND #$00FF
    ASL
    TAX
    LDY #$0000
?..Loop
    LDA [<Source>],y
if !PalettesEffects
    STA.l DX_PPU_CGRAM_PaletteCopy,x
endif
    STA.w DX_PPU_CGRAM_PaletteWriteMirror,x
    STA.w $0905|!addr,x
    INX
    INX
    INY
    INY
    CPY.w #<Length>
    BCC ?..Loop
endif
?.End
endmacro
macro TransferPaletteCommon(Source, CGRAMOffset, Length, GlobalEffect)
?TransferPaletteCommon:
    %TransferPaletteData("<Source>", "<CGRAMOffset>", <Length>, <GlobalEffect>)
if !PalettesEffects == 0 || <GlobalEffect> == 0
    BCS ?.UpdateState
    JMP ?.End
?.UpdateState
endif
if !PalettesEffects
    TXA
    STA $6A
    DEC A
    AND #$01E0
    LSR #4
    ADC #$0002
    STA $68

    LDA $6A
    SEC
    SBC.w #<Length>
    AND #$01E0
    LSR #4
    TAX

    TXY
    LDA.l ?.RangeMasks,x
    LDX $68
    EOR.l ?.RangeMasks,x
    STA $6A
    EOR #$FFFF
    STA $6C

    LDA $6A
    ORA DX_PPU_CGRAM_BGBaseLoaded
    STA DX_PPU_CGRAM_BGBaseLoaded

    LDA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
    AND $6C
    STA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
    LDA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
    AND $6C
    STA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
    LDA DX_PPU_CGRAM_BGEffectLoaded
    AND $6C
    STA DX_PPU_CGRAM_BGEffectLoaded

if <GlobalEffect> != 0
    LDA $6A
    ORA DX_Dynamic_Palettes_GlobalBGEnable
else
    LDA DX_Dynamic_Palettes_GlobalBGEnable
    AND $6C
endif
    STA DX_Dynamic_Palettes_GlobalBGEnable
    TYX
    LDA #$FFFF
?.InvalidateID
    STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal,x
    INX
    INX
    CPX $68
    BCC ?.InvalidateID

endif

    SEP #$30
    SEC
if !PalettesEffects
    BRA ?.End
?.RangeMasks
    dw $0000,$0001,$0003,$0007,$000F,$001F,$003F,$007F
    dw $00FF,$01FF,$03FF,$07FF,$0FFF,$1FFF,$3FFF,$7FFF
    dw $FFFF
endif

?.End
endmacro
