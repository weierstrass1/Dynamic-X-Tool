?DyzenDetectPlayerIsAbove:
	LDA $65
	AND #$0C
	CMP #$0C
	BEQ ?+
	BIT #$08
	BEQ ?+
	
	AND #$04
	BEQ ?+

	LDA #$08
	STA !HitmanNormalPlayerIsAbove,x

RTL
?+
	LDA !HitmanNormalPlayerIsAbove,x
	BMI ?++
	BEQ ?+
	DEC A
	STA !HitmanNormalPlayerIsAbove,x
?+
RTL
?++
	LDA #$00
	STA !HitmanNormalPlayerIsAbove,x
RTL