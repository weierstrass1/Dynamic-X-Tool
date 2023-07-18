sa1rom

freecode cleaned

!OAMBaseOffset = $51

RemapOamTile:
    STA $8A
    CLC
    ADC !OAMBaseOffset
    PHP
    PHA
    EOR $8A
    BIT #$10
    BEQ +
    PLA
    CLC
    ADC #$10
    STA $8A
    PLP
    BCS ++
    LDA #$00
RTL
+
    PLA
    STA $8A
    PLP
    BCS ++
    LDA #$00
RTL
++
    LDA #$01
RTL

print hex(RemapOamTile)