;$8B = Was riding
;A = Is Riding
;$98-$99 = Last Sprite X
;$9A-$9B = Last Sprite X
?Riding:
	XBA
	LDA !PlayerYSpeed
	BMI ?+
	XBA
	BNE ?.IsRiding
?+
	LDA $8B
	BNE ?.WasRiding
	LDA #$00
RTL
?.WasRiding
    %AnonimzwxRoutinesGetXDeltaAndSpeed()

    LDA !PlayerXSpeed
    CLC
    ADC $D7
    BVC ?+
    LDA #$7F
    STA $00
    LDY !PlayerXSpeed
    BPL ?+
    LDA #$80
?+
    STA !PlayerXSpeed

    %AnonimzwxRoutinesGetYDeltaAndSpeed()

    LDA $D7
    BMI ?+
	LDA #$00
RTL
?+
    EOR #$FF
	INC A
	LSR
	EOR #$FF
	INC A
    CLC
    ADC !PlayerYSpeed
    BVC ?+
    LDA #$80
?+
    STA !PlayerYSpeed
	LDA #$00
RTL
?.IsRiding
	%AnonimzwxRoutinesGetXDeltaAndSpeed()
	LDA $8B
	BNE ?..Start
	LDA $D7
	BEQ ?..Start
	;|player x speed| < |sprite x speed| || 
	;sign(player x speed) != sign(sprite x speed) ? 
	;player x speed = 0 : 
	;player x speed = sign(player x speed)*(|player x speed| - |sprite x speed|)
	LDA !PlayerXSpeed
	BEQ ?..Start
	EOR $D7
	AND #$80
	BNE ?..SetPlayerSpeedZero

	LDA $D7
	STA $01

	LDY #$00
	LDA !PlayerXSpeed
	STA $00
	BPL ?+
	EOR #$FF
	INC A
	STA $00

	LDA $D7
	EOR #$FF
	INC A
	STA $01

	INY
?+

	LDA $00
	SEC
	SBC $01
	BCC ?..SetPlayerSpeedZero

	CPY #$00
	BEQ ?+
	EOR #$FF
	INC A
?+
	STA !PlayerXSpeed
	BRA ?..Start
?..SetPlayerSpeedZero
	STZ !PlayerXSpeed
?..Start

	REP #$21
	LDA !PlayerX
	ADC $D5
	STA !PlayerX
	SEP #$20

	%AnonimzwxRoutinesGetYDeltaAndSpeed()

	REP #$21
	LDA !PlayerY
	ADC $D5
	STA !PlayerY
	SEP #$20
	LDA #$01
RTL
