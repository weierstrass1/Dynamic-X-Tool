?CarryYOffset:
    STZ $01
    STA $00
    BPL ?+
    DEC $01
?+

    REP #$21
	LDA $D3
	ADC $00
	SEP #$20
	STA !SpriteYLow,x
	XBA
	STA !SpriteYHigh,x
RTL