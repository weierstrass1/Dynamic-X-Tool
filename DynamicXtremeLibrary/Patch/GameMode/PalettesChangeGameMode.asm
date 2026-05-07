.PaletteSetup
if !sa1 == 1
	LDA.b #..Start
	STA $3180
	LDA.b #..Start>>8
	STA $3181
	LDA.b #..Start>>16
	STA $3182
	JSR $1E80
JML $008075|!rom

..Start
endif

	JSL AllowedGameMode
	ORA $13D4|!addr
    BEQ +
if !sa1 == 1
RTL
else
JML $008075|!rom
endif
+
    REP #$20
    LDA #$0000
    STA DX_Dynamic_Palettes_Updated+$00
    STA DX_Dynamic_Palettes_Updated+$02
    STA DX_Dynamic_Palettes_Updated+$04
    STA DX_Dynamic_Palettes_Updated+$06
    STA DX_Dynamic_Palettes_Updated+$08
    STA DX_Dynamic_Palettes_Updated+$0A
    STA DX_Dynamic_Palettes_Updated+$0C
    STA DX_Dynamic_Palettes_Updated+$0E
    SEP #$20
    !i = $00
while !i < $10
    LDA DX_Dynamic_Palettes_DisableTimer+!i
    BEQ +
    DEC A
    STA DX_Dynamic_Palettes_DisableTimer+!i
+
    !i #= !i+$01
endwhile
