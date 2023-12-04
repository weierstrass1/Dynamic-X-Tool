macro PaletteAssignment(sprite, palette, paletteAssignment, paletteOption, paletteVersion, paletteLastVersion, manualIDGroup, manualID)
;A = Palette Table ID, 16 bits
?DXPaletteAssignment:
    STA $00
    SEP #$20

    LDA $0100|!addr
    TAX
    LDA.l ?.GameModeAllowed,x
    BNE ?+

    if <sprite> != 0
    LDX !SpriteIndex
    endif

	LDA <palette>
	CMP #$FF
	BNE ?++
	CLC
RTL
?++
    LSR
    TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
	SEC
RTL
?+
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    ;$00 = Palette ID
    ;$02-$04 = Palette Address
    LDA #$00
    XBA
    LDA <paletteVersion>
    REP #$30
    CLC
    ADC $00
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

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
    CMP #$FF
    BEQ ?.PaletteNotAssigned

    LDA <paletteVersion>
    CMP <paletteLastVersion>
    BNE ?.PaletteNotAssigned
    
?.PaletteAssigned

    LDA <palette>
    LSR
    TAX

    LDA DX_Dynamic_Palettes_DisableTimer+$08,x
    BEQ ?.PaletteNotAssigned

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
    PHA
    LSR
    TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x
    BRA ?.PaletteNotAssigned_Success
    
?.PaletteNotAssigned
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
    PHA

    JSR ?.AssignPalette
    BCC ?+ 
    JMP ?.Loading
?+
    CPY #$FF
    BNE ?..Success
?..Failed
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    PLA
    STA <palette>
    CLC
RTL
?..Success
if !PaletteEffects

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <paletteOption>
    AND #$80
    BEQ ?..DontLoad

    LDA <palette>
    LSR
    TAX
    LDA DX_Dynamic_Palettes_Updated+$08,x
    BNE ?..SkipLoad

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
    TAX
    LDA DX_Dynamic_Palettes_LastGlobalEffectID+$10,x
    CMP DX_Dynamic_Palettes_GlobalEffectID
    BNE ?.Loading

    LDA DX_Dynamic_Palettes_LastGlobalEffectID+$11,x
    CMP DX_Dynamic_Palettes_GlobalEffectID+$01
    BNE ?.Loading

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
    LSR
    TAX
    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND.l ?.OrTable,x
    BEQ ?.Loading
?..SkipLoad
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <paletteVersion>
    STA <paletteLastVersion>
    PLA
    SEC
RTL
?..DontLoad
    LDA <palette>
    LSR
    TAX
    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND.l ?.AndTable,x
    STA DX_Dynamic_Palettes_GlobalSPEnable
endif
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <paletteVersion>
    STA <paletteLastVersion>
    PLA
    SEC
RTL

?.Loading
    REP #$20
    LDA DX_Dynamic_CurrentDataSend
    CLC
    ADC #$001E
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BEQ ?..Success
    BCC ?..Success
?..Failed
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    PLA
    STA <palette>
    CLC
RTL
?..Success
if !PaletteEffects == 0
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteVersion>
    STA <paletteLastVersion>

    PLA

    LDA <palette>
    ASL
    ASL
    ASL
    CLC
    ADC #$81
    STA $00

    LDA <palette>
    TAX
    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x

    %TransferToCGRAM("$00", "$02", "$04", "#$001E")
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    SEC
RTL
else
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteVersion>
    STA <paletteLastVersion>

    PLA

    LDA <palette>
    TAX
    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
    LSR
    TAX
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA DX_Dynamic_Palettes_GlobalEffectID
    BNE ?+
    LDA DX_Dynamic_Palettes_GlobalEffectID+$01
    BNE ?+
    LDA <palette>
    TAX
    LDA DX_Dynamic_Palettes_GlobalEffectID
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$10,x
    LDA DX_Dynamic_Palettes_GlobalEffectID+$01
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$11,x
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    BRA ?..NoEffect
?+
    LDA <paletteOption>
    AND #$80
    BNE ?..Effect

    LDA <palette>
    LSR
    TAX
    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND.l ?.AndTable,x
    STA DX_Dynamic_Palettes_GlobalSPEnable

    if <sprite> != 0
    LDX !SpriteIndex
    endif

?..NoEffect

    LDA <palette>
    ASL
    ASL
    ASL
    CLC
    ADC #$81
    STA $00

    %TransferToCGRAM("$00", "$02", "$04", "#$001E")
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    SEC
RTL
?..Effect
    LDA <palette>
    TAX
    LDA DX_Dynamic_Palettes_GlobalEffectID
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$10,x
    LDA DX_Dynamic_Palettes_GlobalEffectID+$01
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$11,x

    REP #$30
    LDA DX_Dynamic_Palettes_GlobalEffectID
    TAX
    SEP #$20
    LDA !PaletteEffectsTypes,x
    ASL
    TAY

    if <sprite> != 0
    LDA #$00
    XBA
    LDA !SpriteIndex
    TAX
    endif
    LDA <palette>
    LSR
    TAX
    LDA DX_Dynamic_Palettes_GlobalSPEnable
    ORA.l ?.OrTable,x
    STA DX_Dynamic_Palettes_GlobalSPEnable

    PHX
    TYX
    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded,x
    PLX
    PHA
    ORA.l ?.OrTable,x
    PHX
    TYX
    STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded,x

    LDA DX_PPU_CGRAM_SPPaletteCopyLoaded
    PLX
    PHA
    ORA.l ?.OrTable,x
    STA DX_PPU_CGRAM_SPPaletteCopyLoaded

    if <sprite> != 0
    LDA !SpriteIndex
    TAX
    endif
    
    LDA #$00
    XBA
    LDA <palette>
    ASL
    ASL
    ASL
    CLC
    ADC #$81
    STA $00
    REP #$20
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteCopy
    STA $06
    SEP #$30
    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA $08


    LDA <palette>
    LSR
    TAX

    PLA
    AND.l ?.OrTable,x
    BNE ?...SkipPaletteCopy

    LDA #$0F
    STA $05
    JSL !LoadPaletteOnBuffer

?...SkipPaletteCopy

    PLA
    AND.l ?.OrTable,x
    BNE ?...SkipSetBase
    SEC
    BRA ?...StartEffect
?...SkipSetBase
    CLC
?...StartEffect
    REP #$30
    LDA #$000F
    STA $02
    SEP #$40
    LDA DX_Dynamic_Palettes_GlobalEffectID
    JSL !ApplyPaletteEffect
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    SEC
RTL
endif
;Return Carry set if Must Upload Palette
?.AssignPalette
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <paletteAssignment>
    LSR
    LSR
    LSR
    AND #$06
    TAX

    JMP (?..AssignVersions,x)

?..AssignVersions
    dw ?...AutoAssignment
    dw ?...ManualAssignment
    dw ?...NoAssignment
    dw ?...NoAssignment

?...AutoAssignment
    REP #$20
    LDA $00
    JSL !AssignPalette
    BCS ?....Found

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    TYA
    ASL
    STA <palette>

if !PaletteEffects
    LSR
    TAX
    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND.l ?.AndTable,x
    STA DX_Dynamic_Palettes_GlobalSPEnable

    LDA DX_PPU_CGRAM_SPPaletteCopyLoaded
    AND.l ?.AndTable,x
    STA DX_PPU_CGRAM_SPPaletteCopyLoaded

    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
    AND.l ?.AndTable,x
    STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded

    LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
    AND.l ?.AndTable,x
    STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded

    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <palette>
endif
    LSR
    TAX
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x
    SEC
RTS
?....Found
    CPY #$FF
    BNE ?+
    CLC
RTS
?+
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    TYA
    ASL
    STA <palette>
    LSR
    TAX
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x
    CLC
RTS
?...ManualAssignment
    JSR ?...RegularAssignment
    SEC
RTS
?...NoAssignment
    JSR ?...RegularAssignment
    CLC
RTS
?...RegularAssignment

    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <paletteOption>
    LSR
    LSR
    LSR
    AND #$0E
    STA <palette>
    TAY
    LSR
    TAX
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

if !PaletteEffects
    if <sprite> != 0
    LDX !SpriteIndex
    endif

    LDA <palette>
    LSR
    TAX
    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND.l ?.AndTable,x
    STA DX_Dynamic_Palettes_GlobalSPEnable
    
    LDA DX_PPU_CGRAM_SPPaletteCopyLoaded
    AND.l ?.AndTable,x
    STA DX_PPU_CGRAM_SPPaletteCopyLoaded

    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
    AND.l ?.AndTable,x
    STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded

    LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
    AND.l ?.AndTable,x
    STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
endif

    TYX
    LDA DX_Dynamic_Palettes_ID+$10,x
    CMP #$FE
    BNE ?+
    LDA DX_Dynamic_Palettes_ID+$11,x
    CMP #$FF
    BNE ?+
RTS
?+
    if <sprite> != 0
    LDX !SpriteIndex
    endif
    LDA <manualID>
    TYX
    STA DX_Dynamic_Palettes_ID+$10,x
    LDA <manualIDGroup>
    STA DX_Dynamic_Palettes_ID+$11,x
RTS
?.OrTable
    db $80,$40,$20,$10,$08,$04,$02,$01
?.AndTable
    db $7F,$BF,$DF,$EF,$F7,$FB,$FD,$FE
?.GameModeAllowed
    ;   00  01  02  03  04  05  06  07
    db $00,$00,$00,$00,$00,$00,$00,$01
    ;   08  09  0A  0B  0C  0D  0E  0F
    db $00,$00,$00,$00,$00,$00,$01,$00
    ;   10  11  12  13  14  15  16  17
    db $00,$00,$00,$00,$01,$00,$00,$00
    ;   18  19  1A  1B  1C  1D  1E  1F
    db $00,$00,$00,$01,$00,$00,$00,$01
    ;   20  21  22  23  24  25  26  27
    db $00,$00,$00,$00,$00,$01,$00,$00
    ;   28  29  2A  2B  2C  2D  2E  2F
    db $00,$01,$00,$00,$00,$00,$00,$00
endmacro

macro StandardSpritePaletteAssignment(type, manualIDGroup, manualID)
    %PaletteAssignment(2, "!<type>Palette,x", "!<type>PaletteAssignment,x", "!<type>PaletteOption,x", "!<type>Version,x", "!<type>LastVersion,x", "<manualIDGroup>", "<manualID>")
endmacro

!Source = $45
!Dst = $48 
!iSource = $4B
!iDst = $4D
!length = $4F
!ratio1 = $8A
!ratio2 = $8C
!ratio3 = $8E
!V1 = $51
!V2 = $53
!V3 = $6A
!tmprl = $0E
!tmprh = $0F

macro SetHSLBase(sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    LDA <sourceBNK>
    STA $47

    LDA <destinationBNK>
    STA $4A

    REP #$20
    LDA <sourceAddr>
    STA $45

    LDA <destinationAddr> 
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !SetHSLBase
endmacro

macro SetRGBBase(sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    LDA <sourceBNK>
    STA $47

    LDA <destinationBNK>
    STA $4A

    REP #$20
    LDA <sourceAddr>
    STA $45

    LDA <destinationAddr> 
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !SetRGBBase
endmacro

macro MixHSL(ratio1,ratio2,ratio3,value1,value2,value3,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)

    LDA <ratio1>
    STA $8A

    LDA <ratio2>
    STA $8C

    LDA <ratio3>
    STA $8E

    LDA <value1>
    STA $51

    LDA <value2>
    STA $53

    LDA <value3>
    STA $6A

    LDA <sourceBNK>
    STA $47

    LDA <destinationBNK>
    STA $4A

    REP #$20
    LDA <sourceAddr>
    STA $45

    LDA <destinationAddr> 
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !MixHSL
endmacro

macro MixRGB(ratio1,ratio2,ratio3,value1,value2,value3,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)

    LDA <ratio1>
    STA $8A

    LDA <ratio2>
    STA $8C

    LDA <ratio3>
    STA $8E

    LDA <value1>
    STA $51

    LDA <value2>
    STA $53

    LDA <value3>
    STA $6A

    LDA <sourceBNK>
    STA $47

    LDA <destinationBNK>
    STA $4A

    REP #$20
    LDA <sourceAddr>
    STA $45

    LDA <destinationAddr> 
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !MixRGB
endmacro

macro SetHSLBaseDefault(offset,length)
    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA $47

    LDA.b #DX_PPU_CGRAM_BaseHSLPalette>>16
    STA $4A

    REP #$20
    LDA <offset>
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteCopy
    STA $45

    LDA <offset>
    ASL
    CLC
    ADC <offset>
    CLC
    ADC #DX_PPU_CGRAM_BaseHSLPalette
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !SetHSLBase
endmacro

macro SetRGBBaseDefault(offset,length)
    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA $47

    LDA.b #DX_PPU_CGRAM_BaseRGBPalette>>16
    STA $4A

    REP #$20
    LDA <offset>
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteCopy
    STA $45

    LDA <offset>
    ASL
    CLC
    ADC <offset>
    CLC
    ADC #DX_PPU_CGRAM_BaseRGBPalette
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !SetRGBBase
endmacro

macro MixHSLDefault(ratio1,ratio2,ratio3,value1,value2,value3,offset,length)

    LDA <ratio1>
    STA $8A

    LDA <ratio2>
    STA $8C

    LDA <ratio3>
    STA $8E

    LDA <value1>
    STA $51

    LDA <value2>
    STA $53

    LDA <value3>
    STA $6A

    LDA.b #DX_PPU_CGRAM_BaseHSLPalette>>16
    STA $47

    LDA.b #DX_PPU_CGRAM_PaletteWriteMirror>>16
    STA $4A

    REP #$20
    LDA <offset>
    ASL
    CLC
    ADC <offset>
    CLC
    ADC #DX_PPU_CGRAM_BaseHSLPalette
    STA $45

    LDA <offset>
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteWriteMirror 
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !MixHSL
endmacro

macro MixRGBDefault(ratio1,ratio2,ratio3,value1,value2,value3,offset,length)

    LDA <ratio1>
    STA $8A

    LDA <ratio2>
    STA $8C

    LDA <ratio3>
    STA $8E

    LDA <value1>
    STA $51

    LDA <value2>
    STA $53

    LDA <value3>
    STA $6A

    LDA.b #DX_PPU_CGRAM_BaseRGBPalette>>16
    STA $47

    LDA.b #DX_PPU_CGRAM_PaletteWriteMirror>>16
    STA $4A

    REP #$20
    LDA <offset>
    ASL
    CLC
    ADC <offset>
    CLC
    ADC #DX_PPU_CGRAM_BaseRGBPalette
    STA $45

    LDA <offset>
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteWriteMirror 
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !MixRGB
endmacro

macro SetHSLBaseDRAdder(BinFile,destinationAddr,destinationBNK,length)
    %SetHSLBase("#<BinFile>",".b #<BinFile>>>16","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro SetRGBBaseDRAdder(BinFile,destinationAddr,destinationBNK,length)
    %SetRGBBase("#<BinFile>",".b #<BinFile>>>16","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixH(ratio,value,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixHSL("<ratio>",#$00,#$00,"<value>",#$00,#$00,"<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixS(ratio,value,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixHSL(#$00,"<ratio>",#$00,#$00,"<value>",#$00,"<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixL(ratio,value,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixHSL(#$00,#$00,"<ratio>",#$00,#$00,"<value>","<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixR(ratio,value,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixRGB("<ratio>",#$00,#$00,"<value>",#$00,#$00,"<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixG(ratio,value,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixRGB(#$00,"<ratio>",#$00,#$00,"<value>",#$00,"<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixB(ratio,value,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixRGB(#$00,#$00,"<ratio>",#$00,#$00,"<value>","<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixHDefault(ratio,value,offset,length)
    %MixHSLDefault("<ratio>",#$00,#$00,"<value>",#$00,#$00,"<offset>","<length>")
endmacro

macro MixSDefault(ratio,value,offset,length)
    %MixHSLDefault(#$00,"<ratio>",#$00,#$00,"<value>",#$00,"<offset>","<length>")
endmacro

macro MixLDefault(ratio,value,offset,length)
    %MixHSLDefault(#$00,#$00,"<ratio>",#$00,#$00,"<value>","<offset>","<length>")
endmacro

macro MixRDefault(ratio,value,offset,length)
    %MixRGBDefault("<ratio>",#$00,#$00,"<value>",#$00,#$00,"<offset>","<length>")
endmacro

macro MixGDefault(ratio,value,offset,length)
    %MixRGBDefault(#$00,"<ratio>",#$00,#$00,"<value>",#$00,"<offset>","<length>")
endmacro

macro MixBDefault(ratio,value,offset,length)
    %MixRGBDefault(#$00,#$00,"<ratio>",#$00,#$00,"<value>","<offset>","<length>")
endmacro

macro MixHS(ratio1,ratio2,value1,value2,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixHSL("<ratio1>","<ratio2>",#$00,"<value1>","<value2>",#$00,"<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixHL(ratio1,ratio2,value1,value2,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixHSL("<ratio1>",#$00,"<ratio2>","<value1>",#$00,"<value2>","<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixSL(ratio1,ratio2,value1,value2,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixHSL(#$00,"<ratio1>","<ratio2>",#$00,"<value1>","<value2>","<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixRG(ratio1,ratio2,value1,value2,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixRGB("<ratio1>","<ratio2>",#$00,"<value1>","<value2>",#$00,"<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixRB(ratio1,ratio2,value1,value2,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixRGB("<ratio1>",#$00,"<ratio2>","<value1>",#$00,"<value2>","<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixGB(ratio1,ratio2,value1,value2,sourceAddr,sourceBNK,destinationAddr,destinationBNK,length)
    %MixRGB(#$00,"<ratio1>","<ratio2>",#$00,"<value1>","<value2>","<sourceAddr>","<sourceBNK>","<destinationAddr>","<destinationBNK>","<length>")
endmacro

macro MixHSDefault(ratio1,ratio2,value1,value2,offset,length)
    %MixHSLDefault("<ratio1>","<ratio2>",#$00,"<value1>","<value2>",#$00,"<offset>","<length>")
endmacro

macro MixHLDefault(ratio1,ratio2,value1,value2,offset,length)
    %MixHSLDefault("<ratio1>",#$00,"<ratio2>","<value1>",#$00,"<value2>","<offset>","<length>")
endmacro

macro MixSLDefault(ratio1,ratio2,value1,value2,offset,length)
    %MixHSLDefault(#$00."<ratio1>","<ratio2>",#$00,"<value1>","<value2>","<offset>","<length>")
endmacro

macro MixRGDefault(ratio1,ratio2,value1,value2,offset,length)
    %MixRGBDefault("<ratio1>","<ratio2>",#$00,"<value1>","<value2>",#$00,"<offset>","<length>")
endmacro

macro MixRBDefault(ratio1,ratio2,value1,value2,offset,length)
    %MixRGBDefault("<ratio1>",#$00,"<ratio2>","<value1>",#$00,"<value2>","<offset>","<length>")
endmacro

macro MixGBDefault(ratio1,ratio2,value1,value2,offset,length)
    %MixRGBDefault(#$00."<ratio1>","<ratio2>",#$00,"<value1>","<value2>","<offset>","<length>")
endmacro