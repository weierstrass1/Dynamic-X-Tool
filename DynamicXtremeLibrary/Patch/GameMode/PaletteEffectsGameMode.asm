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

..PalettEffects
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	STA $98
	CMP DX_Dynamic_Palettes_LastGlobalEffectID
	BEQ +
	STA DX_Dynamic_Palettes_LastGlobalEffectID

	LDA #$0000
	STA DX_PPU_CGRAM_BGEffectLoaded
	SEP #$20
	STA DX_PPU_FixedColor_EffectLoaded
+
	SEP #$20

	PHB
	PHK
	PLB

	JSL LoadFixedGlobalColorEffect
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
	LDA DX_PPU_FixedColor_Enable
	BNE +
RTL
+
	REP #$20
	LDA DX_Dynamic_Palettes_GlobalEffectID
	CMP DX_PPU_FixedColor_LastGlobalEffectID
	SEP #$20
	BNE +
RTL
+
;Input:
;$98 = Palette Effect ID, 16 bits
ForceLoadFixedGlobalColorEffect:
RTL

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
