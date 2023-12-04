?DXExtendedRenderBox:
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
    BCS ?.OutOfScreen
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
    BCS ?.OutOfScreen
?+
    SEC
RTL

?.OutOfScreen
    LDA #$FF
    STA !ExtendedPalette,x
    STA !ExtendedLastPoseIndex,x
    STA !ExtendedLastPoseHashIndex,x
    STA !ExtendedLastVersion,x

    LDA !ExtendedGlobalFlip,x
    EOR !ExtendedLocalFlip,x
    STA !ExtendedLastFlip,x
    CLC
RTL