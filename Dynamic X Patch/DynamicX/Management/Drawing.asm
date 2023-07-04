incsrc "../Data/PoseData.asm"

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


maxtilePointer:
    db $00,$10,$20,$30

Draw:
    STZ !MaxTilePriority+1
    REP #$30
    PHX
    LDA !PoseID
    ASL
    PHA
    CLC
    ADC !PoseID
    TAX

    LDA.l GraphicRoutine,x
    STA !GraphicRoutine
    LDA.l GraphicRoutine+1,x
    STA !GraphicRoutine+1

    LDX !MaxTilePriority
    LDA.l maxtilePointer,x
    AND #$00F0
    TAX
    LDA !maxtile_pointer_max,x
    STA !maxtile_pointer
    LDA !maxtile_pointer_max+2,x
    STA !maxtile_pointer+2
    LDA !maxtile_pointer_max+8,x
    STA !maxtile_pointer+4

    PLX
    LDA.l NumberOfTilesMinus1,x
    STA !Iterator
    CLC
    ADC.l TableOffset,x
    TAY
    SEP #$20
    
    JML [!GraphicRoutine|!dp]
.Return
    STZ $0B
    REP #$30

    LDX !MaxTilePriority
    LDA.l maxtilePointer,x
    AND #$00F0
    TAX
    LDA !maxtile_pointer
    STA !maxtile_pointer_max,x
    LDA !maxtile_pointer+2
    STA !maxtile_pointer_max+2,x

    PLX
    SEP #$30
RTL

;DynamicSamePropWithHFlip:
;    PHB
;    PHK
;    PLB
;
;.Loop
;	LDX !maxtile_pointer+0
;	CPX !maxtile_pointer+4
;	BEQ .End
;
;    LDA XDisplacements,y
;    STA !TileX
;    LDA YDisplacements,y
;    STA !TileY
;    LDA Sizes,y
;    STA !TileSize
;    JSL IsValid
;    BCC .next
;    STA $400000,x
;    LDA !TileY
;    STA $400001,x
;
;    LDA Tiles,y
;    JSL RemapOamTile
;    ORA !Property
;    STA $400003,x
;    LDA $8A
;    STA $400002,x
;
;    DEX #4
;    STX !maxtile_pointer+0
;
;    LDX !maxtile_pointer+2
;    LDA !TileSize
;    STA $400000,x
;    DEC !maxtile_pointer+2
;
;.next
;    DEY
;
;    DEC !Iterator
;    BPL .Loop
;.End
;    PLB
;    JML Draw_Return

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