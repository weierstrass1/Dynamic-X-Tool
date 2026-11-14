?DyzenPrepareBounce:
    STZ $65		;Hitbox Direction
	LDA #$FF
	STA $66		;Top
	STA $67		
	STA $68		;Distance
	STA $69
	STA $6A		;Bounce Top
	STA $6B
	; Recordar si ya estaba protegido antes de revisar las hitboxes.
	LDA !HitmanNormalPlayerIsAbove,x
	BMI ?.Prepared
	BIT #$40
	BEQ ?.Prepared
	AND #$3F
	BEQ ?.Prepared
	LDA #$80
	TSB $65
?.Prepared
RTL
