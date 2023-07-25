?DyzenCheckPlayerIsAbove:
	LDA $4F
	BEQ ?+
RTL
?+
	LDA $4D
	ASL
	TAX

	REP #$20
	JSR (?.CalculateDist,x)
	BCS ?+
	SEP #$20
RTL
?+
	PHA
	SEP #$20

	LDA $65
	BEQ ?.checkBox
	CMP #$07
	BNE ?.BoxWithPriority

?.BoxWithoutPriority

	LDA $4D
	CMP #$07
	BEQ ?.changeBox

	BRA ?.checkBox

?.BoxWithPriority
	LDA $4D
	CMP #$07
	BEQ ?.checkBox
?.Return
	PLA
	PLA
RTL	
?.checkBox
	REP #$20
	LDA $68
	CMP $01,s
	SEP #$20
	BCS ?.changeBox
	BRA ?.Return

?.changeBox

	LDA $4D
	STA $65

	REP #$20
	LDA $47
	STA $66
	PLA
	STA $68
	SEP #$20
RTL

?.CalculateDist
	dw ?.Not
	dw ?.Not
	dw ?.Not
	dw ?.Not

	dw ?.Not
	dw ?.DownRight
	dw ?.DownLeft
	dw ?.Down

	dw ?.Not
	dw ?.UpRight
	dw ?.UpLeft
	dw ?.Up

	dw ?.Not
	dw ?.Right
	dw ?.Left
	dw ?.Not

?.Not
	CLC
RTS

?.DownRight
	LDA $47		;HB2?.Top - HB1?.Bottom
	SEC
	SBC $0C
	PHA

	LDA $45		;HB2?.Left - HB1?.Right
	SEC
	SBC $02
	CMP $01,s
	BCS ?+

	STA $01,s

?+
	PLA
	SEC
RTS

?.DownLeft
	LDA $47		;HB2?.Top - HB1?.Bottom
	SEC
	SBC $0C
	PHA

	LDA $00		;HB1?.Left - HB2?.Right
	SEC
	SBC $49
	CMP $01,s
	BCS ?+

	STA $01,s

?+
	PLA
	SEC
RTS

?.Down
	LDA $47		;HB2?.Top - HB1?.Bottom
	SEC
	SBC $0C
	SEC
RTS

?.UpRight
	LDA $08		;HB1?.Top - HB2?.Bottom
	SEC
	SBC $4B
	PHA

	LDA $45		;HB2?.Left - HB1?.Right
	SEC
	SBC $02
	CMP $01,s
	BCS ?+

	STA $01,s

?+
	PLA
	SEC
RTS

?.UpLeft
	LDA $08		;HB1?.Top - HB2?.Bottom
	SEC
	SBC $4B
	PHA

	LDA $00		;HB1?.Left - HB2?.Right
	SEC
	SBC $49
	CMP $01,s
	BCS ?+

	STA $01,s

?+
	PLA
	SEC
RTS

?.Up
	LDA $08		;HB1?.Top - HB2?.Bottom
	SEC
	SBC $4B
	SEC
RTS

?.Right
	LDA $45		;HB2?.Left - HB1?.Right
	SEC
	SBC $02
	SEC
RTS

?.Left
	LDA $00		;HB1?.Left - HB2?.Right
	SEC
	SBC $49
	SEC
RTS