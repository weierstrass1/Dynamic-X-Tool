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
;$0A = Max Tile Priority ($00 = Maximum, $01 = High, $02 = Normal, $03 = Lowest)
;$8A = X Offset
;$8B = Y Offset
;A = Base Pose ID, 16 bits
?DXNormalDrawWithOffset:
    STA !PoseID
    SEP #$20

    LDA !NormalGlobalFlip,x
    EOR !NormalLocalFlip,x
    ROR
    ROR
    ROR
    AND #$C0
    ORA !Property
    ORA !PropParam
    ORA !NormalPalette,x
    STA !Property

    STZ !YOffsetParam+1
    LDA !YOffsetParam
    BPL ?+
    DEC !YOffsetParam+1
?+

    LDA !sprite_y_high,x
    XBA
    LDA !sprite_y_low,x
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

    LDA !sprite_x_high,x
    XBA
    LDA !sprite_x_low,x
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