?GetXDeltaAndSpeed:
    LDY #$00
	LDA !SpriteXHigh,x
	XBA
	LDA !SpriteXLow,x
	REP #$20
	SEC
	SBC $98
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
