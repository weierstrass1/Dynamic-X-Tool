;Output: Carry set if stomp, clear if not stomp
?YoshiStomping:
	LDA !RidingYoshi
	BNE ?+
    CLC
RTL
?+
	LDA !SpriteBlockedStatus_ASB0UDLR,x
	AND #$24
	BNE ?+
    CLC
RTL
?+
	LDA !PlayerBlockedStatus_S00MUDLR
	AND #$04
	BNE ?+
    CLC
RTL
?+
	LDA $16CD|!addr
	ORA $16CD|!addr+1
	ORA $16CD|!addr+2
	ORA $16CD|!addr+3
	AND #$02
	BNE ?+
    CLC
RTL
?+
	LDA !SpriteXHigh,x
	XBA
	LDA !SpriteXLow,x
	REP #$20
	SEC
	SBC !PlayerX
	BPL ?+
	EOR #$FFFF
	INC A
?+
	CMP #$0030
	SEP #$20
	BCC ?+
    CLC
RTL
?+
	LDA !SpriteYHigh,x
	XBA
	LDA !SpriteYLow,x
	REP #$20
	SEC
	SBC !PlayerY
	BPL ?+
	EOR #$FFFF
	INC A
?+
	CMP #$0028
	SEP #$20
	BCC ?+
    CLC
RTL
?+
    SEC
RTL