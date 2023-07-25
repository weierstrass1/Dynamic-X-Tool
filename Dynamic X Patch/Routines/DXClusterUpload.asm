?DXClusterUpload:
    STA $00
    SEP #$20

    LDA DX_Timer
    CMP !ClusterSafeFrameLowByte,x
    BNE ?+
    LDA DX_Timer+1
    CMP !ClusterSafeFrameHighByte,x
    BEQ ?.NotFound
?+
    ;If current Pose equals last Pose then just update DX_Timer and get
    ;Graphics parameters
    LDA !ClusterPoseIndex,x
    CMP !ClusterLastPoseIndex,x
    BNE ?.Request
    LDA !ClusterLastPoseHashIndex,x
    TAX
    JSL !SetPropertyAndOffset
    LDX $15E9|!addr

    LDA DX_Timer
    STA !ClusterSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !ClusterSafeFrameHighByte,x
    SEC
RTL
?.Request
    LDA #$00
    XBA
    LDA !ClusterPoseIndex,x
    REP #$30
    CLC
    ADC $00
    TAY
    SEP #$20
    JSL !TakeDynamicRequest
    BCS ?.Found

?.NotFound
    LDX !SpriteIndex
    LDA !ClusterLastPoseHashIndex,x
    CMP #$FF
    BNE ?.UseOld
    CLC
RTL
?.UseOld
    TAX
    JSL !SetPropertyAndOffset
    LDX $15E9|!addr

    LDA DX_Timer
    STA !ClusterSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !ClusterSafeFrameHighByte,x
    SEC
RTL
?.Found
    TXA
    LDX $15E9|!addr
    STA !ClusterLastPoseHashIndex,x

    LDA DX_Timer
    STA !ClusterSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !ClusterSafeFrameHighByte,x

    LDA !ClusterPoseIndex,x
    STA !ClusterLastPoseIndex,x
    SEC
RTL
    