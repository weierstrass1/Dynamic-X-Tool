CGRAMDMA:
    LDA.l DX_PPU_CGRAM_Transfer_Length
    BPL +
RTS
+
    PHA
    ASL
    TAX

    REP #$20
	LDA.w #$2202              
	STA $20             ;parameter of DMA
-
    LDA.l DX_PPU_CGRAM_Transfer_SourceLength,x
    STA $25
    
    LDA.l DX_PPU_CGRAM_Transfer_Source,x
    STA $22                 ;Load Resource
    SEP #$20

    PLX

    LDA.l DX_PPU_CGRAM_Transfer_SourceBNK,x
    STA $24

    LDA.l DX_PPU_CGRAM_Transfer_Offset,x
    STA $2121

    STY $420B

    TXA
    DEC A
    BMI +

    PHA
    REP #$20
    ASL
    TAX
    BRA -
+
    LDA.b #$FF
    STA.l DX_PPU_CGRAM_Transfer_Length
RTS

if !PalettesEffects
PaletteColor00NMI:
    LDA DX_PPU_PaletteColor00_Update
    BNE +
RTS
+
    STZ $2121
    LDA DX_PPU_PaletteColor00_Result
    STA $2122
    LDA DX_PPU_PaletteColor00_Result+1
    STA $2122

    LDA #$00
    STA DX_PPU_PaletteColor00_Update
RTS
endif
