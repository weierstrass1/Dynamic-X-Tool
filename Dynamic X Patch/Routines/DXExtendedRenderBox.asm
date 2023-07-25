?DXExtendedRenderBox:
    STZ !SpriteHOffScreenFlag,x
    STZ !SpriteVOffScreenFlag,x
    LDA !extended_x_high,x
    XBA
    LDA !extended_x_low,x
    REP #$20
    SEC
    SBC $1A
    SBC #$0080
    BPL ?+
    EOR #$FFFF
    INC A
?+
    SEC
    SBC #$0080
    SEP #$20
    BMI ?+
    CMP !ExtendedRenderXDistanceOutOfScreen,x
    BCC ?+
    INC !SpriteHOffScreenFlag,x
?+
    LDA !extended_y_high,x
    XBA
    LDA !extended_y_low,x
    REP #$20
    SEC
    SBC $1C
    SBC #$0070
    BPL ?+
    EOR #$FFFF
    INC A
?+
    SEC
    SBC #$0070
    SEP #$20
    BMI ?+
    CMP !ExtendedRenderYDistanceOutOfScreen,x
    BCC ?+
    INC !SpriteVOffScreenFlag,x
?+
    LDA !SpriteHOffScreenFlag,x
    ORA !SpriteVOffScreenFlag,x
    BEQ ?+
    CLC
RTL
?+
    SEC
RTL