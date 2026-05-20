?GetBlockInteractionInfo:
    %AnonimzwxRoutinesGetActLike()
	CMP #$FFFF
	BNE ?+
    SEP #$20
    LDA #$00
RTL
?+
    CMP #$0100
    BCS ?+
    SEP #$20
    LDA #$00
RTL
?+
    SEC
    SBC #$0100
    REP #$30
    TAY
    SEP #$20
    PHB
    PHK
    PLB
    LDA ?.InteractionType,y
    SEP #$10
    PLB
RTL

;Format:
;   SSSS UDLR
;   SSSS: Slope type
;       0000	No slope?.
;       0001	Gradual slope left?.
;       0010	Gradual slope right?.
;       0011	Normal slope left?.
;       0100	Normal slope right?.
;       0101	Steep slope left?.
;       0110	Steep slope right?.
;       0111	Left facing up conveyor?.
;       1000	Left facing down conveyor?.
;       1001	Right facing up conveyor?.
;       1010	Right facing down conveyor?.
;       1011	Very steep slope left?.
;       1100	Very steep slope right?.
;   U : Solid from above
;   D : Solid from below
;   L : Solid from left
;   R : Solid from right

?.InteractionType
    ;Page 1
    db $08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08,$08  ; Line 0
    db $08,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F  ; Line 1
    db $0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F  ; Line 2
    db $0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F  ; Line 3
    db $0F,$0F,$0F,$0F,$0F,$0A,$0A,$0A,$09,$09,$09,$02,$01,$06,$04,$05  ; Line 4
    db $02,$01,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F  ; Line 5
    db $0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$0F,$00,$00  ; Line 6
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line 7
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line 8
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line 9
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line A
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line B
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line C
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line D
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$0F,$0F,$0F,$00  ; Line E
    db $00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00,$00  ; Line F