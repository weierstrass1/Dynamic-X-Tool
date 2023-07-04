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

    LDA.b #DX_PPU_CGRAM_BasePalette>>16
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
    ADC #DX_PPU_CGRAM_BasePalette
    STA $48

    LDA <length>
    STA !length
    SEP #$20

    JSL !SetHSLBase
endmacro

macro SetRGBBaseDefault(offset,length)
    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA $47

    LDA.b #DX_PPU_CGRAM_BasePalette>>16
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
    ADC #DX_PPU_CGRAM_BasePalette
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

    LDA.b #DX_PPU_CGRAM_BasePalette>>16
    STA $47

    LDA.b #DX_PPU_CGRAM_PaletteWriteMirror>>16
    STA $4A

    REP #$20
    LDA <offset>
    ASL
    CLC
    ADC <offset>
    CLC
    ADC #DX_PPU_CGRAM_BasePalette
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

    LDA.b #DX_PPU_CGRAM_BasePalette>>16
    STA $47

    LDA.b #DX_PPU_CGRAM_PaletteWriteMirror>>16
    STA $4A

    REP #$20
    LDA <offset>
    ASL
    CLC
    ADC <offset>
    CLC
    ADC #DX_PPU_CGRAM_BasePalette
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