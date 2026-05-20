if !FixedColorOptimization
;Fixed Color Data NMI Optimization
org $00A4D1
	JSR $AE41

org $00AE41
	REP #$20			;2
	LDA $0701|!addr		;5
	ASL #3				;8
	SEP #$21			;10
	ROR #3				;13
	XBA					;14
	ORA #$40			;16
	STA $2132			;19
	LDA $0702|!addr		;22
	LSR A				;23
	SEC					;24
	ROR					;25
	STA $2132			;28
	XBA					;29
	STA $2132			;32
RTS						;33
	db "DX"
	NOP
elseif read2($00AE62) == $5844
org $00A4D1
	db $20,$47,$AE
org $00AE41
DATA_00AE41:
	db $00,$05,$0A				;3
DATA_00AE44:
	db $20,$40,$80				;6
CODE_00AE47:
	LDX.B #$02					;8
--            
	REP #$20                  	;10
	LDA.W $0701|!addr           ;13
	LDY.W DATA_00AE41,X       	;16
-
	DEY                       	;17
	BMI +           			;19
	LSR                       	;20
	BRA -          				;22

+
	SEP #$20                  	;24
	AND.B #$1F                	;26
	ORA.W DATA_00AE44,X       	;29
	STA.W $2132               	;32
	DEX                       	;33
	BPL --           			;35
RTS                       		;36
endif
