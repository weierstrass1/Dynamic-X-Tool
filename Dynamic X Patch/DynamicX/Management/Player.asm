
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

PlayerDynamicRoutine:
    LDX $0100|!addr
    LDA.l GameModeTable,x
    BNE +
    JML $00F69E|!rom
+
    LDA.l DX_Dynamic_Player_GFX_Enable
    BNE +
    JMP .pal
+
    REP #$20                  ; Accum (16 bit) 
    LDX.B #$00
    LDA $09
    ORA.W #$0800
    CMP $09
    BEQ +           
    CLC                       
+
    AND.w #$F700              
    ROR                       
    LSR                       
    ADC.l DX_Dynamic_Player_GFX_Addr       
    %CheckAndSendDMA($0D85, #$6000, DX_Dynamic_Player_GFX_BNK, #$0040)             
    CLC                       
    ADC.W #$0200     
    %CheckAndSendDMA($0D8F, #$6100, DX_Dynamic_Player_GFX_BNK, #$0040)                    
    LDX.B #$00                
    LDA $0A                   
    ORA.W #$0800              
    CMP $0A                   
    BEQ +           
    CLC                       
+
    AND.W #$F700              
    ROR                       
    LSR                       
    ADC.l DX_Dynamic_Player_GFX_Addr 
    %CheckAndSendDMA($0D87, #$6020, DX_Dynamic_Player_GFX_BNK, #$0040)                           
    CLC                       
    ADC.W #$0200       
    %CheckAndSendDMA($0D91, #$6120, DX_Dynamic_Player_GFX_BNK, #$0040)                     
    LDA $0B                   
    AND.W #$FF00              
    LSR                       
    LSR                       
    LSR                       
    ADC.l DX_Dynamic_Player_GFX_Addr       
    %CheckAndSendDMA($0D89, #$6040, DX_Dynamic_Player_GFX_BNK, #$0040)                     
    CLC                       
    ADC.W #$0200  
    %CheckAndSendDMA($0D93, #$6140, DX_Dynamic_Player_GFX_BNK, #$0040)                       
    LDA $0C                   
    AND.W #$FF00              
    LSR                       
    LSR                       
    LSR                       
    ADC.l DX_Dynamic_Player_GFX_Addr     
    %CheckAndSendDMA($0D99, #$67F0, DX_Dynamic_Player_GFX_BNK, #$0020)               
    SEP #$20     
.pal

    LDA.l DX_Dynamic_Player_Palette_Enable
    BNE +
    LDA.B #$0A                
    STA.W $0D84|!addr     
JML $00F69E|!rom
+
    REP #$20
    LDA.l DX_Dynamic_Player_Palette_BNK
    AND #$00FF
    BNE +
    LDA $0D82|!addr
    STA.l DX_Dynamic_Player_Palette_Addr
+
    LDA.l DX_Dynamic_Player_Palette_Addr
    CMP.l DX_PPU_CGRAM_LastPlayerPal
    BNE +

    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND #$0080
    BEQ ++

    LDA DX_Dynamic_Palettes_GlobalEffectID
    CMP DX_Dynamic_Palettes_LastGlobalEffectID+$10
    BNE +

++
    SEP #$20
    LDA.B #$0A                
    STA.W $0D84|!addr               

JML $00F69E|!rom
+
    STA.l DX_PPU_CGRAM_LastPlayerPal

    if !PaletteEffects
    
    SEP #$20
    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND #$80
    BNE .WithPalEffect
    SEP #$20
    %ForcedTransferToCGRAM(#$86, DX_Dynamic_Player_Palette_Addr, DX_Dynamic_Player_Palette_BNK, #$0014)
    SEP #$20
                ; Accum (8 bit) 

    LDA.B #$0A                
    STA.W $0D84|!addr               

JML $00F69E|!rom
.WithPalEffect
    LDA DX_Dynamic_Player_Palette_BNK
    STA $8C
    REP #$20
    LDA DX_Dynamic_Player_Palette_Addr
    STA $8A

    LDY #$00
    LDA [$8A]
    STA DX_PPU_CGRAM_PaletteCopy+($010C+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($010E+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($0110+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($0112+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($0114+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($0116+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($0118+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($011A+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($011C+$00)
    INY
    INY
    LDA [$8A],y
    STA DX_PPU_CGRAM_PaletteCopy+($011E+$00)

    LDA #$FFFF
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$10
    SEP #$20
    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
    AND #$7F
    STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
    LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
    AND #$7F
    STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded

    LDA DX_PPU_CGRAM_SPPaletteCopyLoaded
    ORA #$80
    STA DX_PPU_CGRAM_SPPaletteCopyLoaded

    LDA.B #$0A                
    STA.W $0D84|!addr            

JML $00F69E|!rom
else
    SEP #$20
    %ForcedTransferToCGRAM(#$86, DX_Dynamic_Player_Palette_Addr, DX_Dynamic_Player_Palette_BNK, #$0014)
    SEP #$20
                ; Accum (8 bit) 

    LDA.B #$0A                
    STA.W $0D84|!addr               

JML $00F69E|!rom
endif
PodooboDMA:
    REP #$20                  ; Accum (16 bit) 
    LDA.W #$0008              
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    CLC                       
    ADC.W #$8500              
    %CheckAndSendDMA($0D8B, #$6060, #$007E, #$0040)               
    CLC                       
    ADC.W #$0200   
    %CheckAndSendDMA($0D95, #$6160, #$007E, #$0040)                          
    SEP #$20                  ; Accum (8 bit) 
JML $01E1B7|!rom

YoshiDMA:
    REP #$20                  ; Accum (16 bit) 
    LDA $00                   
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    CLC                       
    ADC.W #$8500              
    %CheckAndSendDMA($0D8B, #$6060, #$007E, #$0040)              
    CLC                       
    ADC.W #$0200              
    %CheckAndSendDMA($0D95, #$6160, #$007E, #$0040)               
    LDA $02                   
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    CLC                       
    ADC.W #$8500              
    %CheckAndSendDMA($0D8D, #$6080, #$007E, #$0040)                 
    CLC                       
    ADC.W #$0200              
    %CheckAndSendDMA($0D97, #$6180, #$007E, #$0040)                  
    SEP #$20                  ; Accum (8 bit) 
JML $01EED8|!rom

IDKDMA:
    REP #$20                  ; Accum (16 bit) 
    LDA $00                   
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    ASL                       
    CLC                       
    ADC.W #$8500              
    %CheckAndSendDMA($0D8B, #$6060, #$007E, #$0040)               
    CLC                       
    ADC.W #$0200              
    %CheckAndSendDMA($0D95, #$6160, #$007E, #$0040)                
    SEP #$20                  ; Accum (8 bit) 
JML $02EA4D|!rom