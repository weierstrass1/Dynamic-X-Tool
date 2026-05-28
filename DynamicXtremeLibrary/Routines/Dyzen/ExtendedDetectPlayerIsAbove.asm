?ExtendedDetectPlayerIsAbove:
	LDA $65
	AND #$0C
	CMP #$0C
	BEQ ?+
	BIT #$08
	BEQ ?+
	
	AND #$04
	BEQ ?+

	LDA #$08
	STA !HitmanExtendedPlayerIsAbove,x
RTL
?+
	LDA !HitmanExtendedPlayerIsAbove,x
	BMI ?++
	BEQ ?+
	DEC A
	STA !HitmanExtendedPlayerIsAbove,x
?+
RTL
?++
	LDA #$00
	STA !HitmanExtendedPlayerIsAbove,x
RTL
