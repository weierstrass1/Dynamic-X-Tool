;A = Length
?InitSpriteLevelPaletteWithLength:
    XBA
    LDX !SpriteIndex
    LDA !NormalPalette,x
    LSR
    TAX
    LDA DX_Dynamic_Palettes_Updated,x
    BEQ ?+
    LDX !SpriteIndex
RTL
?+
    XBA
    PHA
    LDA #$01
    STA DX_Dynamic_Palettes_Updated,x
    LDA #$00
    XBA
    LDA $00   
    PHA 							
	REP #$30	
    STA $06							
    ASL
    CLC
    ADC $06
	CLC
	ADC.w #DX_PPU_CGRAM_BasePalette
	STA !Scratch5
	SEP #$30	

    LDA $02,s
    STA $08
    STZ $09

	LDA.b #DX_PPU_CGRAM_BasePalette>>16
	STA !Scratch7								
    LDA $02
    PHA
    LDA $03
    PHA
    LDA $04
    PHA 																	

	%SetRGBBase(!Scratch2,!Scratch4,!Scratch5,!Scratch7,$08)		

    PLA 
    STA $04
    PLA
    STA $03
    PLA
    STA $02
    PLA
    STA $00
    STZ $01

    PLA 
    %SpriteLevelPaletteWithLength()
RTL
