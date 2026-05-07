if !YoshiFeatures
org $01EEAA
	autoclean JML YoshiDMA
	db "DX"

org $02EA34
	autoclean JML IDKDMA

org $01E19D
	autoclean JML PodooboDMA
	NOP

elseif read2($01EEAA+$04) == $5844

org $01EEAA
	REP #$20                  ; Accum (16 bit) 
	LDA $00  
	ASL
	ASL    

org $02EA34
	REP #$20                  ; Accum (16 bit) 
	LDA $00  	

org $01E19D
	REP #$20                  ; Accum (16 bit) 
	LDA.W #$0008 
endif
