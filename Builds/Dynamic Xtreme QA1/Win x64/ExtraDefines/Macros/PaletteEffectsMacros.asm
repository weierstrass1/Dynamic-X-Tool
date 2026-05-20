
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
