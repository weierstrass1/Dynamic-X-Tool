?SetSpeed:
	STA !SpriteXSpeed,x

	LDA !NormalGlobalFlip,x
	BEQ ?+
	LDA !SpriteXSpeed,x
	EOR #$FF
	INC A
	STA !SpriteXSpeed,x
?+
RTL