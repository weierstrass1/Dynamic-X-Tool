ReleaseItem:
    LDA !PlayerNoReleaseFlag
    BNE .Keep

    LDA !PlayerState
    CMP #$37
    BEQ .Keep
    CMP #$21
    BEQ .Keep

    LDA !PlayerForceReleaseFlag
    BNE .Drop

    LDA !ButtonPressed_AXLR0000
    ORA !ButtonPressed_BYETUDLR
    AND #$40
    BEQ .Release
.Keep
    LDA #$18
    STA !SpriteDecTimer2,x
RTL

.Release
    LDA #$08
    STA !SpriteDecTimer2,x

	LDA !ButtonPressed_BYETUDLR
	BIT #$08
	BNE .Kick
	AND #$04
	BEQ .Kick
.Drop
    LDA #$37
    STA !PlayerState
RTL
.Kick
    LDA #$21
    STA !PlayerState
RTL
