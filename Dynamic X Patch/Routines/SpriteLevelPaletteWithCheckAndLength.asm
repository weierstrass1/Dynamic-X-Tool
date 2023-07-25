?SpriteLevelPaletteWithCheckAndLength:
    XBA
    LDA !LevelSpritePaletteType
    CMP !LastLevelSpritePaletteType
    BNE ?+
RTL
?+
    LDA $00

    LDA !NormalPalette,x
    LSR
    TAX
    LDA DX_Dynamic_Palettes_Updated,x
    BEQ ?+
    LDX !SpriteIndex
RTL
?+   
    LDA #$01
    STA DX_Dynamic_Palettes_Updated,x
    
    XBA
    %SpriteLevelPaletteWithLength()
RTL
