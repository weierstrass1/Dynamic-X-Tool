sa1rom

freecode cleaned

!XOffset = $4B
!YOffset = $4D
!TileX = $00
!TileY = $02

IsValid:
    STZ !TileY+1
    LDA !TileY
    BPL +
    LDA #$FF
    STA !TileY+1
+
    REP #$20
    LDA !YOffset
    CLC
    ADC !TileY
    CMP #$FFF0
    SEP #$20
    BCS .ValidY
    CMP #$E0
    BCS .InValid
.ValidY
    STA !TileY

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

print hex(IsValid)