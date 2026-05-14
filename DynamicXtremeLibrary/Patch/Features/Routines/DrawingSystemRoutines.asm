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
    STA $8A
    CLC
    ADC !PoseOffset
    PHA
    EOR $8A
    AND #$10
    BEQ +
    PLA
    CLC
    ADC #$10
    BRA ++
+
    PLA
++
    CMP !PoseOffset
    STA $8A
    LDA !Property
    BCS +
    ORA #$01
+
RTL


Draw:
    STZ !MaxTilePriority+1
    REP #$30
    PHX
    LDX !PoseID
    LDA.l Data_GraphicRoutineIDs,x
    AND #$00FF
    STA !GraphicRoutine
    ASL
    PHA
    CLC
    ADC !GraphicRoutine
    TAX

    LDA.l Data_GraphicRoutine,x
    STA !GraphicRoutine
    LDA.l Data_GraphicRoutine+1,x
    STA !GraphicRoutine+1

if !sa1
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

    LDX !PoseID
    LDA.l Data_NumberOfTilesMinus1,x
    AND #$00FF
    STA !Iterator
    CLC
    PLX
    ADC.l Data_TableOffset,x
    TAY
    SEP #$20
    
    JML [!GraphicRoutine|!dp]
else
    LDA !MaxTilePriority
    AND #$0002
    LSR
    TAX
    LDA.l DX_Drawing_InitialIndexHighPriority,x
    AND #$00FF
    STA !maxtile_pointer

    PHX
    LDX !PoseID
    LDA.l Data_NumberOfTilesMinus1,x
    AND #$00FF
    STA !Iterator

    LDA $03,s
    TAX
    
    LDA !Iterator
    CLC
    ADC.l Data_TableOffset,x
    TAY
    SEP #$20

    LDA #$FF
    STA !TileX
    LDX !maxtile_pointer
-
    LDA.l DX_Drawing_OAMMap,x
    BNE +

    PHX
    REP #$20
    TXA
    ASL
    ASL
    TAX
    SEP #$20
    LDA !OAMYPos,x
    PLX
    CMP #$F0
    BNE +

    INC !TileX

    LDA !TileX
    CMP !Iterator
    BCS ++
+
    INX
    BPL -

    LDA !TileX
    STA !Iterator
++
    STX !maxtile_pointer
    TXA

    PLX
    STA.l DX_Drawing_InitialIndexLowPriority,x

    LDA.l DX_Drawing_InitialIndexLowPriority
    CMP.l DX_Drawing_InitialIndexHighPriority
    BCS +
    STA.l DX_Drawing_InitialIndexHighPriority
+
    PLB

    JML [!GraphicRoutine|!dp]
endif

.Return
    STZ $0B
    REP #$30

if !sa1
    LDX !MaxTilePriority
    LDA.l maxtilePointer,x
    AND #$00F0
    TAX
    LDA !maxtile_pointer
    STA !maxtile_pointer_max,x
    LDA !maxtile_pointer+2
    STA !maxtile_pointer_max+2,x
else
    LDA DX_Drawing_InitialIndexLowPriority
    CMP DX_Drawing_InitialIndexHighPriority
    BCS +
    STA DX_Drawing_InitialIndexHighPriority
+
endif

    PLX
    SEP #$30
RTL

maxtilePointer:
    db $00,$10,$20,$30
