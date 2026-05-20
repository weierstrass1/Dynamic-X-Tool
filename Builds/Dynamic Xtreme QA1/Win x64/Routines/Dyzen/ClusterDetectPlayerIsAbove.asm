?DyzenClusterDetectPlayerIsAbove:
	LDA $65
	AND #$0C
	CMP #$0C
	BEQ ?+
	BIT #$08
	BEQ ?+
	
	AND #$04
	BEQ ?+

	LDA #$08
	STA !HitmanClusterPlayerIsAbove,x

RTL
?+
	LDA !HitmanClusterPlayerIsAbove,x
	BMI ?++
	BEQ ?+
	DEC A
	STA !HitmanClusterPlayerIsAbove,x
?+
RTL
?++
	LDA #$00
	STA !HitmanClusterPlayerIsAbove,x
RTL