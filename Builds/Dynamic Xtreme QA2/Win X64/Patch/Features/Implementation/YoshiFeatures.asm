if !PlayerFeatures == 0
macro CheckAndSendDMA(addr, vram, bnk, size)
    CMP <addr>|!addr
    BEQ ?+
    STA <addr>|!addr
    PHA
    SEP #$20
    %ForcedTransferToVRAM(<vram>, "<addr>|!addr", <bnk>, <size>)
    REP #$20
    PLA
?+   
endmacro
endif

YoshiDMA:
    PHX
    LDA DX_Dynamic_Yoshi_VRAMEnable
    BNE +
    PLX
JML $01EED8|!rom
+
    REP #$20                  ; Accum (16 bit)
    LDA $00
    ASL
    ASL
    ASL
    ASL
    ASL
    CLC
    ADC DX_Dynamic_Yoshi_Addr
    %CheckAndSendDMA($0D8B, #$6060, DX_Dynamic_Yoshi_BNK, #$0040)
    CLC
    ADC.W #$0200
    %CheckAndSendDMA($0D95, #$6160, DX_Dynamic_Yoshi_BNK, #$0040)
    LDA $02
    ASL
    ASL
    ASL
    ASL
    ASL
    CLC
    ADC DX_Dynamic_Yoshi_Addr
    %CheckAndSendDMA($0D8D, #$6080, DX_Dynamic_Yoshi_BNK, #$0040)
    CLC
    ADC.W #$0200
    %CheckAndSendDMA($0D97, #$6180, DX_Dynamic_Yoshi_BNK, #$0040)
    SEP #$20
    PLX
JML $01EED8|!rom

IDKDMA:
    PHX
    LDA DX_Dynamic_Yoshi_VRAMEnable
    BNE +
    PLX
JML $02EA4D|!rom
+
    REP #$20                  ; Accum (16 bit) 
    LDA $00                   
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    CLC                       
    ADC DX_Dynamic_Yoshi_Addr             
    %CheckAndSendDMA($0D8B, #$6060, DX_Dynamic_Yoshi_BNK, #$0040)               
    CLC                       
    ADC.W #$0200              
    %CheckAndSendDMA($0D95, #$6160, DX_Dynamic_Yoshi_BNK, #$0040)                
    SEP #$20                  ; Accum (8 bit) 
    PLX
JML $02EA4D|!rom

PodooboDMA:
    PHX
    LDA DX_Dynamic_Yoshi_VRAMEnable
    BNE +
    PLX
JML $01E1B7|!rom
+
    REP #$21                  ; Accum (16 bit) 
    LDA DX_Dynamic_Yoshi_Addr
    ADC.w #$0100             
    %CheckAndSendDMA($0D8B, #$6060, DX_Dynamic_Yoshi_BNK, #$0040)               
    CLC                       
    ADC.w #$0200
    %CheckAndSendDMA($0D95, #$6160, DX_Dynamic_Yoshi_BNK, #$0040)                          
    SEP #$20                  ; Accum (8 bit) 
    PLX
JML $01E1B7|!rom