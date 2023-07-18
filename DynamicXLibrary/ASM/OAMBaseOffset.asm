sa1rom

freecode cleaned

!Offset = $49
!Props = $0E


VRAMDisp:
    db $00,$02,$04,$06,$08,$0A,$0C,$0E
    db $20,$22,$24,$26,$28,$2A,$2C,$2E
    db $40,$42,$44,$46,$48,$4A,$4C,$4E
    db $60,$62,$64,$66,$68,$6A,$6C,$6E
    db $80,$82,$84,$86,$88,$8A,$8C,$8E
    db $A0,$A2,$A4,$A6,$A8,$AA,$AC,$AE
    db $C0,$C2,$C4,$C6,$C8,$CA,$CC,$CE
    db $E0,$E2,$E4,$E6,$E8,$EA,$EC,$EE

OAMBaseOffset:
    LDA #$01
    TRB !Props

    LDA #$00
    XBA
    LDA !Offset
    BIT #$40
    BEQ +
    INC !Props
+
    PHX
    AND #$3F
    TAX

    LDA VRAMDisp,x
    PLX
RTL

print hex(OAMBaseOffset)