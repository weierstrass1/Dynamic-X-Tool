; this routine will get Act Like value
; If position is invalid range, will return 0xFFFF?.
;
; input:
; $98-$99 block position Y
; $9A-$9B block position X
; $1933   layer
;
; output:
; A Act Like 16 bits

?GetActLike:
	%GetMap16()
    STA $1693|!addr
	CMP #$FF
	BNE ?+
	XBA
	CMP #$FF
	BNE ?++
	REP #$20
RTL
?++
	XBA
?+
    XBA
	PHK
	PEA.w ?.jslrtsreturn-1
	PEA.w $06856B|!rom
	JML $06F608|!rom
?.jslrtsreturn
	REP #$20
	LDA $03
RTL