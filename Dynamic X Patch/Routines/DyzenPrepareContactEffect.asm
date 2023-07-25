;$6C = Left
;$6A = Top
;$6E = Right
;$8D = Bottom

;$00 = Left
;$02 = Right
;$08 = Top
;$0C = Bottom
?DyzenPrepareContactEffect:
    REP #$20
	LDA $1487|!addr
	SEC
	SBC $6A				;A = HB 2 Bottom - Top = Height
	SEP #$20
	STA $07				

	REP #$20
	LDA $6E
	SEC
	SBC $6C				;A = HB 2 Right - Left = Width
	SEP #$20
	STA $06

	LDA $6C
	STA $04				;$04 = HB 2 X offset Low Byte
	LDA $6D				
	STA $0A				;$0A = HB 2 X offset High Byte

	LDA $6A
	STA $05				;$05 = HB 2 Y offset Low Byte
	LDA $6B
	STA $0B				;$0B = HB 2 Y offset High Byte

	REP #$20
	LDA $02
	PHA					;$01,s = HB 1 Right

	LDA $0C
	SEC
	SBC $08				;A = HB 1 Bottom - Top = Height
	SEP #$20
	STA $02

	REP #$20
	PLA
	SEC
	SBC $00				;A = HB 1 Right - Bottom
	SEP #$20
	STA $03

	LDA $08
	PHA 
	LDA $01
	STA $08				;$08 = HB 1 X offset High Byte
	PLA
	STA $01				;$01 = HB 1 Y offset Low Byte
RTL