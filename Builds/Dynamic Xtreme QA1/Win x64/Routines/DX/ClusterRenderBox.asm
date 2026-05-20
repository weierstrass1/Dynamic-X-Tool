?DXClusterRenderBox:
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
    BCS ?.OutOfScreen
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
    BCS ?.OutOfScreen
?+
    SEC
RTL

?.OutOfScreen
    LDA #$FF
    STA !ClusterPalette,x
    STA !ClusterLastPoseIndex,x
    STA !ClusterLastPoseHashIndex,x
    STA !ClusterLastVersion,x
    
    LDA !ClusterGlobalFlip,x
    EOR !ClusterLocalFlip,x
    STA !ClusterLastFlip,x
    CLC
RTL