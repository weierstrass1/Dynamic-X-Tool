;A = Palette Table ID, 16 bits
?DXClusterPaletteAssignment:
    STA $00
    SEP #$20

    STZ $05

    ;Check if palette was already assigned
    LDA !ClusterPalette,x
    CMP #$FF
    BEQ ?.StartAssignment

    INC $05

if !PaletteEffects
    LDA !ClusterPaletteAssignment,x
    AND #$20
    BNE ?+

    LDA !ClusterPaletteOption,x
    AND #$80
    BEQ ?+

    LDA !ClusterPalette,x
    LSR
    TAX

    LDA DX_Dynamic_Palettes_Updated+$08,x
    BNE ?+

    TXA
    ASL
    TAX
    
    LDA DX_Dynamic_Palettes_GlobalEffectID
    CMP DX_Dynamic_Palettes_LastGlobalEffectID+$10,x
    BNE ?.PaletteNotAssigned

    LDA DX_Dynamic_Palettes_GlobalEffectID+$01
    CMP DX_Dynamic_Palettes_LastGlobalEffectID+$11,x
    BNE ?.PaletteNotAssigned

    TXA
    LSR
    TAX
?+
endif
    ;Check if palette is still valid
    LDA DX_Dynamic_Palettes_DisableTimer+$08,x
    BEQ ?.PaletteNotAssigned

    ;If palette was already assigned and is valid then
    ;Update Disable Timer to avoid being overwrite by other sprite
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    LDX !SpriteIndex
    SEC
RTL
?.PaletteNotAssigned
    LDX !SpriteIndex 
?.StartAssignment

    ;Check if uses No Assigment
    LDA !ClusterPaletteAssignment,x
    AND #$20
    BEQ ?.Assignment

    ;If no assignment then set up palette using Palette Option
    LDA !ClusterPaletteOption,x
    LSR
    LSR
    LSR
    AND #$0E
    STA !ClusterPalette,x
    TAY

    LDA !new_sprite_num,x
    TYX
    ;Set up ID to avoid being ovewrite by other sprite, ID = FEXX
    ;XX = Custom sprite Number
    STA DX_Dynamic_Palettes_ID+$10,x
    LDA #$FE
    STA DX_Dynamic_Palettes_ID+$11,x

    TXA
    LSR
    TAX

    ;Updates Disable Timer to avoid being overwrite by other sprite
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x
    LDX !SpriteIndex

if !PaletteEffects
    ;Enable or disable palette effects depends on Palette Option
    LDA !ClusterPaletteOption,x
    AND #$80
    BEQ ?+

    LDA !ClusterPalette,x
    LSR
    TAX

    LDA DX_Dynamic_Palettes_GlobalSPEnable
    ORA.l ?.OrTable,x
    BRA ?++
?+
    LDA !ClusterPalette,x
    LSR
    TAX

    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND.l ?.AndTable,x
?++
    STA DX_Dynamic_Palettes_GlobalSPEnable

    LDX !SpriteIndex
endif
    SEC
RTL

?.Assignment
    LDA #$00
    XBA
    LDA !ClusterVersion,x
    REP #$20
    CLC
    ADC $00
    STA $00
    ASL
    CLC
    ADC $00
    TAX

    LDA !PaletteAddrTables,x
    STA $02
    LDA !PaletteAddrTables+1,x
    STA $03

    LDA $00
    ASL
    TAX
    LDA !PaletteIDTables,x
    STA $00
    SEP #$30

    LDX !SpriteIndex

    LDA !ClusterPaletteAssignment,x
    AND #$10
    BNE ?.ManualAssignment
?.AutoAssignment

    REP #$20
    LDA $00
    JSL !AssignPalette
    BCC ?.LoadPal

    TYA
    ASL
    STA !ClusterPalette,x
    LSR
    TAX
    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    LDX !SpriteIndex
    SEC
RTL
?.LoadPal
    CPY #$FF
    BNE ?+
    LDX !SpriteIndex
    CLC
RTL
?+
    TYA
    ASL
    STA !ClusterPalette,x
    BRA ?.StartPal

?.ManualAssignment
    LDA !ClusterPaletteOption,x
    LSR
    LSR
    LSR
    AND #$0E
    STA !ClusterPalette,x
    TAY

    LDA !ClusterPaletteOption,x
    TYX
    ;Set up ID to avoid being ovewrite by other sprite, ID = FEXX
    ;XX = Custom sprite Number
    STA DX_Dynamic_Palettes_ID+$10,x
    LDA #$FE
    STA DX_Dynamic_Palettes_ID+$11,x
    
    LDX !SpriteIndex
?.StartPal

    LDA !ClusterPalette,x
    TAX

    LDA DX_Dynamic_Palettes_GlobalEffectID
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$10,x
    LDA DX_Dynamic_Palettes_GlobalEffectID+$01
    STA DX_Dynamic_Palettes_LastGlobalEffectID+$11,x

    LDX !SpriteIndex
    LDA !ClusterPalette,x
    LSR
    TAX

    LDA #$02
    STA DX_Dynamic_Palettes_DisableTimer+$08,x

    LDA DX_Dynamic_Palettes_Updated+$08,x
    BEQ ?+

    LDX !SpriteIndex
    SEC
RTL
?+

    LDX !SpriteIndex
    LDA !ClusterPalette,x
    ASL
    ASL
    ASL
    CLC
    ADC #$81
    STA $00

if !PaletteEffects
    ;Use palette effects depends on Palette Option
    LDA !ClusterPaletteOption,x
    AND #$80
    BNE ?.DoEffects
endif
    LDA !ClusterPalette,x
    LSR
    TAX

    LDA DX_Dynamic_Palettes_GlobalSPEnable
    AND.l ?.AndTable,x
    STA DX_Dynamic_Palettes_GlobalSPEnable

    %TransferToCGRAM("$00", "$02", "$04", "#$001E")
    BCS ?+
    LDX !SpriteIndex
    CLC
RTL
?+
    
    LDX !SpriteIndex

    LDA !ClusterPalette,x
    LSR
    TAX

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x

    LDX !SpriteIndex
    SEC
RTL

if !PaletteEffects
?.DoEffects

    LDA #$00
    XBA
    LDA $00
    REP #$20
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteCopy
    STA $06
    SEP #$20
    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA $08

    LDA !ClusterPalette,x
    LSR
    TAX

    LDA DX_Dynamic_Palettes_GlobalSPEnable
    ORA.l ?.OrTable,x
    STA DX_Dynamic_Palettes_GlobalSPEnable

    LDX !SpriteIndex

    LDA $05
    PHA
    LDA #$0F
    STA $05
    LDA !ClusterBasePaletteLoaded,x
    AND #$04
    BNE +
    LDA #$04
    ORA !ClusterBasePaletteLoaded,x
    STA !ClusterBasePaletteLoaded,x

    LDX !SpriteIndex
    LDA !ClusterPalette,x
    LSR
    TAX
    LDA DX_PPU_CGRAM_SPPaletteCopyLoaded
    ORA.l ?.OrTable,x
    STA DX_PPU_CGRAM_SPPaletteCopyLoaded

    JSL !LoadPaletteOnBuffer
+
    LDA #$0F
    STA $02

    REP #$30
    LDA DX_Dynamic_Palettes_GlobalEffectID
    TAX
    SEP #$20
    LDA !PaletteEffectsTypes,x
    PHA

    TAX

    PHX
    PHX
    LDX !SpriteIndex
    LDA !ClusterPalette,x
    LSR
    TAY
    PLX
    LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded,x
    TYX
    ORA.l ?.OrTable,x
    PLX
    STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded,x

    PLA
    INC A
    PHA
    LDX !SpriteIndex
    AND !ClusterBasePaletteLoaded,x
    BEQ ?+
    
    CLC
    BRA ?++
?+
    SEC
?++ 
    PLA
    ORA !ClusterBasePaletteLoaded,x
    STA !ClusterBasePaletteLoaded,x 

    REP #$30
    SEP #$40
    LDA DX_Dynamic_Palettes_GlobalEffectID
    JSL !ApplyPaletteEffect
    BCS ?+

    PLA
    BNE ?++
    LDX !SpriteIndex
    CLC
RTL
?+
    PLA
?++
    LDX !SpriteIndex

    LDA !ClusterPalette,x
    LSR
    TAX

    LDA #$01
    STA DX_Dynamic_Palettes_Updated+$08,x

    LDX !SpriteIndex
    SEC
RTL
endif
?.OrTable
    db $80,$40,$20,$10,$08,$04,$02,$01
?.AndTable
    db $7F,$BF,$DF,$EF,$F7,$FB,$FD,$FE