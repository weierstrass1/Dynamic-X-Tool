;$02-$04 = Palette Address, 24 bits
;A = Palette ID, 16 bits
?DXNormalPaletteAutoAssign:
    STA $00
    SEP #$20
    LDA !NormalPalette,x
    CMP #$FF
    BEQ ?.PaletteNotAssigned

    LSR
    TAX
    LDA DX_Dynamic_Palettes_DisableTimer,x
    BEQ ?.PaletteNotAssigned

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer,x

    LDX !SpriteIndex
    SEC
RTL

?.PaletteNotAssigned
    LDX !SpriteIndex
    REP #$20
    LDA $00
    JSL !AssignPalette
    BCS ?.CheckIfPaletteWasFound
?.NewPalette
    TYA
    ASL
    STA !NormalPalette,x
    CLC
    ADC #$10
    ASL
    ASL
    ASL
    INC A
    STA $00

    TYX
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer,x

    LDA #$0F
    %InitSpriteLevelPaletteWithLength()

    LDX !SpriteIndex
    SEC
RTL
?.CheckIfPaletteWasFound
    CPY #$FF
    BNE ?.PaletteFound
?.PaletteNotFound
    LDX !SpriteIndex
    CLC
RTL
?.PaletteFound
    TYX
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer,x

    LDX $15E9|!addr
    TYA
    ASL
    STA !NormalPalette,x
    CLC
    ADC #$10
    ASL
    ASL
    ASL
    INC A
    STA $00

    %SpriteLevelPalette()

    LDX !SpriteIndex
    SEC
RTL