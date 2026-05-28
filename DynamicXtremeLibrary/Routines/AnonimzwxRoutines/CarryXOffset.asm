?CarryXOffset:
    XBA
    %AnonimzwxRoutinesPlayerIsFrontPose()
	BEQ ?+
	LDA $D1
	STA !SpriteXLow,x
	LDA $D2
	STA !SpriteXHigh,x
RTL
?+
	STZ $01
    XBA
	STA $00

	LDA !PlayerDirection
	AND #$01
	REP #$21
	BNE ?+
	LDA $00
	EOR #$FFFF
	INC A
	BRA ?++
?+
	LDA $00
?++
	CLC
	ADC $D1
	SEP #$20
	STA !SpriteXLow,x
	XBA
	STA !SpriteXHigh,x
RTL