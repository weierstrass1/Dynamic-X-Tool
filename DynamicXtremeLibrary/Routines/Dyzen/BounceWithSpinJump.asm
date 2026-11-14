?DyzenBounceWithSpinJump:
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
	LDA !HitmanNormalPlayerIsAbove,x
	AND #$40
	ORA #$08
	STA !HitmanNormalPlayerIsAbove,x

	LDA !SpinJumpFlag
	BNE ?+
	JSL $01AA33|!rom    ;Do the player boost its Y Speed  
RTL
?+
	LDA #$F8
	STA !PlayerYSpeed
RTL
