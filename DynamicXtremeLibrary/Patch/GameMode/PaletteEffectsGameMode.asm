!Source = $45
!Dst = $48 
!iSource = $4B
!iDst = $4D
!length = $4F
!ratio1 = $8A
!ratio2 = $8C
!ratio3 = $8E
!V1 = $51
!V2 = $53
!V3 = $6A
!tmprl = $0E
!tmprh = $0F

!Target = $51

!ratio1Inv = $D5
!ratio2Inv = $D6
!ratio3Inv = $D7

!FixedColorFlag = $01
!PaletteColor00Flag = $02

..PalettEffects
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	STA $98
	CMP DX_Dynamic_Palettes_LastGlobalEffectID
	BEQ +
	STA DX_Dynamic_Palettes_LastGlobalEffectID

	LDA #$0000
	STA DX_PPU_CGRAM_BGEffectLoaded
+
	SEP #$20

	PHB
	PHK
	PLB

	JSL LoadFixedGlobalColorEffect
	JSL LoadPaletteColor00GlobalEffect
	LDA DX_Dynamic_Palettes_GlobalBGEnable
	BEQ +
!i = 0
while !i < 8
	LDA.b #!i
	JSL LoadBGPaletteGlobalEffect
!i #= !i+1
endwhile
+
	LDA DX_Dynamic_Palettes_GlobalSPEnable
	BEQ +
!i = 0
while !i < 8
	LDA.b #!i
	JSL LoadSPPaletteGlobalEffect
!i #= !i+1
endwhile
+
	PLB
if !sa1
RTL
else
JML $008075|!rom
endif

;Input:
;$98 = Palette Effect ID, 16 bits
LoadFixedGlobalColorEffect:
	LDA DX_PPU_ColorEffect_Enable
	AND #!FixedColorFlag
	BNE +
RTL
+
	REP #$20
	LDA $98
	CMP DX_PPU_FixedColor_LastGlobalEffectID
	SEP #$20
	BNE +
RTL
+
;Input:
;$98 = Palette Effect ID, 16 bits
ForceLoadFixedGlobalColorEffect:
	LDA #!FixedColorFlag
	BRA ProcessGlobalColorEffect

;Input:
;$98 = Palette Effect ID, 16 bits
LoadPaletteColor00GlobalEffect:
	LDA DX_PPU_ColorEffect_Enable
	AND #!PaletteColor00Flag
	BNE +
RTL
+
	REP #$20
	LDA $98
	CMP DX_PPU_PaletteColor00_LastGlobalEffectID
	SEP #$20
	BNE +
RTL
+
;Input:
;$98 = Palette Effect ID, 16 bits
ForceLoadPaletteColor00GlobalEffect:
	LDA #!PaletteColor00Flag

;Input:
;$98 = Palette Effect ID, 16 bits
;A = !FixedColorFlag or !PaletteColor00Flag, 8 bits
;Scratch:
;$65 = Color model (0 = RGB, 1 = HSL)
;$66 = Target flag
ProcessGlobalColorEffect:
	STA $66

	LDA DX_PPU_ColorEffect_BaseLoaded
	AND $66
	BNE .BaseLoaded

	LDA $66
	CMP #!FixedColorFlag
	BNE .SetBaseLoaded

	REP #$20
	LDA DX_PPU_FixedColor_Mirror
	STA DX_PPU_FixedColor_Base
	SEP #$20

.SetBaseLoaded
	LDA DX_PPU_ColorEffect_BaseLoaded
	ORA $66
	STA DX_PPU_ColorEffect_BaseLoaded

.BaseLoaded
	REP #$30
	LDA $98
	BNE +
	SEP #$30
	JMP .CopyBase
+
	DEC A
	TAX
	SEP #$20

	LDA !PaletteEffectsTypes,x
	SEP #$10
	AND #$01
	STA $65
	BNE .LoadHSLBase

.LoadRGBBase
	LDA DX_PPU_ColorEffect_RGBBaseLoaded
	AND $66
	BNE .ApplyEffect

	JSR .SetRGBBase
	LDA DX_PPU_ColorEffect_RGBBaseLoaded
	ORA $66
	STA DX_PPU_ColorEffect_RGBBaseLoaded
	BRA .ApplyEffect

.LoadHSLBase
	LDA DX_PPU_ColorEffect_HSLBaseLoaded
	AND $66
	BNE .ApplyEffect

	JSR .SetHSLBase
	LDA DX_PPU_ColorEffect_HSLBaseLoaded
	ORA $66
	STA DX_PPU_ColorEffect_HSLBaseLoaded

.ApplyEffect
	JSR .DoEffect
	BRA .Publish

.CopyBase
	LDA $66
	CMP #!FixedColorFlag
	BNE .CopyPaletteColor00Base

	REP #$20
	LDA DX_PPU_FixedColor_Base
	STA DX_PPU_FixedColor_Result
	SEP #$20
	BRA .Publish

.CopyPaletteColor00Base
	REP #$20
	LDA DX_PPU_PaletteColor00_Base
	STA DX_PPU_PaletteColor00_Result
	SEP #$20

.Publish
	LDA $66
	CMP #!FixedColorFlag
	BNE .PublishPaletteColor00

	REP #$20
	LDA DX_PPU_FixedColor_Result
	STA DX_PPU_FixedColor_Mirror
	LDA $98
	STA DX_PPU_FixedColor_LastGlobalEffectID
	SEP #$20
RTL

.PublishPaletteColor00
	LDA #$01
	STA DX_PPU_PaletteColor00_Update
	REP #$20
	LDA $98
	STA DX_PPU_PaletteColor00_LastGlobalEffectID
	SEP #$20
RTL

.SetRGBBase
	LDA $66
	CMP #!FixedColorFlag
	BNE ..PaletteColor00

	%SetRGBBase("#DX_PPU_FixedColor_Base","#DX_PPU_FixedColor_Base>>16","#DX_PPU_FixedColor_RGBBase","#DX_PPU_FixedColor_RGBBase>>16","s",#$0001)
RTS

..PaletteColor00
	%SetRGBBase("#DX_PPU_PaletteColor00_Base","#DX_PPU_PaletteColor00_Base>>16","#DX_PPU_PaletteColor00_RGBBase","#DX_PPU_PaletteColor00_RGBBase>>16","s",#$0001)
RTS

.SetHSLBase
	LDA $66
	CMP #!FixedColorFlag
	BNE ..PaletteColor00

	%SetHSLBase("#DX_PPU_FixedColor_Base","#DX_PPU_FixedColor_Base>>16","#DX_PPU_FixedColor_HSLBase","#DX_PPU_FixedColor_HSLBase>>16","s",#$0001)
RTS

..PaletteColor00
	%SetHSLBase("#DX_PPU_PaletteColor00_Base","#DX_PPU_PaletteColor00_Base>>16","#DX_PPU_PaletteColor00_HSLBase","#DX_PPU_PaletteColor00_HSLBase>>16","s",#$0001)
RTS

.DoEffect
	LDA $65
	BEQ +
	JMP ..HSL
+

	LDA $66
	CMP #!FixedColorFlag
	BNE ..PaletteColor00RGB

	%DoEffectAndMerge($98,"#DX_PPU_FixedColor_RGBBase","#DX_PPU_FixedColor_RGBBase>>16","#DX_PPU_FixedColor_Result","#DX_PPU_FixedColor_Result>>16","s",#$0001)
RTS

..PaletteColor00RGB
	%DoEffectAndMerge($98,"#DX_PPU_PaletteColor00_RGBBase","#DX_PPU_PaletteColor00_RGBBase>>16","#DX_PPU_PaletteColor00_Result","#DX_PPU_PaletteColor00_Result>>16","s",#$0001)
RTS

..HSL
	LDA $66
	CMP #!FixedColorFlag
	BNE ..PaletteColor00HSL

	%DoEffectAndMerge($98,"#DX_PPU_FixedColor_HSLBase","#DX_PPU_FixedColor_HSLBase>>16","#DX_PPU_FixedColor_Result","#DX_PPU_FixedColor_Result>>16","s",#$0001)
RTS

..PaletteColor00HSL
	%DoEffectAndMerge($98,"#DX_PPU_PaletteColor00_HSLBase","#DX_PPU_PaletteColor00_HSLBase>>16","#DX_PPU_PaletteColor00_Result","#DX_PPU_PaletteColor00_Result>>16","s",#$0001)
RTS

;Input:
;$98 = Palette Effect ID, 16 bits
;A = Palette (between 0 and 7), 8 bits
LoadBGPaletteGlobalEffect:
	TAX
	LDA DX_Dynamic_Palettes_GlobalBGEnable
	AND.l LoadPaletteGlobalEffect_BitCheck,x
	BNE +
RTL
+
	TXA
	ASL
	TAX
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	CMP DX_Dynamic_Palettes_LastGlobalEffectIDPerPal,x
	BNE +
	SEP #$20
RTL
+
	STA $98
	SEP #$20
	TXA
	LSR
	BRA LoadPaletteGlobalEffect

;Input:
;$98 = Palette Effect ID, 16 bits
;A = Palette (between 0 and 7), 8 bits
LoadSPPaletteGlobalEffect:
	TAX
	LDA DX_Dynamic_Palettes_GlobalSPEnable
	AND.l LoadPaletteGlobalEffect_BitCheck,x
	BNE +
RTL
+
	TXA
	ASL
	TAX
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	CMP DX_Dynamic_Palettes_LastGlobalEffectIDPerPal+$10,x
	BNE +
	SEP #$20
RTL
+
	STA $98
	SEP #$20
	TXA
	LSR
	CLC
	ADC #$08
;Input:
;$98 = Palette Effect ID, 16 bits
;A = Palette (between 0 and F), 8 bits
LoadPaletteGlobalEffect:
	ASL
	TAX
.SetBase
	REP #$20
	LDA $98
	BNE +
	LDA DX_PPU_CGRAM_BGEffectLoaded
	BIT .BitCheckExtended,x
	SEP #$20
	BNE ++

	TXA
	ASL
	ASL
	ASL
	STA $00
	REP #$20
	AND #$00FF
	ASL
	CLC
	ADC.w #DX_PPU_CGRAM_PaletteCopy
	STA $01
	SEP #$20
	LDA.b #DX_PPU_CGRAM_PaletteCopy>>16
	STA $03

	JSR UploadPalette
	BCC ++

	REP #$20
	LDA DX_PPU_CGRAM_BGEffectLoaded
	ORA .BitCheckExtended,x
	STA DX_PPU_CGRAM_BGEffectLoaded

	LDA $98
	STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal,x
	SEP #$20
++
RTL
+
	STX $D5

	REP #$30
	LDA $98
	DEC A
	TAX

	SEP #$20
	LDA !PaletteEffectsTypes,x
	SEP #$10
	AND #$01
	STA $D6
	ASL
	TAX

	LDA $D5
	ASL
	ASL
	ASL
	STA $00
	STZ $01
	
	JSR (.SetBaseRoutines,x)

.DoEffect
	LDX $D5
	REP #$20
	LDA DX_PPU_CGRAM_BGEffectLoaded
	BIT .BitCheckExtended,x
	BNE .UploadToCGRAM
	SEP #$20

	PHX

	LDA $D5
	ASL
	ASL
	ASL
	STA $00

	LDA #DX_PPU_CGRAM_PaletteWriteMirror>>16
	STA !Dst+2
	STA !Source+2

	REP #$31

	LDA #$0010
	STA !length

	LDA #DX_PPU_CGRAM_PaletteWriteMirror
	STA !Dst

	LDA $D6
	AND #$0001
	BNE +
	LDA #DX_PPU_CGRAM_BaseRGBPalette
	BRA ++
+
	LDA #DX_PPU_CGRAM_BaseHSLPalette
++
	STA !Source

	LDA $98
	JSL Routines_DoEffectAndMerge
	PLX
.UploadToCGRAM
	SEP #$20

	TXA
	ASL
	ASL
	ASL
	STA $00
	REP #$20
	AND #$00FF
	ASL
	CLC
	ADC.w #DX_PPU_CGRAM_PaletteWriteMirror
	STA $01
	SEP #$20
	LDA.b #DX_PPU_CGRAM_PaletteWriteMirror>>16
	STA $03

	JSR UploadPalette
	BCC ++
	
	REP #$20
	LDA $98
	STA DX_Dynamic_Palettes_LastGlobalEffectIDPerPal,x

	LDA DX_PPU_CGRAM_BGEffectLoaded
	ORA .BitCheckExtended,x
	STA DX_PPU_CGRAM_BGEffectLoaded
	SEP #$20
++
RTL
.BitCheck
	db $01,$02,$04,$08,$10,$20,$40,$80
.BitCheckExtended
	dw $0001,$0002,$0004,$0008,$0010,$0020,$0040,$0080,$0100,$0200,$0400,$0800,$1000,$2000,$4000,$8000
.SetBaseRoutines
	dw .SetBaseRGB
	dw .SetBaseHSL
.SetBaseRGB
	LDX $D5
	REP #$20
	LDA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
	BIT .BitCheckExtended,x
	BEQ +
	SEP #$20
RTS
+
	ORA .BitCheckExtended,x
	STA DX_PPU_CGRAM_BGBaseRGBPaletteLoaded
	SEP #$20

	PHX
	%SetRGBBaseDefault($00,#$0010)
	PLX
RTS

.SetBaseHSL
	LDX $D5
	REP #$20
	LDA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
	BIT .BitCheckExtended,x
	BEQ +
	SEP #$20
RTS
+
	ORA .BitCheckExtended,x
	STA DX_PPU_CGRAM_BGBaseHSLPaletteLoaded
	SEP #$20

	PHX
	%SetHSLBaseDefault($00,#$0010)
	PLX
RTS

UploadPalette:
	PHX
	%TransferToCGRAM($00,$01,$03,#$0020)
	PHP

    LDA $00
	REP #$31
	AND #$00FF
    ASL
    ADC #$001E
    TAX
    LDY #$001E
-
    LDA [$01],y
    STA.w $0905|!addr,x
	DEX
	DEX
    DEY
    DEY
    BPL -
	SEP #$30

	PLP
	PLX
RTS
