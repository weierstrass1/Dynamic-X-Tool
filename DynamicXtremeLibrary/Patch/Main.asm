DXBaseHijack1:
    JSR DynamicX

	PHK
	PEA.w .jslrtsreturn-1
	PEA.w $0084CE|!rom
	JML $0085D2|!rom
.jslrtsreturn

	PHK
	PEA.w .jslrtsreturn2-1
	PEA.w $0084CE|!rom
	JML $008449|!rom
.jslrtsreturn2
	JML $008243|!rom

DXBaseHijack2:
	JSR DynamicX
if !PlayerFeatures == 0    
	PHK
	PEA.w .jslrtsreturn2-1
	PEA.w $0084CE|!rom
	JML $00A300|!rom
.jslrtsreturn2
endif
    BIT.W $0D9B|!addr
    JML $0082DD|!rom
endif
