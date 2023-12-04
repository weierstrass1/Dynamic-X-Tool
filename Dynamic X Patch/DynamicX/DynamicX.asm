if read1($00FFD5) == $23
    fullsa1rom
else
    lorom
endif

    !sa1 = 0

if read1($00FFD5) == $23
    !sa1 = 1
endif

incsrc "./ExtraDefines/DynamicXDefines.asm"

incsrc "Hijacks/BaseHijacks.asm"

if !Desinstallation == 0
reset freespaceuse
freecode

Routines:
if read3($00821F) == $1B80A3
    dl $000000
    dl $000000
    dl $000000
else
if !DynamicPoses
    dl !PoseWasLoaded
    dl !TakeDynamicRequest
else
    dl $000000
    dl $000000
endif
if !DrawingSystem
    dl !Draw
else
    dl $000000
endif
endif
if !DrawingSystem
    dl IsValid|!rom
    dl RemapOamTile|!rom
    dl XIsValid|!rom
    dl YIsValid|!rom
    dl Draw_Return|!rom
else
    dl $000000
    dl $000000
    dl $000000
    dl $000000
    dl $000000
endif
if read3($00821F) == $1B80A3
    dl $000000
    dl $000000
else
if !DynamicPoses
    dl !ResourceTable
else
    dl $000000
endif
if !DrawingSystem
    dl !GraphicRoutinesTable
else
    dl $000000
endif
endif
if !PaletteChange
    dl AssignPalette|!rom
if !PaletteEffects
    dl SetHSLBase|!rom
    dl SetRGBBase|!rom
    dl MixHSL|!rom
    dl MixRGB|!rom
else
    dl $000000
    dl $000000
    dl $000000
    dl $000000
endif
else
    dl $000000
    dl $000000
    dl $000000
    dl $000000
    dl $000000
endif
if read3($00821F) == $1B80A3
    dl $000000
elseif !DynamicPoses
    dl !SetPropertyAndOffset
else 
    dl $000000
endif
if read3($00821F) == $1B80A3
    dl $000000
    dl $000000
elseif !PaletteEffects
    dl !PaletteEffectsTable
    dl !PaletteEffectsPatch
else 
    dl $000000
    dl $000000
endif
    dl $000000
    dl $000000
if !PaletteChange || !ControllerOptimization
    dl DXGameModeHijack_Start
    dl DXGameModeHijack_End
else
    dl $000000
    dl $000000
endif
if read3($00821F) == $1B80A3
    dl $000000
    dl $000000
elseif !DynamicPoses
    dl !PaletteTables
    dl !DynamicPoseSpaceConfig
else 
    dl $000000
    dl $000000
endif
if !GraphicChange || !PaletteChange
GameModeTable:
    db $00,$00,$01,$01,$01,$01,$00,$01
    ;  g00,g01,g02,g03,g04,g05,g06,g07
    db $01,$01,$01,$00,$02,$02,$02,$00
    ;  g08,g09,g0A,g0B,g0C,g0D,g0E,g0F
    db $00,$01,$01,$01,$01,$01,$00,$00
    ;  g10,g11,g12,g13,g14,g15,g16,g17
    db $00,$00,$00,$01,$01,$01,$01,$01
    ;  g18,g19,g1A,g1B,g1C,g1D,g1E,g1F
    db $00,$00,$00,$00,$01,$01,$00,$00
    ;  g20,g21,g22,g23,g24,g25,g26,g27
    db $00,$00,$00,$00,$00,$00,$00,$00
    ;  g28,g29,g2A,g2B,g2C,g2D,g2E,g2F

DynamicX:

    LDX $0100|!addr

    ;Find on the table if the current level mode activate Dynamic X
    LDA.l GameModeTable,x
    BNE +
    ;If Dynamic X is inactive, reset its importants variables
    JSR Init
RTS
+
    STZ $4200
    LDA #$80
    STA $2100

    PHD

	REP #$30
	LDY #$0004            ;Used to activate DMA Transfer

    LDA #$0000
    STA DX_Dynamic_CurrentDataSend

    LDA DX_Timer
    INC A
    STA DX_Timer

	LDA #$4300
	TCD                 ;direct page = 4300 for speed

    SEP #$30

if !GraphicChange
    JSR VRAMDMA
endif
if !PaletteChange
    JSR CGRAMDMA
endif
if !PaletteEffects
    JSR CGRAMToBufferDMA
endif

    PLD

    LDA #$81
    STA $4200
RTS

Start:

    PHP
    PHX
    PHY

    LDA #$00
    STA DX_Timer
    STA DX_Timer+1
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
if !PaletteChange
    LDA #$FF
    STA DX_PPU_CGRAM_Transfer_Length
if !PaletteEffects
    STA DX_PPU_CGRAM_BufferTransfer_Length
endif
endif
    REP #$20
    LDA #$0000
if !PaletteChange
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
    STA DX_PPU_CGRAM_BGPaletteCopyLoaded
	STA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
	STA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
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
if !PaletteChange
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
    STA.w DX_Dynamic_Palettes_ID+$12
    LDA #$FFFF
endif
if !PaletteEffects
    STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$00
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$02
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$04
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$06
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$08
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$0A
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$0C
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$0E
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$10
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$12
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$14
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$16
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$18
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$1A
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$1C
	STA.w DX_Dynamic_Palettes_LastGlobalEffectID+$1E
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
if !GraphicChange
    STA DX_PPU_VRAM_Transfer_Length
endif
if !PaletteEffects
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
    PLB
    RTS

DXBaseHijack1:
    JSR DynamicX

	PHK
	PEA.w .jslrtsreturn-1
	PEA.w $0084CE|!rom
	JML $0085D2|!rom
.jslrtsreturn

	PHK
	PEA.w .jslrtsreturn2-1
	PEA.w $0084CE|!rom
	JML $008449|!rom
.jslrtsreturn2
	JML $008243|!rom

DXBaseHijack2:
	JSR DynamicX
if !PlayerFeatures == 0    
	PHK
	PEA.w .jslrtsreturn2-1
	PEA.w $0084CE|!rom
	JML $00A300|!rom
.jslrtsreturn2
endif
    BIT.W $0D9B|!addr
    JML $0082DD|!rom
endif

if !ControllerOptimization || !PaletteChange
DXGameModeHijack:
    CLI                       ; Enable IRQ 
    INC $13                   ; Increment frame number

.Start
if !ControllerOptimization
    JSL $008650|!rom
endif

	PHK
	PEA.w .jslrtsreturn-1
	PEA.w $0084CE|!rom
	JML $009322|!rom
.jslrtsreturn

.End
if !PaletteChange
	LDA $0100|!addr
	CMP #$14
	BNE +
	LDA $13D4|!addr
    BEQ +
	JMP ..skipPalSetup
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
endif
..skipPalSetup
endif
    JML $008075|!rom
    dl $000000,$000000
endif

if !DrawingSystem
incsrc "Management/Drawing.asm"
endif
if !PaletteChange
incsrc "Management/Palette.asm"
endif
if !PlayerFeatures
incsrc "Management/Player.asm"
endif
if !GraphicChange
incsrc "NMI/VRAMDMA.asm"
endif
if !PaletteChange
incsrc "NMI/ColorPaletteChange.asm"
endif

End:

print dec(Routines)
print freespaceuse
endif