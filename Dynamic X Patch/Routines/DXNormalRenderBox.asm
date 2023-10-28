DXNormalRenderBox:
    STZ !SpriteHOffScreenFlag,x
    STZ !SpriteVOffScreenFlag,x
    LDA !sprite_x_high,x
    XBA
    LDA !sprite_x_low,x
    REP #$20
    SEC
    SBC $1A
    SBC #$0080
    BPL +
    EOR #$FFFF
    INC A
+
    SEC
    SBC #$0080
    SEP #$20
    BMI +
    CMP !NormalRenderXDistanceOutOfScreen,x
    BCC +
    INC !SpriteHOffScreenFlag,x
+
    LDA !sprite_y_high,x
    XBA
    LDA !sprite_y_low,x
    REP #$20
    SEC
    SBC $1C
    SBC #$0070
    BPL +
    EOR #$FFFF
    INC A
+
    SEC
    SBC #$0070
    SEP #$20
    BMI +
    CMP !NormalRenderYDistanceOutOfScreen,x
    BCC +
    INC !SpriteVOffScreenFlag,x
+
    LDA !SpriteHOffScreenFlag,x
    ORA !SpriteVOffScreenFlag,x
    BEQ +

    LDA #$FF
    STA !NormalPalette,x
    STA !NormalLastPoseIndex,x
    STA !NormalLastPoseHashIndex,x
    STA !NormalLastVersion,x

    LDA !NormalGlobalFlip,x
    EOR !NormalLocalFlip,x
    STA !NormalLastFlip,x
    CLC
RTL
+
    SEC
RTL