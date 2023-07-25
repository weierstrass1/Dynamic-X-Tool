!OutOfRangePal = GRatioTab-RRatioTab

?SpriteLevelPaletteWithLength:
    STA $05
    STZ $06

    LDA !LevelSpritePaletteType
    BNE ?.continue
    REP #$20
    LDA $05
    ASL
    STA $05
    SEP #$20
    %TransferToCGRAM($00, $02, $04, $05)
    LDX !SpriteIndex	
RTL
?.continue

    CMP.b #!OutOfRangePal
    BCC ?+
    LDX !SpriteIndex	
RTL
?+
    TAY

    PHB
    PHK
    PLB

    STZ $01
    LDA $05
    PHA
    LDA $00
    PHA

    %MixRGBDefault("RRatioTab,y","GRatioTab,y","BRatioTab,y","RValTab,y","GValTab,y","BValTab,y",$00,$05)							

    LDA #$00
    XBA
    PLA
    STA $00

    REP #$20
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteWriteMirror
    STA $02
    SEP #$20

    LDA #$00
    XBA
    PLA
    REP #$20
    ASL
    STA $05
    SEP #$20
    

	%TransferToCGRAM($00, $02, #DX_PPU_CGRAM_PaletteWriteMirror>>16, $05)	
    LDX !SpriteIndex	
    PLB		   
RTL

RRatioTab:
    db $00,$08,$08
    db $01,$02,$03,$04
    db $05,$06,$07,$08
    db $09,$0A,$0B,$0C
    db $0D,$0E,$0F,$10
GRatioTab:
    db $00,$08,$08
    db $01,$02,$03,$04
    db $05,$06,$07,$08
    db $09,$0A,$0B,$0C
    db $0D,$0E,$0F,$10
BRatioTab:
    db $00,$08,$08
    db $00,$00,$00,$00
    db $00,$00,$00,$00
    db $00,$00,$00,$00
    db $00,$00,$00,$00

RValTab:
    db $00,$00,$1F
    db $00,$00,$00,$00
    db $00,$00,$00,$00
    db $00,$00,$00,$00
    db $00,$00,$00,$00
    
GValTab:
    db $00,$1F,$00
    db $00,$00,$00,$00
    db $00,$00,$00,$00
    db $00,$00,$00,$00
    db $00,$00,$00,$00

BValTab:
    db $00,$00,$00
    db $06,$06,$06,$06
    db $06,$06,$06,$06
    db $06,$06,$06,$06
    db $06,$06,$06,$06