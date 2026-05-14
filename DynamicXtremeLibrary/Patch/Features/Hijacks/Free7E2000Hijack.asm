if !Free7E2000
org $0098D1
	db Data_GFX33>>16

org $00A340
	db Data_GFX32>>16
	
org $00A39E
	db Data_GFX33>>16

org read3($00A391)+$14
	db Data_GFX33>>16

org $00A367
	autoclean JML fix_yoshi
	
org $00A387
	JML fix_yoshi2
	
org $00A3F0
	JSL fix_berries
	db "DX"
	
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
elseif read2($00A3F0+$04) == $5844
org $0098D1
	db $7E

org $00A340
	db $7E
	
org $00A39E
	db $7E

org read3($00A391)+$14
	db $7E

%CleanOrg($00A367)
	db $E8,$EC,$84,$0D
	
org $00A387
	db $E8,$EC,$84,$0D
	
org $00A3F0
	db $AD,$76,$0D,$8D,$22,$43
	
org $00B888
	db $C2
	
org $00F649
	db $69,$00,$20
	
org $00F667
	db $69,$00,$20
	
org $00F67C
	db $69,$00,$20
	
org $00F691
	db $69,$00,$20
	
org $01E1A8
	db $69,$00,$85
	
org $01EEB4
	db $69,$00,$85
	
org $01EEC9
	db $69,$00,$85
	
org $02EA3E
	db $69,$00,$85

org $05BB8F
	db $0A,$05,$00,$A8
endif
