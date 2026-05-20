?GetYDeltaAndSpeed:
    LDY #$00
	LDA !SpriteYHigh,x
	XBA
	LDA !SpriteYLow,x
	REP #$20
	SEC
	SBC $9A
	STA $D5
	BPL ?+
	EOR #$FFFF
	INC A
	INY
?+
	CMP #$0008
	BCC ?+
	LDA #$0007
?+
	SEP #$20
	ASL
	ASL
	ASL
	ASL
	CPY #$00
	BEQ ?+
	EOR #$FF
	INC A
?+
	STA $D7
RTL
