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
if !PaletteChange == 0
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
if !PaletteEffects == 1
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
if !PaletteEffects == 1
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

if !PaletteEffects == 1
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

if !PaletteEffects == 1
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

if !PaletteEffects == 1
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

!Target = $51

!ratio1Inv = $D5
!ratio2Inv = $D6
!ratio3Inv = $D7

macro SetBaseParams(src,srcbnk,dst,dstbnk,offset,length)
    LDA <srcbnk>
    STA !Source+2

    LDA <dstbnk>
    STA !Dst+2

    REP #$21
if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    ASL
    PHA
    ADC <offset>
    ADC <dst>
    STA !Dst

    PLA
    CLC
    ADC <src>
    STA !Source
else
    LDA <src>
    STA !Source

    LDA <dst>
    STA !Dst
endif

    LDA <length>
    STA !length

    SEP #$20
endmacro

macro SetRGBBase(src,srcbnk,dst,dstbnk,offset,length)
    %SetBaseParams("<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !SetRGBBase
endmacro

macro SetRGBBaseDefault(offset,length)
    %SetRGBBase("#DX_PPU_CGRAM_PaletteCopy","#DX_PPU_CGRAM_PaletteCopy>>16","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","<offset>","<length>")
endmacro

macro SetHSLBase(src,srcbnk,dst,dstbnk,offset,length)
    %SetBaseParams("<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !SetHSLBase
endmacro

macro SetHSLBaseDefault(offset,length)
    %SetHSLBase("#DX_PPU_CGRAM_PaletteCopy","#DX_PPU_CGRAM_PaletteCopy>>16","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","<offset>","<length>")
endmacro

macro SetMixAndMerge3Params(r1,r2,r3,v1,v2,v3,src,srcbnk,dst,dstbnk,offset,length)
    if stringsequalnocase("<r1>","!ratio1") == 0
    LDA <r1>
    STA !ratio1
    endif

    if stringsequalnocase("<r2>","!ratio2") == 0
    LDA <r2>
    STA !ratio2
    endif

    if stringsequalnocase("<r3>","!ratio3") == 0
    LDA <r3>
    STA !ratio3
    endif

    if stringsequalnocase("<v1>","!V1") == 0
    LDA <v1>
    STA !V1
    endif

    if stringsequalnocase("<v2>","!V2") == 0
    LDA <v2>
    STA !V2
    endif

    if stringsequalnocase("<v3>","!V3") == 0
    LDA <v3>
    STA !V3
    endif

    LDA <srcbnk>
    STA !Source+2

    LDA <dstbnk>
    STA !Dst+2

    REP #$21
if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    ASL
    PHA
    ADC <offset>
    ADC <src>
    STA !Source

    PLA
    CLC
    ADC <dst>
    STA !Dst
else
    LDA <src>
    STA !Source

    LDA <dst>
    STA !Dst
endif

    LDA <length>
    STA !length

    SEP #$20
endmacro

macro MixAndMergeRGB(r1,r2,r3,v1,v2,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>","<r2>","<r3>","<v1>","<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeRGB
endmacro

macro MixAndMergeRGBDefault(r1,r2,r3,v1,v2,v3,offset,length)
    %MixAndMergeRGB("<r1>","<r2>","<r3>","<v1>","<v2>","<v3>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeRG(r1,r2,v1,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>","<r2>",!ratio3,"<v1>","<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeRG
endmacro

macro MixAndMergeRGDefault(r1,r2,v1,v2,offset,length)
    %MixAndMergeRG("<r1>","<r2>","<v1>","<v2>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeRB(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>",!ratio2,"<r3>","<v1>",!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeRB
endmacro

macro MixAndMergeRBDefault(r1,r3,v1,v3,offset,length)
    %MixAndMergeRB("<r1>","<r3>","<v1>","<v3>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeGB(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,"<r2>","<r3>",!V1,"<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeGB
endmacro

macro MixAndMergeGBDefault(r2,r3,v2,v3,offset,length)
    %MixAndMergeGB("<r2>","<r3>","<v2>","<v3>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeR(r1,v1,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>",!ratio2,!ratio3,"<v1>",!V2,!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeR
endmacro

macro MixAndMergeRDefault(r1,v1,offset,length)
    %MixAndMergeR("<r1>","<v1>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeG(r2,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,"<r2>",!ratio3,!V1,"<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeG
endmacro

macro MixAndMergeGDefault(r2,v2,offset,length)
    %MixAndMergeG("<r2>","<v2>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeB(r3,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,!ratio2,"<r3>",!V1,!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeB
endmacro

macro MixAndMergeBDefault(r3,v3,offset,length)
    %MixAndMergeB("<r3>","<v3>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeHSL(r1,r2,r3,v1,v2,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>","<r2>","<r3>","<v1>","<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeHSL
endmacro

macro MixAndMergeHSLDefault(r1,r2,r3,v1,v2,v3,offset,length)
    %MixAndMergeHSL("<r1>","<r2>","<r3>","<v1>","<v2>","<v3>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeHS(r1,r2,v1,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>","<r2>",!ratio3,"<v1>","<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeHS
endmacro

macro MixAndMergeHSDefault(r1,r2,v1,v2,offset,length)
    %MixAndMergeHS("<r1>","<r2>","<v1>","<v2>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeHL(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>",!ratio2,"<r3>","<v1>",!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeHL
endmacro

macro MixAndMergeHLDefault(r1,r3,v1,v3,offset,length)
    %MixAndMergeHL("<r1>","<r3>","<v1>","<v3>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeSL(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,"<r2>","<r3>",!V1,"<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeSL
endmacro

macro MixAndMergeSLDefault(r2,r3,v2,v3,offset,length)
    %MixAndMergeSL("<r2>","<r3>","<v2>","<v3>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeH(r1,v1,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params("<r1>",!ratio2,!ratio3,"<v1>",!V2,!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeH
endmacro

macro MixAndMergeHDefault(r1,v1,offset,length)
    %MixAndMergeH("<r1>","<v1>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeS(r2,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,"<r2>",!ratio3,!V1,"<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeS
endmacro

macro MixAndMergeSDefault(r2,v2,offset,length)
    %MixAndMergeS("<r2>","<v2>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro MixAndMergeL(r3,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,!ratio2,"<r3>",!V1,!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixAndMergeL
endmacro

macro MixAndMergeLDefault(r3,v3,offset,length)
    %MixAndMergeL("<r3>","<v3>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro SetMix3Params(r1,r2,r3,v1,v2,v3,src,srcbnk,dst,dstbnk,offset,length)
    if stringsequalnocase("<r1>","!ratio1") == 0
    LDA <r1>
    STA !ratio1
    endif

    if stringsequalnocase("<r2>","!ratio2") == 0
    LDA <r2>
    STA !ratio2
    endif

    if stringsequalnocase("<r3>","!ratio3") == 0
    LDA <r3>
    STA !ratio3
    endif

    if stringsequalnocase("<v1>","!V1") == 0
    LDA <v1>
    STA !V1
    endif

    if stringsequalnocase("<v2>","!V2") == 0
    LDA <v2>
    STA !V2
    endif

    if stringsequalnocase("<v3>","!V3") == 0
    LDA <v3>
    STA !V3
    endif

    LDA <srcbnk>
    STA !Source+2

    LDA <dstbnk>
    STA !Dst+2

    REP #$21
if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    ASL
    ADC <offset>
    PHA
    ADC <src>
    STA !Source

    PLA
    CLC
    ADC <dst>
    STA !Dst
else
    LDA <src>
    STA !Source

    LDA <dst>
    STA !Dst
endif

    LDA <length>
    STA !length

    SEP #$20
endmacro

macro MixRGB(r1,r2,r3,v1,v2,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>","<r2>","<r3>","<v1>","<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixRGB
endmacro

macro MixRG(r1,r2,v1,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>","<r2>",!ratio3,"<v1>","<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixRG
endmacro

macro MixRB(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>",!ratio2,"<r3>","<v1>",!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixRB
endmacro

macro MixGB(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params(!ratio1,"<r2>","<r3>",!V1,"<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixGB
endmacro

macro MixR(r1,v1,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>",!ratio2,!ratio3,"<v1>",!V2,!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixR
endmacro

macro MixG(r2,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params(!ratio1,"<r2>",!ratio3,!V1,"<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixG
endmacro

macro MixB(r3,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params(!ratio1,!ratio2,"<r3>",!V1,!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixB
endmacro

macro MixHSL(r1,r2,r3,v1,v2,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>","<r2>","<r3>","<v1>","<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixHSL
endmacro

macro MixHS(r1,r2,v1,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>","<r2>",!ratio3,"<v1>","<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixHS
endmacro

macro MixHL(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>",!ratio2,"<r3>","<v1>",!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixHL
endmacro

macro MixSL(r1,r3,v1,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params(!ratio1,"<r2>","<r3>",!V1,"<v2>","<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixSL
endmacro

macro MixH(r1,v1,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params("<r1>",!ratio2,!ratio3,"<v1>",!V2,!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixH
endmacro

macro MixS(r2,v2,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params(!ratio1,"<r2>",!ratio3,!V1,"<v2>",!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixS
endmacro

macro MixL(r3,v3,src,srcbnk,dst,dstbnk,offset,length)
    %SetMix3Params(!ratio1,!ratio2,"<r3>",!V1,!V2,"<v3>","<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !MixL
endmacro

macro SetPalTransAndMergeParams(r1,r2,r3,src,srcbnk,targetpal,targetpalbnk,dst,dstbnk,offset,length)
    if stringsequalnocase("<r1>","!ratio1") == 0
    LDA <r1>
    STA !ratio1
    endif

    if stringsequalnocase("<r2>","!ratio2") == 0
    LDA <r2>
    STA !ratio2
    endif

    if stringsequalnocase("<r3>","!ratio3") == 0
    LDA <r3>
    STA !ratio3
    endif

    LDA <targetpalbnk>
    STA !Target+2

    LDA <srcbnk>
    STA !Source+2

    LDA <dstbnk>
    STA !Dst+2

    REP #$21
if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    ASL
    PHA
    ADC <offset>
    PHA
    ADC <src>
    STA !Source

    PLA
    CLC
    ADC <targetpal>
    STA !Target

    PLA
    CLC
    ADC <dst>
    STA !Dst

else
    LDA <src>
    STA !Source

    LDA <dst>
    STA !Dst

    LDA <targetpal>
    STA !Target
endif

    LDA <length>
    STA !length

    SEP #$20
endmacro

macro PaletteTransitionRGBAndMerge(r1,r2,r3,src,srcbnk,targetpal,targetpalbnk,dst,dstbnk,offset,length)
    %SetPalTransAndMergeParams("<r1>","<r2>","<r3>","<src>","<srcbnk>","<targetpal>","<targetpalbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !PalTransitionAndMergeRGB
endmacro

macro PaletteTransitionRGBAndMergeDefault(r1,r2,r3,targetpal,targetpalbnk,offset,length)
    %PaletteTransitionRGBAndMerge("<r1>","<r2>","<r3>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","<targetpal>","<targetpalbnk>","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro PaletteTransitionHSLAndMerge(r1,r2,r3,src,srcbnk,targetpal,targetpalbnk,dst,dstbnk,offset,length)
    %SetPalTransAndMergeParams("<r1>","<r2>","<r3>","<src>","<srcbnk>","<targetpal>","<targetpalbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !PalTransitionAndMergeHSL
endmacro

macro PaletteTransitionHSLAndMergeDefault(r1,r2,r3,targetpal,targetpalbnk,offset,length)
    %PaletteTransitionHSLAndMerge("<r1>","<r2>","<r3>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","<targetpal>","<targetpalbnk>","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro SetPalTransParams(r1,r2,r3,src,srcbnk,targetpal,targetpalbnk,dst,dstbnk,offset,length)
    if stringsequalnocase("<r1>","!ratio1") == 0
    LDA <r1>
    STA !ratio1
    endif

    if stringsequalnocase("<r2>","!ratio2") == 0
    LDA <r2>
    STA !ratio2
    endif

    if stringsequalnocase("<r3>","!ratio3") == 0
    LDA <r3>
    STA !ratio3
    endif

    LDA <targetpalbnk>
    STA !Target+2

    LDA <srcbnk>
    STA !Source+2

    LDA <dstbnk>
    STA !Dst+2

    REP #$21
if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    ASL
    ADC <offset>
    PHA
    ADC <src>
    STA !Source

    LDA $01,s
    CLC
    ADC <targetpal>
    STA !Target

    PLA
    CLC
    ADC <dst>
    STA !Dst

else
    LDA <src>
    STA !Source

    LDA <dst>
    STA !Dst

    LDA <targetpal>
    STA !Target
endif

    LDA <length>
    STA !length

    SEP #$20
endmacro

macro PaletteTransitionRGB(r1,r2,r3,src,srcbnk,targetpal,targetpalbnk,dst,dstbnk,offset,length)
    %SetPalTransParams("<r1>","<r2>","<r3>","<src>","<srcbnk>","<targetpal>","<targetpalbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !PalTransitionRGB
endmacro

macro PaletteTransitionHSL(r1,r2,r3,src,srcbnk,targetpal,targetpalbnk,dst,dstbnk,offset,length)
    %SetPalTransParams("<r1>","<r2>","<r3>","<src>","<srcbnk>","<targetpal>","<targetpalbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !PalTransitionHSL
endmacro

macro SetConverterParams(src,srcbnk,dst,dstbnk,offset,length)
    LDA <srcbnk>
    STA !Source+2

    LDA <dstbnk>
    STA !Dst+2

    REP #$21
if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    ASL
    ADC <offset>
    PHA
    ADC <dst>
    STA !Dst

    PLA
    CLC
    ADC <src>
    STA !Source
else
    LDA <src>
    STA !Source

    LDA <dst>
    STA !Dst
endif

    LDA <length>
    STA !length

    SEP #$20
endmacro

macro RGBToHSL(src,srcbnk,dst,dstbnk,offset,length)
    %SetConverterParams("<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !RGBToHSL
endmacro

macro HSLToRGB(src,srcbnk,dst,dstbnk,offset,length)
    %SetConverterParams("<src>","<srcbnk>","<dst>","<dstbnk>","<offset>","<length>")

    JSL !HSLToRGB
endmacro

macro DoEffectAndMerge(effect,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,!ratio2,!ratio3,!V1,!V2,!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","s","<length>")

if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    STA $00
endif

    REP #$30
    LDA <effect>
    JSL !DoEffectAndMerge
endmacro

macro DoEffectAndMergeDefaultRGB(effect,offset,length)
    %DoEffectAndMerge("<effect>","#DX_PPU_CGRAM_BaseRGBPalette","#DX_PPU_CGRAM_BaseRGBPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro DoEffectAndMergeDefaultHSL(effect,offset,length)
    %DoEffectAndMerge("<effect>","#DX_PPU_CGRAM_BaseHSLPalette","#DX_PPU_CGRAM_BaseHSLPalette>>16","#DX_PPU_CGRAM_PaletteWriteMirror","#DX_PPU_CGRAM_PaletteWriteMirror>>16","<offset>","<length>")
endmacro

macro DoEffect(effect,src,srcbnk,dst,dstbnk,offset,length)
    %SetMixAndMerge3Params(!ratio1,!ratio2,!ratio3,!V1,!V2,!V3,"<src>","<srcbnk>","<dst>","<dstbnk>","s","<length>")

if stringsequalnocase("<offset>","s") == 0
    LDA <offset>
    STA $00
endif

    REP #$30
    LDA <effect>
    JSL !DoEffect
endmacro

macro PaletteAssignmentNoUpload(sprite, palette, paletteAssignment, paletteOption, paletteVersion, paletteLastVersion, manualIDGroup, manualID)
?DXPaletteAssignment:
if !PaletteChange == 0
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
if !PaletteEffects == 1
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
if !PaletteEffects == 1
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

if !PaletteEffects == 1
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
if !PaletteEffects == 1
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
	BNE ?.DoEffectWithBase
	CMP DX_PPU_CGRAM_LocalEffectID+$10,x
    BEQ ?+
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
endmacro

macro ReplacePalette(sprite, paletteOption, palette)
?ReplacePalette:
    if <sprite> != 0
    LDX !SpriteIndex
    endif

if !PaletteEffects == 1
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

if !PaletteEffects == 1
?.BitSetter
    db $01,$02,$04,$08,$10,$20,$40,$80
?.BitClearer
    db $FE,$FD,$FB,$F7,$EF,$DF,$BF,$7F
endif
endmacro