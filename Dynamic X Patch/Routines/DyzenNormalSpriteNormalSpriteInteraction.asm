?DyzenNormalSpriteNormalSpriteInteraction:
    PHY
    %DyzenNormalSpriteClippingRoutine()				; MarioClipping
	PLY

    LDX !SpriteIndex
    PHY
    %DyzenNormalSpriteInteraction()
    BCS ?+
    PLY
    CLC
RTL
?+
    PLY
    SEC
RTL