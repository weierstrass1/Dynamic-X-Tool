?DyzenBounce:
	LDA !HitmanNormalPlayerIsAbove,x
	BIT #$40
	BNE ?.ApplyBounce
	REP #$20
	LDA $6A
	SEC
	SBC !HitmanHB2Bottom
	CLC
	ADC !PlayerY
	STA !PlayerY
	SEP #$20

?.ApplyBounce
	JSL $01AA33|!rom    ;Do the player boost its Y Speed  

	LDA !HitmanNormalPlayerIsAbove,x
	AND #$40
	ORA #$08
	STA !HitmanNormalPlayerIsAbove,x
RTL
