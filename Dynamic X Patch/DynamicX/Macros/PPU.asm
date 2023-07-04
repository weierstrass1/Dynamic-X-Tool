macro ForcedTransferToVRAM(VRAMOffset, ResourceAddr, ResourceBNK, Lenght)

    PHX

    LDA #$00
    XBA
    LDA.l DZ_PPUMirrors_VRAM_Transfer_Length
    INC A
    STA.l DZ_PPUMirrors_VRAM_Transfer_Length
    REP #$20
    ASL
    TAX
    
    LDA <ResourceBNK>
    STA.l  DZ_PPUMirrors_VRAM_Transfer_SourceBNK,x
    LDA #$0000
    STA.l  DZ_PPUMirrors_VRAM_Transfer_SourceBNK+1,x

    LDA <ResourceAddr>
    STA.l DZ_PPUMirrors_VRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DZ_PPUMirrors_VRAM_Transfer_SourceLength,x

    LDA <VRAMOffset>
    STA.l DZ_PPUMirrors_VRAM_Transfer_Offset,x
    SEP #$20
    
    PLX

endmacro

macro TransferToVRAM(VRAMOffset, ResourceAddr, ResourceBNK, Lenght)

    PHX

    LDA #$00
    XBA
    LDA.l DZ_PPUMirrors_VRAM_Transfer_Length
    INC A
    STA.l DZ_PPUMirrors_VRAM_Transfer_Length
    REP #$20
    ASL
    TAX
    
    LDA <ResourceBNK>
    STA.l  DZ_PPUMirrors_VRAM_Transfer_SourceBNK,x
    LDA #$0000
    STA.l  DZ_PPUMirrors_VRAM_Transfer_SourceBNK+1,x

    LDA <ResourceAddr>
    STA.l DZ_PPUMirrors_VRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DZ_PPUMirrors_VRAM_Transfer_SourceLength,x
    CLC
    ADC DZ_CurrentDataSend
    STA DZ_CurrentDataSend

    LDA <VRAMOffset>
    STA.l DZ_PPUMirrors_VRAM_Transfer_Offset,x
    SEP #$20
    
    PLX

endmacro

macro ForcedTransferToCGRAM(CGRAMOffset, TableAddr, TableBNK, Lenght)

    PHX

    LDA #$00
    XBA
    LDA.l DZ_PPUMirrors_CGRAM_Transfer_Length
    INC A
    PHA
    STA.l DZ_PPUMirrors_CGRAM_Transfer_Length
    REP #$30
    ASL
    TAX

    LDA <TableAddr>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_SourceLength,x
    SEP #$30
    
    PLX
    LDA <TableBNK>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_SourceBNK,x

    LDA.b <CGRAMOffset>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_Offset,x

    PLX

endmacro

macro TransferToCGRAM(CGRAMOffset, TableAddr, TableBNK, Lenght)

    PHX

    LDA #$00
    XBA
    LDA.l DZ_PPUMirrors_CGRAM_Transfer_Length
    INC A
    PHA
    STA.l DZ_PPUMirrors_CGRAM_Transfer_Length
    REP #$30
    ASL
    TAX

    LDA <TableAddr>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_Source,x

    LDA <Lenght>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_SourceLength,x
    CLC
    ADC DZ_CurrentDataSend
    STA DZ_CurrentDataSend
    SEP #$30
    
    PLX
    LDA.b <TableBNK>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_SourceBNK,x

    LDA.b <CGRAMOffset>
    STA.l DZ_PPUMirrors_CGRAM_Transfer_Offset,x

    PLX

endmacro

macro TransferToCGRAMBuffer(CGRAMOffset, Lenght)

    PHX

    LDA #$00
    XBA
    LDA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Length
    INC A
    PHA
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Length
    REP #$30
    ASL
    TAX

    LDA.w #DZ_PPUMirrors_CGRAM_PaletteCopy
    CLC
    ADC.w #<CGRAMOffset>*2
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Destination,x

    LDA <Lenght>
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_SourceLength,x
    SEP #$30
    
    PLX
    LDA.b #DZ_PPUMirrors_CGRAM_PaletteCopy>>16
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_DestinationBNK,x

    LDA.b #<CGRAMOffset>
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Offset,x

    PLX

endmacro

macro TransferToCGRAMBufferNoConstant(CGRAMOffset, Lenght)

    PHX

    LDA #$00
    XBA
    LDA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Length
    INC A
    PHA
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Length
    REP #$30
    ASL
    TAX

    LDA <CGRAMOffset>
    CLC
    ASL
    CLC
    ADC #DZ_PPUMirrors_CGRAM_PaletteCopy
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Destination,x

    LDA <Lenght>
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_SourceLength,x
    SEP #$30
    
    PLX
    LDA.b #DZ_PPUMirrors_CGRAM_PaletteCopy>>16
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_DestinationBNK,x

    LDA <CGRAMOffset>
    STA.l DZ_PPUMirrors_CGRAM_BufferTransfer_Offset,x

    PLX

endmacro