UpdateNormalSpriteSpeedWithGravity:
	LDA !SpriteYSpeed,x
	BMI +

	LDA !SpriteBlockedStatus_ASB0UDLR,x
	AND #$24
	BEQ +

	LDA #$20
	STA !SpriteYSpeed,x

+
	JSL $01802A|!rom
RTL