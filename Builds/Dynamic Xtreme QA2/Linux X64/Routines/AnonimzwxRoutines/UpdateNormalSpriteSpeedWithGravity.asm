?UpdateNormalSpriteSpeedWithGravity:
	LDA !SpriteBlockedStatus_ASB0UDLR,x
	AND #$24
	PHA
	BEQ ?+

	LDA !SpriteYSpeed,x
	STA $D5
	BMI ?+

	LDA #$20
	STA !SpriteYSpeed,x

?+
	JSL $01802A|!rom

	PLA
	BEQ ?+
	LDA $D5
	BMI ?+
	STZ !SpriteYSpeed,x
?+
RTL