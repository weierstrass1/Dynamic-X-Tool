macro ForcedTransferToVRAM(VRAMOffset, ResourceAddr, ResourceBNK, Lenght)

    SEP #$30

    LDA #$00
    XBA
    LDA.l DX_PPU_VRAM_Transfer_Length
    ASL
    TAX
    LSR
    INC A
    STA.l DX_PPU_VRAM_Transfer_Length
    REP #$20
    
    LDA <ResourceBNK>
    STA.l  DX_PPU_VRAM_Transfer_SourceBNK,x
    LDA #$0000
    STA.l  DX_PPU_VRAM_Transfer_SourceBNK+1,x

    LDA <ResourceAddr>
    STA.l DX_PPU_VRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DX_PPU_VRAM_Transfer_SourceLength,x
    CLC
    ADC DX_Dynamic_CurrentDataSend
    STA DX_Dynamic_CurrentDataSend

    LDA <VRAMOffset>
    STA.l DX_PPU_VRAM_Transfer_Offset,x
    SEP #$20

endmacro

macro TransferToVRAM(VRAMOffset, ResourceAddr, ResourceBNK, Lenght)

    REP #$20
    LDA DX_Dynamic_CurrentDataSend
    CLC
    ADC <Lenght>
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BCC ?+
    BEQ ?+
    CLC
    BRA ?++
?+
    SEP #$30

    LDA #$00
    XBA
    LDA.l DX_PPU_VRAM_Transfer_Length
    ASL
    TAX
    LSR
    INC A
    STA.l DX_PPU_VRAM_Transfer_Length
    REP #$20
    
    LDA <ResourceBNK>
    STA.l  DX_PPU_VRAM_Transfer_SourceBNK,x
    LDA #$0000
    STA.l  DX_PPU_VRAM_Transfer_SourceBNK+1,x

    LDA <ResourceAddr>
    STA.l DX_PPU_VRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DX_PPU_VRAM_Transfer_SourceLength,x
    CLC
    ADC DX_Dynamic_CurrentDataSend
    STA DX_Dynamic_CurrentDataSend

    LDA <VRAMOffset>
    STA.l DX_PPU_VRAM_Transfer_Offset,x
    SEP #$20

    SEC
?++
endmacro

macro ForcedTransferToCGRAM(CGRAMOffset, TableAddr, TableBNK, Lenght)

    SEP #$30

    LDA #$00
    XBA
    LDA.l DX_PPU_CGRAM_Transfer_Length
    INC A
    PHA
    STA.l DX_PPU_CGRAM_Transfer_Length
    REP #$30
    ASL
    TAX

    LDA <TableAddr>
    STA.l DX_PPU_CGRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DX_PPU_CGRAM_Transfer_SourceLength,x
    CLC
    ADC DX_Dynamic_CurrentDataSend
    STA DX_Dynamic_CurrentDataSend
    SEP #$30
    
    PLX
    LDA <TableBNK>
    STA.l DX_PPU_CGRAM_Transfer_SourceBNK,x

    LDA.b <CGRAMOffset>
    STA.l DX_PPU_CGRAM_Transfer_Offset,x

endmacro

macro TransferToCGRAM(CGRAMOffset, TableAddr, TableBNK, Lenght)

    REP #$20
    LDA DX_Dynamic_CurrentDataSend
    CLC
    ADC <Lenght>
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BCC ?+
    BEQ ?+
    CLC
    BRA ?++
?+
    SEP #$30

    LDA #$00
    XBA
    LDA.l DX_PPU_CGRAM_Transfer_Length
    INC A
    PHA
    STA.l DX_PPU_CGRAM_Transfer_Length
    REP #$30
    ASL
    TAX

    LDA <TableAddr>
    STA.l DX_PPU_CGRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DX_PPU_CGRAM_Transfer_SourceLength,x
    CLC
    ADC DX_Dynamic_CurrentDataSend
    STA DX_Dynamic_CurrentDataSend
    SEP #$30
    
    PLX
    LDA.b <TableBNK>
    STA.l DX_PPU_CGRAM_Transfer_SourceBNK,x

    LDA.b <CGRAMOffset>
    STA.l DX_PPU_CGRAM_Transfer_Offset,x
    SEC
?++
endmacro

macro TransferToCGRAMBuffer(CGRAMOffset, Lenght)

    REP #$20
    LDA DX_Dynamic_CurrentDataSend
    CLC
    ADC <Lenght>
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BCC ?+
    BEQ ?+
    CLC
    BRA ?++
?+
    SEP #$30
    LDA #$00
    XBA
    LDA.l DX_PPU_CGRAM_BufferTransfer_Length
    INC A
    PHA
    STA.l DX_PPU_CGRAM_BufferTransfer_Length
    REP #$30
    ASL
    TAX

    LDA.w #DX_PPU_CGRAM_PaletteCopy
    CLC
    ADC.w #<CGRAMOffset>*2
    STA.l DX_PPU_CGRAM_BufferTransfer_Destination,x

    LDA <Lenght>
    STA.l DX_PPU_CGRAM_BufferTransfer_SourceLength,x
    CLC
    ADC DX_Dynamic_CurrentDataSend
    STA DX_Dynamic_CurrentDataSend
    SEP #$30
    
    PLX
    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA.l DX_PPU_CGRAM_BufferTransfer_DestinationBNK,x

    LDA.b #<CGRAMOffset>
    STA.l DX_PPU_CGRAM_BufferTransfer_Offset,x
    SEC
?++

endmacro

macro TransferToCGRAMBufferNoConstant(CGRAMOffset, Lenght)

    REP #$20
    LDA DX_Dynamic_CurrentDataSend
    CLC
    ADC <Lenght>
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BCC ?+
    BEQ ?+
    CLC
    BRA ?++
?+
    SEP #$30

    LDA #$00
    XBA
    LDA DX_PPU_CGRAM_BufferTransfer_Length
    INC A
    PHA
    STA DX_PPU_CGRAM_BufferTransfer_Length
    REP #$30
    ASL
    TAX

    LDA <CGRAMOffset>
    CLC
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteCopy
    STA DX_PPU_CGRAM_BufferTransfer_Destination,x

    LDA <Lenght>
    STA DX_PPU_CGRAM_BufferTransfer_SourceLength,x
    CLC
    ADC DX_Dynamic_CurrentDataSend
    STA DX_Dynamic_CurrentDataSend
    SEP #$30
    
    PLX
    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA DX_PPU_CGRAM_BufferTransfer_DestinationBNK,x

    LDA <CGRAMOffset>
    STA DX_PPU_CGRAM_BufferTransfer_Offset,x

    SEC
?++
endmacro