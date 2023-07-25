?DXExtendedUpload:
    STA $00
    SEP #$20

    LDA DX_Timer
    CMP !ExtendedSafeFrameLowByte,x
    BNE ?+
    LDA DX_Timer+1
    CMP !ExtendedSafeFrameHighByte,x
    BEQ ?.NotFound
?+
    ;If current Pose equals last Pose then just update DX_Timer and get
    ;Graphics parameters
    LDA !ExtendedPoseIndex,x
    CMP !ExtendedLastPoseIndex,x
    BNE ?.Request
    LDA !ExtendedLastPoseHashIndex,x
    TAX
    JSL !SetPropertyAndOffset
    LDX $15E9|!addr

    LDA DX_Timer
    STA !ExtendedSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !ExtendedSafeFrameHighByte,x
    SEC
RTL
?.Request
    LDA #$00
    XBA
    LDA !ExtendedPoseIndex,x
    REP #$30
    CLC
    ADC $00
    TAY
    SEP #$20
    JSL !TakeDynamicRequest
    BCS ?.Found

?.NotFound
    LDX !SpriteIndex
    LDA !ExtendedLastPoseHashIndex,x
    CMP #$FF
    BNE ?.UseOld
    CLC
RTL
?.UseOld
    TAX
    JSL !SetPropertyAndOffset
    LDX $15E9|!addr

    LDA DX_Timer
    STA !ExtendedSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !ExtendedSafeFrameHighByte,x
    SEC
RTL
?.Found
    TXA
    LDX $15E9|!addr
    STA !ExtendedLastPoseHashIndex,x

    LDA DX_Timer
    STA !ExtendedSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !ExtendedSafeFrameHighByte,x

    LDA !ExtendedPoseIndex,x
    STA !ExtendedLastPoseIndex,x
    SEC
RTL
    