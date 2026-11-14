?DyzenDetectPlayerIsAbove:
	LDA !HitmanNormalPlayerIsAbove,x
	BMI ?.NormalTimer
	BIT #$40
	BEQ ?.NormalTimer
	AND #$3F
	BEQ ?.ClearInterior
	DEC A
	BEQ ?.ClearInterior
	ORA #$40
	BRA ?.StoreInterior
?.ClearInterior
	LDA #$00
?.StoreInterior
	STA !HitmanNormalPlayerIsAbove,x
RTL
?.NormalTimer
	LDA $65
	AND #$0C
	CMP #$0C
	BEQ ?+
	BIT #$04
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
