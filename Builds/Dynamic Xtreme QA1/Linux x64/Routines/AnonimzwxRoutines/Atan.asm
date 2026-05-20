;$00 = X
;$02 = Y
?Atan:
    LDA $00
    BNE ?+
    LDA #$B4
RTL
?+
    STZ $03
    REP #$20
    LDA $02
    ASL
    ASL
    ASL
    STA $02
    SEP #$20
    %DivW($03,$02,$00)
    REP #$30
    LDA !DivisionResult
    CMP #$0200
    BCC ?+
    LDA #$01FF
?+
    TAX
    SEP #$20
    LDA.l ?.AtanTable,x
    SEP #$10
    LDX !SpriteIndex
RTL

?.AtanTable
    db $00,$0E,$1C,$29,$35,$40,$4A,$52,$5A,$61,$67,$6C,$71,$75,$79,$7C
    db $7F,$82,$84,$86,$88,$8A,$8C,$8E,$8F,$91,$92,$93,$94,$95,$96,$97
    db $98,$99,$9A,$9A,$9B,$9C,$9C,$9D,$9D,$9E,$9E,$9F,$9F,$A0,$A0,$A1
    db $A1,$A1,$A2,$A2,$A3,$A3,$A3,$A3,$A4,$A4,$A4,$A5,$A5,$A5,$A5,$A6
    db $A6,$A6,$A6,$A6,$A7,$A7,$A7,$A7,$A7,$A7,$A8,$A8,$A8,$A8,$A8,$A8
    db $A9,$A9,$A9,$A9,$A9,$A9,$A9,$A9,$AA,$AA,$AA,$AA,$AA,$AA,$AA,$AA
    db $AA,$AB,$AB,$AB,$AB,$AB,$AB,$AB,$AB,$AB,$AB,$AB,$AC,$AC,$AC,$AC
    db $AC,$AC,$AC,$AC,$AC,$AC,$AC,$AC,$AC,$AC,$AC,$AD,$AD,$AD,$AD,$AD
    db $AD,$AD,$AD,$AD,$AD,$AD,$AD,$AD,$AD,$AD,$AD,$AD,$AD,$AE,$AE,$AE
    db $AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE,$AE
    db $AE,$AE,$AE,$AE,$AE,$AE,$AE,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF
    db $AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF
    db $AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$AF,$B0,$B0,$B0,$B0
    db $B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0
    db $B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0
    db $B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0,$B0
    db $B0,$B0,$B0,$B0,$B0,$B0,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1
    db $B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1
    db $B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1
    db $B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1
    db $B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1
    db $B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1
    db $B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B1,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2
    db $B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2,$B2