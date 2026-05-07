org $0098D1
	db GFX33>>16

org $00A340
	db GFX32>>16
	
org $00A39E
	db GFX33>>16

org read3($00A391)+$14
	db GFX33>>16

org $00A367
	autoclean JML fix_yoshi
	
org $00A387
	JML fix_yoshi2
	
org $00A3F0
	JSL fix_berries
	NOP #2
	
org $00B888
	RTS
	
org $00F649
	ADC #$8000
	
org $00F667
	ADC #$8000
	
org $00F67C
	ADC #$8000
	
org $00F691
	ADC #$8000
	
org $01E1A8
	ADC #$8800
	
org $01EEB4
	ADC #$8800
	
org $01EEC9
	ADC #$8800
	
org $02EA3E
	ADC #$8800

org $05BB8F
	jml load_animated_data