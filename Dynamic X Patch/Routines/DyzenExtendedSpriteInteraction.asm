;Load Hitboxes of Normal Sprite
;$51 = X Offset
;$53 = Y Offset
;$8D = Hitbox Data Table (16 bits)
DyzenClusterSpriteInteraction:
	LDA !ExtendedXHigh,x
	STA $5A
	LDA !ExtendedXLow,x
	STA $59

	LDA !ExtendedYHigh,x
	STA $52
	LDA !ExtendedYLow,x
	STA $51

	%DyzenProcessHitBoxes()
RTL