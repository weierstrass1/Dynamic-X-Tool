macro GeneralUpload(Pose16, sprite, flip, safeCheck, withoutOffset, safeLow, safeHigh, poseIndex, lastPoseIndex, lastPoseHashIndex, globalFlip, localFlip, lastFlip)
?DXUpload:
    STA $00
    SEP #$20

    LDA $0100|!addr
    TAX
    LDA.l ?.GameModeAllowed,x
    BNE ?+

if <sprite> != 0
    LDX !SpriteIndex
endif

	LDA <lastPoseHashIndex>
	CMP #$FF
	BNE ?++
	CLC
RTS
?++

    ASL
    TAX

    LDA DX_Timer
    STA DX_Dynamic_Pose_TimeLastUse,x
    LDA DX_Timer+1
    STA DX_Dynamic_Pose_TimeLastUse+1,x

if <sprite> != 0
    LDX !SpriteIndex
endif

    SEC
RTS
?+

if <sprite> != 0
    LDX !SpriteIndex
endif

if <safeCheck>
    LDA DX_Timer
    CMP <safeLow>
    BNE ?.continue
    LDA DX_Timer+1
    CMP <safeHigh>
    BEQ ?.NotFound
?.continue
endif
    LDA DX_Timer
    STA <safeLow>
    LDA DX_Timer+1
    STA <safeHigh>
    
    ;If current Pose equals last Pose then just update DX_Timer and get
    ;Graphics parameters
if <sprite> != 2
if <Pose16> == 0
    LDA <poseIndex>
    CMP <lastPoseIndex>
else
    REP #$20
    LDA <poseIndex>
    CMP <lastPoseIndex>
    SEP #$20
endif
    BNE ?.Request
if <flip> == 1
    LDA <globalFlip>
    EOR <localFlip>
    STA <lastFlip>
endif
    SEC
RTS
?.Request
endif

if <withoutOffset> == 0
if <Pose16> == 0
    LDA #$00
    XBA
    LDA <poseIndex>
    REP #$30
else
    REP #$30
    LDA <poseIndex>
endif
    CLC
    ADC $00
    TAY
    SEP #$20
else
    REP #$10
    LDY.b $00
endif
    JSL !TakeDynamicRequest
    BCS ?.Found

?.NotFound
if <sprite> != 0
    LDX !SpriteIndex
endif
    LDA <lastPoseHashIndex>
    CMP #$FF
    BNE ?.UseOld
    CLC
RTS
?.UseOld
    SEC
RTS
?.Found
    TXA
if <sprite> != 0
    LDX !SpriteIndex
endif
    STA <lastPoseHashIndex>

if <sprite> != 2
    if <Pose16> == 0
        LDA <poseIndex>
        STA <lastPoseIndex>
    else
        REP #$20
        LDA <poseIndex>
        STA <lastPoseIndex>
        SEP #$20
    endif
endif
if <flip> == 1
    LDA <globalFlip>
    EOR <localFlip>
    STA <lastFlip>
endif
    SEC
RTS
?.GameModeAllowed
    ;   00  01  02  03  04  05  06  07
    db $00,$00,$00,$00,$00,$00,$00,$01
    ;   08  09  0A  0B  0C  0D  0E  0F
    db $00,$00,$00,$00,$00,$00,$01,$00
    ;   10  11  12  13  14  15  16  17
    db $00,$00,$00,$00,$01,$00,$00,$00
    ;   18  19  1A  1B  1C  1D  1E  1F
    db $00,$00,$00,$01,$00,$00,$00,$01
    ;   20  21  22  23  24  25  26  27
    db $00,$00,$00,$00,$00,$01,$00,$00
    ;   28  29  2A  2B  2C  2D  2E  2F
    db $00,$01,$00,$00,$00,$00,$00,$00
endmacro
macro Upload(sprite, flip, safeCheck, withoutOffset, safeLow, safeHigh, poseIndex, lastPoseIndex, lastPoseHashIndex, globalFlip, localFlip, lastFlip)
    %GeneralUpload(0, "<sprite>", "<flip>", "<safeCheck>", "<withoutOffset>", "<safeLow>", "<safeHigh>", "<poseIndex>", "<lastPoseIndex>", "<lastPoseHashIndex>", "<globalFlip>", "<localFlip>", "<lastFlip>")
endmacro
macro StandardSpriteUpload(withoutOffset, type)
    %Upload(1, 1, 1, "<withoutOffset>", "!<type>SafeFrameLowByte,x", "!<type>SafeFrameHighByte,x", "!<type>PoseIndex,x", "!<type>LastPoseIndex,x", "!<type>LastPoseHashIndex,x", "!<type>GlobalFlip,x", "!<type>LocalFlip,x", "!<type>LastFlip,x")
endmacro