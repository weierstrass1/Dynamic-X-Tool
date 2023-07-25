;Load Hitboxes of Normal Sprite
;$51 = X Offset
;$53 = Y Offset
;$8D = Hitbox Data Table (16 bits)
?DyzenNormalSpriteInteraction:
	LDA !SpriteXHigh,x
	STA $5A
	LDA !SpriteXLow,x
	STA $59

	LDA !SpriteYHigh,x
	STA $52
	LDA !SpriteYLow,x
	STA $51

	%DyzenProcessHitBoxes()
RTL