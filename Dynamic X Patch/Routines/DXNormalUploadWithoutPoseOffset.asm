DXNormalUploadWithoutPoseOffset:
    STA $00
    SEP #$20

    LDA DX_Timer
    CMP !NormalSafeFrameLowByte,x
    BNE ?+
    LDA DX_Timer+1
    CMP !NormalSafeFrameHighByte,x
    BEQ ?.NotFound
?+
    ;If current Pose equals last Pose then just update DX_Timer and get
    ;Graphics parameters
    LDA !NormalPoseIndex,x
    CMP !NormalLastPoseIndex,x
    BNE ?.Request
    LDA !NormalLastPoseHashIndex,x
    TAX
    JSL !SetPropertyAndOffset
    LDX $15E9|!addr

    LDA DX_Timer
    STA !NormalSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !NormalSafeFrameHighByte,x
    SEC
RTL
?.Request

    REP #$10
    LDY $00|!dp
    JSL !TakeDynamicRequest
    BCS ?.Found

?.NotFound
    LDX !SpriteIndex
    LDA !NormalLastPoseHashIndex,x
    CMP #$FF
    BNE ?.UseOld
?.Failed
    CLC
RTL
?.UseOld
    TAX
    JSL !SetPropertyAndOffset
    LDX $15E9|!addr

    LDA DX_Timer
    STA !NormalSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !NormalSafeFrameHighByte,x
    SEC
RTL
?.Found
    TXA
    LDX $15E9|!addr
    STA !NormalLastPoseHashIndex,x

    LDA DX_Timer
    STA !NormalSafeFrameLowByte,x
    LDA DX_Timer+1
    STA !NormalSafeFrameHighByte,x

    LDA !NormalPoseIndex,x
    STA !NormalLastPoseIndex,x

    LDA !NormalGlobalFlip,x
    EOR !NormalLocalFlip,x
    STA !NormalLastFlip,x
    SEC
RTL
    