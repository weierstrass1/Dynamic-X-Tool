?DyzenPrepareBounce:
    STZ $65		;Hitbox Direction
	LDA #$FF
	STA $66		;Top
	STA $67		
	STA $68		;Distance
	STA $69
	STA $6A		;Bounce Top
	STA $6B
RTL