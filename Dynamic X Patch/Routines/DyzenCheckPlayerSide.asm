;Carry Set = Player at the left, Carry Clear = Player at the right
?DyzenCheckPlayerSide:
	LDA !SpriteXHigh,x
	XBA
	LDA !SpriteXLow,x
	REP #$20
	CLC
	ADC $00
	CMP !PlayerX
	SEP #$20
RTL