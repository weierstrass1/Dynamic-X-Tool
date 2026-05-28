?DyzenBounce:
	REP #$20
	LDA $6A
	SEC
	SBC $0C
	CLC
	ADC !PlayerY
	STA !PlayerY
	SEP #$20

	JSL $01AA33|!rom    ;Do the player boost its Y Speed  

	LDA #$08
	STA !HitmanExtendedPlayerIsAbove,x
RTL