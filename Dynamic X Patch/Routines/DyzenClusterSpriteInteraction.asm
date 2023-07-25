;Load Hitboxes of Normal Sprite
;$51 = X Offset
;$53 = Y Offset
;$8D = Hitbox Data Table (16 bits)
?DyzenClusterSpriteInteraction:
	LDA !ClusterXHigh,x
	STA $5A
	LDA !ClusterXLow,x
	STA $59

	LDA !ClusterYHigh,x
	STA $52
	LDA !ClusterYLow,x
	STA $51

	%DyzenProcessHitBoxes()
RTL