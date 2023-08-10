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

macro ForcedTransferToVRAMFromAddr(VRAMOffset, addr, Lenght)
    %ForcedTransferToVRAM("<VRAMOffset>", #<addr>, #((<addr>>>16)&$00FF), "<Lenght>")
endmacro

macro ForcedTransferResourceToVRAMWithLenght(VRAMOffset, ResourceName, Lenght)
    %ForcedTransferToVRAMFromAddr("<VRAMOffset>", "!Resource<ResourceName>", "<Lenght>")
endmacro

macro ForcedTransferResourceToVRAM(VRAMOffset, ResourceName)
    %ForcedTransferToVRAMFromAddr("<VRAMOffset>", "!Resource<ResourceName>", "#!Resource<ResourceName>Size")
endmacro

macro TransferToVRAMFromAddr(VRAMOffset, addr, Lenght)
    %TransferToVRAM("<VRAMOffset>", #<addr>, #((<addr>>>16)&$00FF), "<Lenght>")
endmacro

macro TransferResourceToVRAMWithLenght(VRAMOffset, ResourceName, Lenght)
    %TransferToVRAMFromAddr("<VRAMOffset>", "!Resource<ResourceName>", "<Lenght>")
endmacro

macro TransferResourceToVRAM(VRAMOffset, ResourceName)
    %TransferToVRAMFromAddr("<VRAMOffset>", "!Resource<ResourceName>", "#!Resource<ResourceName>Size")
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
    LDA <TableBNK>
    STA.l DX_PPU_CGRAM_Transfer_SourceBNK,x

    LDA.b <CGRAMOffset>
    STA.l DX_PPU_CGRAM_Transfer_Offset,x
    SEC
?++
endmacro

macro ForcedTransferToCGRAMFromAddr(CGRAMOffset, addr, Lenght)
    %ForcedTransferToCGRAM("<CGRAMOffset>", #<addr>, #<addr>>>16, "<Lenght>")
endmacro

macro ForcedTransferResourceToCGRAMWithLenght(CGRAMOffset, ResourceName, Lenght)
    %ForcedTransferToCGRAMFromAddr("<CGRAMOffset>", "!Resource<ResourceName>", "<Lenght>")
endmacro

macro ForcedTransferResourceToCGRAM(CGRAMOffset, ResourceName)
    %ForcedTransferToCGRAMFromAddr("<CGRAMOffset>", "!Resource<ResourceName>", "#!Resource<ResourceName>Size")
endmacro

macro TransferToCGRAMFromAddr(CGRAMOffset, addr, Lenght)
    %TransferToCGRAM("<CGRAMOffset>", #<addr>, #<addr>>>16, "<Lenght>")
endmacro

macro TransferResourceToCGRAMWithLenght(CGRAMOffset, ResourceName, Lenght)
    %TransferToCGRAMFromAddr("<CGRAMOffset>", "!Resource<ResourceName>", "<Lenght>")
endmacro

macro TransferResourceToCGRAM(CGRAMOffset, ResourceName)
    %TransferToCGRAMFromAddr("<CGRAMOffset>", "!Resource<ResourceName>", "#!Resource<ResourceName>Size")
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
    LDA #DX_PPU_CGRAM_PaletteCopy>>16
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
    LDA #DX_PPU_CGRAM_PaletteCopy>>16
    STA DX_PPU_CGRAM_BufferTransfer_DestinationBNK,x

    LDA <CGRAMOffset>
    STA DX_PPU_CGRAM_BufferTransfer_Offset,x

    SEC
?++
endmacro