?DyzenBounce:
	REP #$20
	LDA $6A
	SEC
	SBC !HitmanHB2Bottom
	CLC
	ADC !PlayerY
	STA !PlayerY
	SEP #$20

	JSL $01AA33|!rom    ;Do the player boost its Y Speed  

	LDA #$08
	STA !HitmanNormalPlayerIsAbove,x
RTL