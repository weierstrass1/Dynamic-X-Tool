!XOffSet = $00
!YOffSet = $02
!Property = $04
!Tile = $05
!PoseID = $06
!PoseOffset = $08
!MaxTilePriority = $0A

!GraphicRoutine = $45
!Iterator = !GraphicRoutine+3
!maxtile_pointer = !Iterator+2

!maxtile_pointer_max        = $6180       ; 16 bytes
!maxtile_pointer_high       = $6190       ; 16 bytes
!maxtile_pointer_normal     = $61A0       ; 16 bytes
!maxtile_pointer_low        = $61B0       ; 16 bytes

!TileX = $0B
!TileY = $0D
!TileSize = $0F

YIsValid:
    STZ !TileY+1
    LDA !TileY
    BPL .posY
.negY
    DEC !TileY+1
.posY
    REP #$21
    LDA !TileY
    ADC !YOffSet
    BPL .posYOffset
.negYOffset
    CMP #$FFF0
    BCS .continueY
    SEP #$20
    CLC
RTL
.posYOffset
    CMP #$00E0
    BCC .continueY
    SEP #$20
    CLC
RTL
.continueY
    SEP #$20
    STA !TileY
    SEC
RTL

XIsValid:
    STZ !TileX+1
    LDA !TileX
    BPL .posX
.negX
    DEC !TileX+1
.posX
    REP #$21
    LDA !TileX
    ADC !XOffSet
    BPL .posXOffset
.negXOffset
    CMP #$FFF0
    BCS .continueXHigh
    SEP #$20
    CLC
RTL
.posXOffset
    CMP #$0100
    BCC .continueX
    SEP #$20
    CLC
RTL
.continueXHigh
    SEP #$20
    STA !TileX
    INC !TileSize
    SEC
RTL
.continueX
    SEP #$20
    STA !TileX
    SEC
RTL

IsValid:
    STZ !TileY+1
    LDA !TileY
    BPL .posY
.negY
    DEC !TileY+1
.posY
    REP #$21
    LDA !TileY
    ADC !YOffSet
    BPL .posYOffset
.negYOffset
    CMP #$FFF0
    BCS .continueY
    SEP #$20
    CLC
RTL
.posYOffset
    CMP #$00E0
    BCC .continueY
    SEP #$20
    CLC
RTL
.continueY
    SEP #$20
    STA !TileY

    STZ !TileX+1
    LDA !TileX
    BPL .posX
.negX
    DEC !TileX+1
.posX
    REP #$21
    LDA !TileX
    ADC !XOffSet
    BPL .posXOffset
.negXOffset
    CMP #$FFF0
    BCS .continueXHigh
    SEP #$20
    CLC
RTL
.posXOffset
    CMP #$0100
    BCC .continueX
    SEP #$20
    CLC
RTL
.continueXHigh
    SEP #$20
    STA !TileX
    INC !TileSize
    SEC
RTL
.continueX
    SEP #$20
    STA !TileX
    SEC
RTL

RemapOamTile:
    STA $0B
    CLC
    ADC !PoseOffset
    PHP
    PHA
    EOR $0B
    AND #$10
    BEQ .skipFix
    PLA
    CLC
    ADC #$10
    STA $8A
    BRA .prop
.skipFix
    PLA
    STA $8A
.prop
    PLP
    BCC .ZeroProp
    LDA !Property
    INC A
RTL  
.ZeroProp
    LDA !Property
RTL