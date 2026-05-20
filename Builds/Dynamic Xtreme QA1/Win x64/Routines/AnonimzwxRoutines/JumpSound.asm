?JumpSound:
	LDA $1697|!addr	    ; consecutive enemies stomped
	CLC		            ;
	ADC !1626,x	        ; plus number of enemies this sprite has killed (...huh?)
	INC $1697|!addr	    ; increment the counter
	TAY		            ; -> Y
	INY		            ; increment
	CPY #$08		    ; if the result is 8+...
	BCS ?+  	        ; don't play a sound effect
	TYX		            ;
	LDA $037FFF,x	    ; star sounds (X is never 0 here; they start at $038000)
	STA $1DF9|!addr	    ;
	LDX $15E9|!addr	    ;
?+
	TYA		;
	CMP #$08		    ; if the number is 8+...
	BCC ?+
	LDA #$08		    ; just use 8 when giving points
?+
	JSL $02ACE5|!rom	;
RTL