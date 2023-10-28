!Gravity = $7F9A7B+$08

if read1($00FFD5) == $23
    !Gravity = $418800+$08
endif
!MaxFallSpeed = !Gravity+$01

!Gravity = #$06
!MaxFallSpeed = #$40

;A = X Offset
CalculateXYSpeedForAimedJumpBasedOnJumpForce:
	PHA

	LDA !SpriteYSpeed,x
	STA $00
	LDA #$FF
	STA $01
	
	LDA #$00
	XBA
	LDA !MaxFallSpeed
	REP #$20
	SEC
	SBC $00
	STA $00
	SEP #$20

	%DivW(" !Scratch1", " !Scratch0", " !Gravity")

	REP #$20
	LDA !DivisionResult								;TMAXFallSpeed
	ASL
	STA $02
	SEP #$20

	LDA !SpriteYSpeed,x
	EOR #$FF
	INC A
	STA $00

	%MulW(" !Scratch0", " $02")

	REP #$20
	LDA !MultiplicationResult	
	STA $04											;TMaxFallSpeed*Vy0
	SEP #$20

	%MulW(" $02", " !Gravity")

	REP #$20
	LDA !MultiplicationResult
	LSR
	LSR
	STA $06											;TMaxFallSpeed*g
	SEP #$20

	%MulW(" $02", " $06")

	LDA !SpriteYHigh,x
	XBA
	LDA !SpriteYLow,x
	REP #$20
	SEC
	SBC !PlayerY
	ASL
	ASL
	ASL
	ASL
	SEC
	SBC $04
	CLC
	ADC !MultiplicationResult
	EOR #$FFFF
	INC A
	STA $00
	SEP #$20
	BMI .TargetPosUseLessThanMaxSpeed

	%DivW(" !Scratch1", " !Scratch0", " !MaxFallSpeed")

	LDA !DivisionResult	
	CLC
	ADC $02
	STA $02
	JMP .CalculateXSpeed
.TargetPosUseLessThanMaxSpeed

	LDA !SpriteYSpeed,x
	EOR #$FF
	INC A
	STA $00
	STZ $01

	%DivW(" !Scratch1", " !Scratch0", " !Gravity")
	
	LDA !DivisionResult	
	ASL
	STA $02
	STZ $03					;TMaxH

	%MulW(" $00", " $02")

	LDA !MultiplicationResult
	STA $04
	LDA !MultiplicationResult+$01
	STA $05

	%Mul(" $02", " !Gravity")
	%MulWAfterMul("$02")

	LDA !SpriteYHigh,x
	XBA
	LDA !SpriteYLow,x
	REP #$20
	ASL
	ASL
	ASL
	ASL
	PHA

	LDA !MultiplicationResult
	LSR
	LSR
	STA $06

	PLA
	SEC
	SBC $04
	CLC
	ADC $06
	LSR
	LSR
	LSR
	LSR
	SEC
	SBC !PlayerY
	EOR #$FFFF
	INC A	
	ASL
	STA $08
	SEP #$20
	BMI .CalculateXSpeed

	%DivW(" !Scratch9", " !Scratch8", " !Gravity")

	LDA !DivisionResult	
	ASL
	%SQRT()
	ASL
	ASL
	CLC
	ADC $02
	STA $02

.CalculateXSpeed

	STZ $0F
	PLA
	STA $0E
	BPL .negOffset
	LDA #$FF
	STA $0F
.negOffset
	LDA !SpriteXHigh,x
	XBA
	LDA !SpriteXLow,x
	REP #$20
	CLC
	ADC $0E
	SEC
	SBC !PlayerX
	BPL .posXDiff
	EOR #$FFFF
	INC A
.posXDiff
	ASL
	ASL
	ASL
	ASL
	STA $00
	SEP #$20

	%DivW(" !Scratch1", " !Scratch0", " !Scratch2")

	REP #$20
	LDA !DivisionResult								;X Speed
	BPL .posNewXSpeed
	LDA #$7F
.posNewXSpeed
	STA $04
	SEP #$20

	LDA !SpriteXHigh,x
	XBA
	LDA !SpriteXLow,x
	REP #$20
	CLC
	ADC $0E
	CMP !PlayerX
	SEP #$20
	BCC .playerOnRight
	LDA $04
	EOR #$FF
	INC A
	STA !SpriteXSpeed,x
RTL
.playerOnRight
	LDA $04
	STA !SpriteXSpeed,x
RTL