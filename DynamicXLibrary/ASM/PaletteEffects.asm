if read1($00FFD5) == $23
    fullsa1rom
else
    lorom
endif

incsrc "../DynamicX/ExtraDefines/DynamicXDefines.asm"

org !Routines+$33
    dl Start
    dl ApplyPaletteEffect|!rom
    dl LoadPaletteOnBuffer|!rom

org !PostFrameHijack
    JML GlobalPaletteEffect|!rom
reset freespaceuse
freecode cleaned

Start:

macro ProcessPalette(bitcheck,palette,paletteEnable,paletteCopy,baseloaded)
    BIT #<bitcheck>
    BEQ ?+

    LDA #<palette>
    STA $00
	LDA #<bitcheck>
	STA $01

    REP #$30
    LDA DX_Dynamic_Palettes_GlobalEffectID
    CMP DX_Dynamic_Palettes_LastGlobalEffectID+(2*<palette>)
    BEQ ?++
    JSR LoadPaletteEffect
	BVC ?++
	BCS ?+++
	LDA <paletteCopy>
	ORA $01
	STA <paletteCopy>
	BRA ?++
?+++
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	STA DX_Dynamic_Palettes_LastGlobalEffectID+(2*<palette>)
	SEP #$20
?++
    SEP #$30
	LDA <paletteEnable>
?+
endmacro

GlobalPaletteEffect:

if !sa1
	LDA.b #.init
	STA $3180
	LDA.b #.init>>8
	STA $3181
	LDA.b #.init>>16
	STA $3182
	JSR $1E80
    STZ $10
    JML $00806B|!rom
endif
.init
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

	JSR LoadFixedColorEffect
    LDA DX_Dynamic_Palettes_GlobalBGEnable
	%ProcessPalette($80,$00,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
	%ProcessPalette($40,$01,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
	%ProcessPalette($20,$02,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
	%ProcessPalette($10,$03,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
	%ProcessPalette($08,$04,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
	%ProcessPalette($04,$05,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
	%ProcessPalette($02,$06,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
	%ProcessPalette($01,$07,DX_Dynamic_Palettes_GlobalBGEnable,DX_PPU_CGRAM_BGPaletteCopyLoaded,DX_PPU_CGRAM_BGPaletteCopyLoaded)
    LDA DX_Dynamic_Palettes_GlobalSPEnable
	%ProcessPalette($80,$08,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)
	%ProcessPalette($40,$09,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)
	%ProcessPalette($20,$0A,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)
	%ProcessPalette($10,$0B,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)
	%ProcessPalette($08,$0C,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)
	%ProcessPalette($04,$0D,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)
	%ProcessPalette($02,$0E,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)
	%ProcessPalette($01,$0F,DX_Dynamic_Palettes_GlobalSPEnable,DX_PPU_CGRAM_SPPaletteCopyLoaded,DX_PPU_CGRAM_SPPaletteCopyLoaded)

if !sa1
RTL
else
    STZ $10
    JML $00806B|!rom
endif

LoadFixedColorEffect:
	LDA DX_PPU_FixedColor_Enable
	BNE +
RTS
+
	REP #$30
    LDA DX_Dynamic_Palettes_GlobalEffectID
    CMP DX_PPU_FixedColor_LastGlobalEffectID
	BNE +
	SEP #$30
RTS
+
	STA DX_PPU_FixedColor_LastGlobalEffectID
	TAX
	LDA DX_PPU_FixedColor_CopyLoaded
	AND #$00FF
	BNE +
	LDA #$0001
	STA DX_PPU_FixedColor_CopyLoaded
	LDA $0701|!addr
	STA DX_PPU_FixedColor_Copy
+

	CPX #$0000
	BNE +

	LDA DX_PPU_FixedColor_Copy
	STA $0701|!addr
	SEP #$30
RTS
+
	DEX
    LDA !PaletteEffectsTypes,x
	AND #$00FF
    BEQ .RGB
	JMP .HSL
.RGB
	LDA DX_PPU_FixedColor_RGBBaseLoaded
	AND #$00FF
	BNE ..SkipBaseSetup

	PHX
	SEP #$30
	%SetRGBBase("#DX_PPU_FixedColor_Copy","#DX_PPU_FixedColor_Copy>>16","#DX_PPU_FixedColor_RGBBase","#DX_PPU_FixedColor_RGBBase>>16",#$0001)
	
	LDA #$01
	STA DX_PPU_FixedColor_RGBBaseLoaded
	REP #$10
	PLX
..SkipBaseSetup
	SEP #$20
	%MixRGB("!PaletteEffectsRatios1,x","!PaletteEffectsRatios2,x","!PaletteEffectsRatios3,x","!PaletteEffectsChannels1,x","!PaletteEffectsChannels2,x","!PaletteEffectsChannels3,x","#DX_PPU_FixedColor_RGBBase","#DX_PPU_FixedColor_RGBBase>>16","#$0701","#!normalBnk>>16",#$0001)
RTS
.HSL
	LDA DX_PPU_FixedColor_HSLBaseLoaded
	AND #$00FF
	BNE ..SkipBaseSetup

	PHX
	SEP #$30
	%SetHSLBase("#DX_PPU_FixedColor_Copy","#DX_PPU_FixedColor_Copy>>16","#DX_PPU_FixedColor_HSLBase","#DX_PPU_FixedColor_HSLBase>>16",#$0001)
	
	LDA #$01
	STA DX_PPU_FixedColor_HSLBaseLoaded
	REP #$10
	PLX
..SkipBaseSetup
	SEP #$20
	%MixHSL("!PaletteEffectsRatios1,x","!PaletteEffectsRatios2,x","!PaletteEffectsRatios3,x","!PaletteEffectsChannels1,x","!PaletteEffectsChannels2,x","!PaletteEffectsChannels3,x","#DX_PPU_FixedColor_HSLBase","#DX_PPU_FixedColor_HSLBase>>16","#$0701","#!normalBnk>>16",#$0001)
RTS

LoadPaletteEffect:
    TAX
	SEP #$20

	LDA $00
	CMP #$08
	BCS .SPPal
.BGPal
	LDA DX_PPU_CGRAM_BGPaletteCopyLoaded
	STA $02
	LDA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
	STA $03
	LDA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
	STA $04
	BRA +
.SPPal
	LDA DX_PPU_CGRAM_SPPaletteCopyLoaded
	STA $02
	LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
	STA $03
	LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
	STA $04
+

	LDA $00
	ASL
	ASL
	ASL
	ASL
	INC A
	STA $05
	STZ $06

	LDA $02
	AND $01
	BNE .Start

	%TransferToCGRAMBufferNoConstant($05, #$001E)
	BCS +

	REP #$41
RTS
+
	SEP #$40
	CLC
RTS

.Start

	CPX #$0000
	BNE ++

	STZ $01
	REP #$20
	LDA $05
	ASL
	CLC
	ADC #DX_PPU_CGRAM_PaletteCopy
	STA $00
	SEP #$20
	%TransferToCGRAM($05, $00,"#DX_PPU_CGRAM_PaletteCopy>>16", #$001E)
	BCS +
	REP #$40
	SEC
RTS
+
	SEP #$41
RTS
++
	DEX
	REP #$20
	LDA $05
	PHA
	SEP #$20

    LDA !PaletteEffectsTypes,x
    BEQ .RGB
	JMP .HSL
.RGB

	LDA $00
	CMP #$08
	BCS ..SPPal
..BGPal
	LDA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
	ORA $01
	STA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
	BRA +
..SPPal
	LDA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
	ORA $01
	STA DX_PPU_CGRAM_SPBaseRGBPaletteLoaded
+

	LDA $03
	AND $01
	BNE ..SkipBaseSetup

	PHX
	%SetRGBBaseDefault("$05",#$000F)
	REP #$10
	PLX

..SkipBaseSetup
	%MixRGBDefault("!PaletteEffectsRatios1,x","!PaletteEffectsRatios2,x","!PaletteEffectsRatios3,x","!PaletteEffectsChannels1,x","!PaletteEffectsChannels2,x","!PaletteEffectsChannels3,x", "$01,s", #$000F)
	JMP .End
.HSL

	LDA $00
	CMP #$08
	BCS ..SPPal
..BGPal
	LDA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
	ORA $01
	STA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
	BRA +
..SPPal
	LDA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
	ORA $01
	STA DX_PPU_CGRAM_SPBaseHSLPaletteLoaded
+
	LDA $04
	AND $01
	BNE ..SkipBaseSetup

	PHX
	%SetHSLBaseDefault("$05",#$000F)
	REP #$10
	PLX

..SkipBaseSetup
	%MixHSLDefault("!PaletteEffectsRatios1,x","!PaletteEffectsRatios2,x","!PaletteEffectsRatios3,x","!PaletteEffectsChannels1,x","!PaletteEffectsChannels2,x","!PaletteEffectsChannels3,x", "$01,s", #$000F)
.End
	STZ $01
	LDA #$00
	XBA
	LDA $01,s
	REP #$20
	ASL
	CLC
	ADC #DX_PPU_CGRAM_PaletteWriteMirror
	STA $00
	PLA 
	STA $02
	SEP #$20
	%TransferToCGRAM($02, $00,"#DX_PPU_CGRAM_PaletteWriteMirror>>16", #$001E)
	BCS +
	REP #$40
	SEC
RTS
+
	SEP #$41
RTS

;Input:
;   Use REP #$30 before the routine
;   DX_PPU_CGRAM_PaletteCopy =  Put the colors of the palette here (is a buffer of 512 bytes, every 2 bytes is 1 color in BGR555)
;                               You can use !LoadPaletteOnBuffer or use macro TransferToCGRAMBuffer(CGRAMOffset, Lenght)
;   $00 = Color ID (PPPP CCCC)
;       PPPP : Palette ID
;       CCCC : Color on the Palette
;   $02 = Number Of Colors
;   A = Palette Effect ID (16 bits)
;   Carry : Set => Load (RGB or HSL) Base. If it is the first time that you use a effect that you need to use it, but the others times
;                   it is not needed. The base for RGB and the base for HSL are separate.
;Output:
;   Carry Set : Sucessfully Loaded
;   Carry Clear: Load Failed
;   This changes X value
ApplyPaletteEffect:
    TAX
    STZ $01
    STZ $03
    LDA $02
    PHA
    LDA $00
    PHA
    SEP #$20
    LDA !PaletteEffectsTypes,x
    BEQ .RGB
	JMP .HSL
.RGB
    BCC ..skipSetBase
    PHX
    %SetRGBBaseDefault($00,$02)
    REP #$10
    PLX

..skipSetBase

	%MixRGBDefault("!PaletteEffectsRatios1,x","!PaletteEffectsRatios2,x","!PaletteEffectsRatios3,x","!PaletteEffectsChannels1,x","!PaletteEffectsChannels2,x","!PaletteEffectsChannels3,x", "$01,s", "$03,s")

    BRA .End
.HSL
    BCC ..skipSetBase
    PHX
    %SetHSLBaseDefault($00,$02)
    REP #$10
    PLX
..skipSetBase
	%MixHSLDefault("!PaletteEffectsRatios1,x","!PaletteEffectsRatios2,x","!PaletteEffectsRatios3,x","!PaletteEffectsChannels1,x","!PaletteEffectsChannels2,x","!PaletteEffectsChannels3,x", "$01,s", "$03,s")

.End
    REP #$20
    PLA 
    STA $00
    ASL
    CLC
    ADC #DX_PPU_CGRAM_PaletteWriteMirror
    STA $04
    PLA 
    STA $02
    SEP #$20

    %TransferToCGRAM($00, $04,"#DX_PPU_CGRAM_PaletteWriteMirror>>16", $02)
RTL

;Input:
;   $00 = Color ID (PPPP CCCC)
;       PPPP : Palette
;       CCCC : Color on the Palette
;   $02-$04 = Palette Address (16 bits)
;   $05 = Number Of Colors
;Output:
;   The palette is loaded on DX_PPU_CGRAM_PaletteCopy
;   This changes X value
LoadPaletteOnBuffer:
if !sa1
    LDA.B #.init				; \ Put the SNES pointer to run
    STA $0183				; | in $0183-$0185.
    LDA.B #.init/256			; |
    STA $0184				; | (Remember that $0000-$07FF
    LDA.B #.init/65536			; |  is same as $3000-$37FF).
    STA $0185				; /
    LDA #$D0				; \ Invoke/Call SNES
    STA $2209				; /
    .Wait					; \ Wait for SNES.
    LDA $018A				; |
    BEQ .Wait				; |
    STZ $018A				; /
RTL
endif
.init
    STZ $01
    LDA #$00
    XBA
    LDA $05
    REP #$20
    STA $4305

    LDA #$8000
    STA $4300

    LDA $02
    STA $4302

    LDA $00
    ASL
    CLC
    ADC.w #DX_PPU_CGRAM_PaletteCopy  ;Get lower 16-bits of destination ptr
    STA $2181                        ;Set WRAM offset
    SEP #$20

    LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
    STA $2183                        ;Set WRAM bank (only LSB is significant)

    LDA #$01                         ;DMA transfer mode=auto increment
    STA $420B                        ;Initiate transfer using channel 0
RTL

print dec(snestopc(Start))
print freespaceuse
    