if !PlayerFeatures

org $00F636
	autoclean JML PlayerDynamicRoutine
	RTS
	db "X"
elseif read1($00F63B) == $58
org $00F636
	REP #$20                  ; Accum (16 bit) 
	LDX.B #$00                
	LDA $09
endif
