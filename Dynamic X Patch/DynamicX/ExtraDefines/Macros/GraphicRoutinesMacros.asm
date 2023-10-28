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
macro Draw(offset, sprite, lastPoseHashIndex, lastFlip, palette, xl, xh, yl, yh)
?DXDraw:
    STA !PoseID
    SEP #$20

    LDA <lastPoseHashIndex>
    CMP #$FF
    BNE ?.continue
RTS
?.continue
    TAX
    JSL !SetPropertyAndOffset
if <sprite> == 1
    LDX !SpriteIndex
endif

    LDA <lastFlip>
    ROR
    ROR
    ROR
    AND #$C0
    ORA !Property
    ORA !PropParam
    ORA <palette>
    STA !Property

if <offset> == 1
    STZ !YOffsetParam+1
    LDA !YOffsetParam
    BPL ?.skipYNeg
    DEC !YOffsetParam+1
?.skipYNeg
endif

    LDA <yh>
    XBA
    LDA <yl>
    REP #$20
if <offset> == 1
    CLC
    ADC !YOffsetParam
endif
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

if <offset> == 1
    STZ !XOffsetParam+1
    LDA !XOffsetParam
    BPL ?.skipXNeg
    DEC !XOffsetParam+1
?.skipXNeg
endif

    LDA <xh>
    XBA
    LDA <xl>
    REP #$20
if <offset> == 1
    CLC
    ADC !XOffsetParam
endif
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    JSL !Draw
RTS
endmacro
macro StandardSpriteDraw(offset, type)
    %Draw("<offset>", 1, "!<type>LastPoseHashIndex,x", "!<type>LastFlip,x", "!<type>Palette,x", "!<type>XLow,x", "!<type>XHigh,x", "!<type>YLow,x", "!<type>YHigh,x")
endmacro