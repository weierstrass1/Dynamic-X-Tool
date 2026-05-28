?CapeStomping:
	LDA $19
	CMP #$02
	BEQ ?+
    CLC
RTL
?+
	LDA $1887|!addr
	CMP #$30
	BCS ?+
    CLC
RTL
?+
	LDA !SpriteBlockedStatus_ASB0UDLR,x
	AND #$24
	BNE ?+
    CLC
RTL
?+
	LDA !PlayerBlockedStatus_S00MUDLR
	AND #$04
	BNE ?+
    CLC
RTL
?+
    SEC
RTL