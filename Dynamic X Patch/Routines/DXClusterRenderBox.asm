?DXClusterRenderBox:
    STZ !SpriteHOffScreenFlag,x
    STZ !SpriteVOffScreenFlag,x
    LDA !cluster_x_high,x
    XBA
    LDA !cluster_x_low,x
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
    CMP !ClusterRenderXDistanceOutOfScreen,x
    BCC ?+
    INC !SpriteHOffScreenFlag,x
?+
    LDA !cluster_y_high,x
    XBA
    LDA !cluster_y_low,x
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
    CMP !ClusterRenderYDistanceOutOfScreen,x
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