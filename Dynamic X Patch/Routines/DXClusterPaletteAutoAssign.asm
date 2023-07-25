;$02-$04 = Palette Address, 24 bits
;A = Palette ID, 16 bits
?DXClusterPaletteAutoAssign:
    STA $00
    SEP #$20
    LDA !ClusterPalette,x
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
    LDX $15E9|!addr
    REP #$20
    LDA $00
    JSL !AssignPalette
    BCS ?.CheckIfPaletteWasFound
?.NewPalette
    TYA
    ASL
    STA !ClusterPalette,x
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

    %TransferToCGRAM("$00", "$02", "$04", "#$001E")

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
    STA !ClusterPalette,x

    LDX !SpriteIndex
    SEC
RTL