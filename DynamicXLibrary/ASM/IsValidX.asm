sa1rom

freecode cleaned

!XOffset = $4B
!TileX = $00

IsValidX:
    STZ !TileX+1
    LDA !TileX
    BPL +
    LDA #$FF
    STA !TileX+1
+
    REP #$20
    LDA !XOffset
    CLC
    ADC !TileX
    CMP #$FFF0
    SEP #$20
    BCS .ValidX
    XBA
    BNE .InValid
    XBA
    STA !TileX
    SEC
RTL
.ValidX
    XBA
    STA !TileX
    INC !TileSize
    SEC
RTL
.InValid
    CLC
RTL

print hex(IsValidX)