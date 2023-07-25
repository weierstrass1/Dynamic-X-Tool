?DyzenCheckBounce:	
	REP #$20
	LDA #$0001
	STA $4F

	LDA $47
	CMP $6A
	BCS ?+
	STA $6A

	LDA $45
	STA $6C
	LDA $49
	STA $6E
	LDA $4B
	STA $1487|!addr

?+
	SEP #$20

	LDA !NormalPlayerIsAbove,x
	CMP #$01
	BEQ ?+

	LDA !PlayerBlockedStatus_S00MUDLR
	AND #$04
	BNE ?+
	LDA !PlayerYSpeed
	BEQ ?+
	BMI ?+

	REP #$20
	LDA $0C
	SEC
	SBC #$0008
	CMP $47
	SEP #$20
	BCS ?+

	LDA #$02
	STA !NormalPlayerIsAbove,x

?+
RTL