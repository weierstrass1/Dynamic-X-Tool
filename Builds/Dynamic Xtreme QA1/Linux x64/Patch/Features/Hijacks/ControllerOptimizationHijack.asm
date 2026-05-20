if !ControllerOptimization
;Controller Optimization
org $0082F4
	BRA +
	db "X"
+

org $008243
	BRA +
	NOP
+

org $0086C6
RTL
elseif read1($0082F6) == $58
org $0082F4
	db $20,$50,$86,$A9,$09

org $008243
	db $20,$50,$86

org $0086C6
RTS
endif
