?DyzenDetectPlayerIsAbove:
	LDA $65
	AND #$0C
	CMP #$0C
	BNE ?+

	LDA !NormalPlayerIsAbove,x
	BEQ ?++

	LDA #$01
	STA !NormalPlayerIsAbove,x

?++
RTL
?+
	
	AND #$04
	BEQ ?+

	LDA #$01
	STA !NormalPlayerIsAbove,x

?+
RTL