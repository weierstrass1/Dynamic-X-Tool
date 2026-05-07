Start:

    PHP
    PHX
    PHY

    JSR Init

    PLY
    PLX
    PLP

    LDA #$03                  ;\ Set OAM name base to #$03, clear the name and allow 8x8 and 16x16 sprites
    STA $2101                 ;/
    INC $10 
    JML $00806B

Init:
    PHB
    LDA.b #DX_FreeRams>>16
    PHA
    PLB
if !PalettesChange
    LDA #$FF
    STA DX_PPU_CGRAM_Transfer_Length
endif
if !DrawingSystem && !sa1 == 0
    LDA #$7F
    STA.w DX_Drawing_InitialIndexLowPriority
    LDA #$41
    STA.w DX_Drawing_InitialIndexHighPriority
endif
    REP #$20
if !DrawingSystem && !sa1 == 0
    LDA #$0101
    STA.w DX_Drawing_OAMMap+$03
    STA.w DX_Drawing_OAMMap+$05
    STA.w DX_Drawing_OAMMap+$07
    STA.w DX_Drawing_OAMMap+$09
    STA.w DX_Drawing_OAMMap+$0B
    STA.w DX_Drawing_OAMMap+$0D
    STA.w DX_Drawing_OAMMap+$0E
    STA.w DX_Drawing_OAMMap+$14
    STA.w DX_Drawing_OAMMap+$16
    STA.w DX_Drawing_OAMMap+$18
    STA.w DX_Drawing_OAMMap+$1A
    STA.w DX_Drawing_OAMMap+$1C
    STA.w DX_Drawing_OAMMap+$1E
    STA.w DX_Drawing_OAMMap+$38
    STA.w DX_Drawing_OAMMap+$3E
    STA.w DX_Drawing_OAMMap+$40
    STA.w DX_Drawing_OAMMap+$42
    STA.w DX_Drawing_OAMMap+$44
    STA.w DX_Drawing_OAMMap+$46
    STA.w DX_Drawing_OAMMap+$48
    STA.w DX_Drawing_OAMMap+$4A
endif
    LDA #$0000
if !DrawingSystem && !sa1 == 0
    STA.w DX_Drawing_OAMMap+$00
    STA.w DX_Drawing_OAMMap+$01
    STA.w DX_Drawing_OAMMap+$10
    STA.w DX_Drawing_OAMMap+$12
    STA.w DX_Drawing_OAMMap+$20
    STA.w DX_Drawing_OAMMap+$22
    STA.w DX_Drawing_OAMMap+$24
    STA.w DX_Drawing_OAMMap+$26
    STA.w DX_Drawing_OAMMap+$28
    STA.w DX_Drawing_OAMMap+$2A
    STA.w DX_Drawing_OAMMap+$2C
    STA.w DX_Drawing_OAMMap+$2E
    STA.w DX_Drawing_OAMMap+$30
    STA.w DX_Drawing_OAMMap+$32
    STA.w DX_Drawing_OAMMap+$34
    STA.w DX_Drawing_OAMMap+$36
    STA.w DX_Drawing_OAMMap+$39
    STA.w DX_Drawing_OAMMap+$3B
    STA.w DX_Drawing_OAMMap+$3C
!i = $4C
while !i < $7F
    STA.w DX_Drawing_OAMMap+!i
    !i #= !i+2
endif
    STA.w DX_Drawing_OAMMap+$7E
endif
if !PalettesChange
    STA.w DX_Dynamic_Palettes_Updated+$00
    STA.w DX_Dynamic_Palettes_Updated+$02
    STA.w DX_Dynamic_Palettes_Updated+$04
    STA.w DX_Dynamic_Palettes_Updated+$06
    STA.w DX_Dynamic_Palettes_Updated+$08
    STA.w DX_Dynamic_Palettes_Updated+$0A
    STA.w DX_Dynamic_Palettes_Updated+$0C
    STA.w DX_Dynamic_Palettes_Updated+$0E
    STA.w DX_Dynamic_Palettes_DisableTimer+$00
    STA.w DX_Dynamic_Palettes_DisableTimer+$02
    STA.w DX_Dynamic_Palettes_DisableTimer+$04
    STA.w DX_Dynamic_Palettes_DisableTimer+$06
    STA.w DX_Dynamic_Palettes_DisableTimer+$08
    STA.w DX_Dynamic_Palettes_DisableTimer+$0A
    STA.w DX_Dynamic_Palettes_DisableTimer+$0C
    STA.w DX_Dynamic_Palettes_DisableTimer+$0E
endif
if !PaletteEffects
    STA.w DX_Dynamic_Palettes_GlobalEffectID
    STA.w DX_Dynamic_Palettes_GlobalBGEnable
	STA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
	STA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
    STA DX_PPU_CGRAM_BGEffectLoaded
	STA DX_PPU_FixedColor_RGBBaseLoaded
    STA DX_PPU_FixedColor_CopyLoaded
endif
if !DynamicPoses
    STA.w DX_Dynamic_Pose_Length
endif
    STA.w DX_Dynamic_CurrentDataSend
if !DynamicPoses
    !i = 0
    while !i < 128
    STA.w DX_Dynamic_Pose_HashSize+!i
    !i #= !i+2
    endif
endif
    LDA #$0800
    STA.w DX_Dynamic_MaxDataPerFrame 
    LDA #$FFFF
if !PalettesChange
    STA.w DX_Dynamic_Palettes_ID+$00
    STA.w DX_Dynamic_Palettes_ID+$02
    STA.w DX_Dynamic_Palettes_ID+$04
    STA.w DX_Dynamic_Palettes_ID+$06
    STA.w DX_Dynamic_Palettes_ID+$08
    STA.w DX_Dynamic_Palettes_ID+$0A
    STA.w DX_Dynamic_Palettes_ID+$0C
    STA.w DX_Dynamic_Palettes_ID+$0E
    STA.w DX_Dynamic_Palettes_ID+$12
    STA.w DX_Dynamic_Palettes_ID+$14
    STA.w DX_Dynamic_Palettes_ID+$16
    STA.w DX_Dynamic_Palettes_ID+$18
    STA.w DX_Dynamic_Palettes_ID+$1A
    STA.w DX_Dynamic_Palettes_ID+$1C
    STA.w DX_Dynamic_Palettes_ID+$1E
if !PlayerFeatures
    STA DX_PPU_CGRAM_LastPlayerPal
endif
    LDA #$FFFE
    STA.w DX_Dynamic_Palettes_ID+$10
    LDA #$FFFF
endif
if !PaletteEffects
    STA.w DX_Dynamic_Palettes_LastGlobalEffectID
    STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$00
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$02
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$04
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$06
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$08
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$0A
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$0C
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$0E
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$12
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$14
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$16
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$18
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$1A
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$1C
	STA.w DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$1E
	STA DX_PPU_FixedColor_LastGlobalEffectID
endif
if !PlayerFeatures
    STA $0D85|!normalBnk
    STA $0D87|!normalBnk
    STA $0D89|!normalBnk
    STA $0D8B|!normalBnk
    STA $0D8D|!normalBnk
    STA $0D8F|!normalBnk
    STA $0D91|!normalBnk
    STA $0D93|!normalBnk
    STA $0D95|!normalBnk
    STA $0D97|!normalBnk
    STA $0D99|!normalBnk
endif
if !DynamicPoses
    !i = 0
    while !i < 256
    STA.w DX_Dynamic_Pose_ID+!i
    !i #= !i+2
    endif
endif
if !PlayerFeatures
    LDA $0D82|!normalBnk
    STA.w DX_Dynamic_Player_Palette_Addr
    LDA #$2000
    STA.w DX_Dynamic_Player_GFX_Addr
    LDA #$007E
    STA.w DX_Dynamic_Player_GFX_BNK
endif
    SEP #$20

    LDA #$00
if !GraphicsChange
    STA DX_PPU_VRAM_Transfer_Length
endif
if !PalettesEffects
    STA DX_PPU_FixedColor_Enable
endif

if !DynamicPoses
    LDA #$5F|$80
    STA.w DX_Dynamic_Tile_Size+$00
    STA.w DX_Dynamic_Tile_Size+$5F
    LDA #$00|$80
    STA.w DX_Dynamic_Tile_Offset+$00
    STA.w DX_Dynamic_Tile_Offset+$5F
    LDA #$1F|$80
    STA.w DX_Dynamic_Tile_Size+$60
    STA.w DX_Dynamic_Tile_Size+$7F
    LDA #$60
    STA.w DX_Dynamic_Tile_Offset+$60
    STA.w DX_Dynamic_Tile_Offset+$7F
endif
if !PlayerFeatures
    LDA #$01
    STA.w DX_Dynamic_Player_GFX_Enable
    STA.w DX_Dynamic_Player_Palette_Enable
    LDA #$00
    STA.w DX_Dynamic_Player_CustomPlayer
    STA.w DX_Dynamic_Player_Palette_BNK
    LDA #$FF
    STA.w DX_Dynamic_Player_LastCustomPlayer
endif
if !YoshiFeatures
    LDA #$01
    STA.w DX_Dynamic_Yoshi_VRAMEnable
    LDA #$00
    STA.w DX_Dynamic_Yoshi_Addr
    LDA #$85
    STA.w DX_Dynamic_Yoshi_Addr+1
    LDA #$7E
    STA.w DX_Dynamic_Yoshi_BNK
endif
    PLB
RTS
