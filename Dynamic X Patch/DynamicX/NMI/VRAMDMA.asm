VRAMDMA:
    LDA #$00
    XBA
    LDA DX_PPU_VRAM_Transfer_Length
    BNE +
RTS
+
    LDX #$80                
	STX $2115

    REP #$30
    DEC A
    ASL
    TAX
	LDA #$1801              
	STA $20             ;parameter of DMA
-
    LDA DX_PPU_VRAM_Transfer_SourceBNK,x
    STA $24

    LDA DX_PPU_VRAM_Transfer_SourceLength,x
    STA $25                 ;Load Length

    LDA DX_PPU_VRAM_Transfer_Offset,x
    STA $2116               ;Loads VRAM destination

    LDA DX_PPU_VRAM_Transfer_Source,x
    STA $22                 ;Load Resource

    STY $420B               ;Start the DMA Transfer

    DEX
    DEX 
    BPL -

    SEP #$30
    LDA #$00
    STA DX_PPU_VRAM_Transfer_Length
RTS
