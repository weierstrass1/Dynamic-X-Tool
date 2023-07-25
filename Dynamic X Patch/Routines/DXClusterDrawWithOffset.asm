!XOffSet = $00
!YOffSet = $02
!Property = $04
!Tile = $05
!PoseID = $06
!PoseOffset = $08
!MaxTilePriority = $0A

!PropParam = $52
!MaxTilePriorityParam = $0A
!XOffsetParam = $8A
!YOffsetParam = $8B

;$52 = Property (YXCC ----)
;$0A = Max Tile Priority ($00 = Maximum, $01 = High, $02 = Cluster, $03 = Lowest)
;$8A = X Offset
;$8B = Y Offset
;A = Base Pose ID, 16 bits
?DXClusterDrawWithOffset:
    STA !PoseID
    SEP #$20

    LDA !ClusterGlobalFlip,x
    EOR !ClusterLocalFlip,x
    ROR
    ROR
    ROR
    AND #$C0
    ORA !Property
    ORA !PropParam
    ORA !ClusterPalette,x
    STA !Property

    STZ !YOffsetParam+1
    LDA !YOffsetParam
    BPL ?+
    DEC !YOffsetParam+1
?+

    LDA !cluster_y_high,x
    XBA
    LDA !cluster_y_low,x
    REP #$20
    CLC
    ADC !YOffsetParam
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

    STZ !XOffsetParam+1
    LDA !XOffsetParam
    BPL ?+
    DEC !XOffsetParam+1
?+

    LDA !cluster_x_high,x
    XBA
    LDA !cluster_x_low,x
    REP #$20
    CLC
    ADC !XOffsetParam
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    JSL !Draw
    SEC
RTL