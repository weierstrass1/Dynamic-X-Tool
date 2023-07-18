sa1rom

freecode cleaned

!YOffset = $4D
!TileY = $02

IsValidY:
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
    BCC .ValidY
.InValid
    CLC
RTL
.ValidY
    STA !TileY
    SEC
RTL

print hex(IsValidY)