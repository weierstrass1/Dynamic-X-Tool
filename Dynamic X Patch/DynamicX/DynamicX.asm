    !sa1 = 0

if read1($00FFD5) == $23
    !sa1 = 1
    fullsa1rom
endif

incsrc "DynamicXDefines.asm"

incsrc "Hijacks/BaseHijacks.asm"

reset freespaceuse
freecode

Routines:
if read2($00823D+4) == $8449 && read1($0082DA+4) == $09
    dl $000000
    dl $000000
    dl $000000
else
    dl !PoseWasLoaded
    dl !TakeDynamicRequest
    dl !Draw
endif
    dl IsValid|!rom
    dl RemapOamTile|!rom
    dl XIsValid|!rom
    dl YIsValid|!rom
if read2($00823D+4) == $8449 && read1($0082DA+4) == $09
    dl $000000
    dl $000000
    dl $000000
else
    dl !Draw_Return
    dl !ResourceTable
    dl !GraphicRoutinesTable
endif
    dl AssignPalette|!rom
    dl SetHSLBase|!rom
    dl SetRGBBase|!rom
    dl MixHSL|!rom
    dl MixRGB|!rom
if read2($00823D+4) == $8449 && read1($0082DA+4) == $09
    dl $000000
else
    dl !SetPropertyAndOffset
endif
GameModeTable:
    db $00,$00,$01,$01,$01,$01,$01,$01
    ;  g00,g01,g02,g03,g04,g05,g06,g07
    db $01,$01,$01,$00,$02,$02,$02,$01
    ;  g08,g09,g0A,g0B,g0C,g0D,g0E,g0F
    db $00,$01,$01,$01,$01,$01,$01,$00
    ;  g10,g11,g12,g13,g14,g15,g16,g17
    db $01,$01,$01,$01,$01,$01,$01,$01
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

    JSR VRAMDMA
    JSR CGRAMDMA
    JSR CGRAMToBufferDMA

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
    LDA #$FF
    STA DX_PPU_CGRAM_Transfer_Length
    STA DX_PPU_CGRAM_BufferTransfer_Length
    REP #$20
    LDA #$0000
    STA DX_Dynamic_Palettes_Updated+$00
    STA DX_Dynamic_Palettes_Updated+$02
    STA DX_Dynamic_Palettes_Updated+$04
    STA DX_Dynamic_Palettes_Updated+$06
    STA DX_Dynamic_Palettes_DisableTimer+$00
    STA DX_Dynamic_Palettes_DisableTimer+$02
    STA DX_Dynamic_Palettes_DisableTimer+$04
    STA DX_Dynamic_Palettes_DisableTimer+$06
    STA DX_Dynamic_Pose_Length
    STA DX_Dynamic_CurrentDataSend
    !i = 0
    while !i < 128
    STA DX_Dynamic_Pose_HashSize+!i
    !i #= !i+2
    endif
    LDA #$0800
    STA DX_Dynamic_MaxDataPerFrame 
    LDA #$FFFF
    STA DX_Dynamic_Palettes_ID+$00
    STA DX_Dynamic_Palettes_ID+$02
    STA DX_Dynamic_Palettes_ID+$04
    STA DX_Dynamic_Palettes_ID+$06
    STA DX_Dynamic_Palettes_ID+$08
    STA DX_Dynamic_Palettes_ID+$0A
    STA DX_Dynamic_Palettes_ID+$0C
    STA DX_Dynamic_Palettes_ID+$0E
    !i = 0
    while !i < 256
    STA DX_Dynamic_Pose_ID+!i
    !i #= !i+2
    endif
    SEP #$20

    LDA #$00
    STA DX_PPU_VRAM_Transfer_Length

    LDA #$3F|$80
    STA DX_Dynamic_Tile_Size
    STA DX_Dynamic_Tile_Size+$3F

    STA DX_Dynamic_Tile_Size+$40
    STA DX_Dynamic_Tile_Size+$7F
    LDA #$80
    STA DX_Dynamic_Tile_Offset
    STA DX_Dynamic_Tile_Offset+$3F
    LDA #$40
    STA DX_Dynamic_Tile_Offset+$40
    STA DX_Dynamic_Tile_Offset+$7F

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
	BIT.w $0D9B|!addr
	BVS +
	JML $0082E8|!rom
+	
	JML $0082DF|!rom

DXEndFrameHijack:
    LDA #$00
    STA DX_Dynamic_Palettes_Updated+$00
    STA DX_Dynamic_Palettes_Updated+$01
    STA DX_Dynamic_Palettes_Updated+$02
    STA DX_Dynamic_Palettes_Updated+$03
    STA DX_Dynamic_Palettes_Updated+$04
    STA DX_Dynamic_Palettes_Updated+$05
    STA DX_Dynamic_Palettes_Updated+$06
    STA DX_Dynamic_Palettes_Updated+$07

    !i = $00
while !i < $08
    LDA DX_Dynamic_Palettes_DisableTimer+!i
    BEQ +
    DEC A
    STA DX_Dynamic_Palettes_DisableTimer+!i
+
    !i #= !i+$01
endif
    STZ $10
    JML $00806B|!rom

incsrc "Management/Drawing.asm"
incsrc "Management/Palette.asm"
incsrc "NMI/VRAMDMA.asm"
incsrc "NMI/ColorPaletteChange.asm"

End:

print dec(Routines)
print freespaceuse