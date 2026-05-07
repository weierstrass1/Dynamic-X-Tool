
DynamicPoseSpaceConfig:
    ASL
    TAX

    PHB
    PHK
    PLB
    JSR (.Configs,x)
    PLB
RTL

.Configs
    dw ..SecondHalfSSP4
    dw ..SP4
    dw ..SecondHalfSP3
    dw ..FirstHalfSP3
    dw ..SP3
    dw ..SecondHalfSP2
    dw ..FirstHalfSP2
    dw ..SP2
    dw ..SecondHalfSP1
    dw ..FirstHalfSP1
    dw ..SP1
    dw ..FirstHalfSP1WithPlayer
    dw ..SP11WithPlayer
    dw ..SecondHalfSP3ndSP4
    dw ..SP34
    dw ..SecondHalfSP2andSP34
    dw ..SP234
    dw ..SecondHalfSP1andSP234
    dw ..SP1234
    dw ..SecondHalfSP1andSP234WithPlayer
    dw ..SP1234WithPlayer
    dw ..SecondHalfSP3ndSP4WithDSX
    dw ..SP34WithDSX
    dw ..SecondHalfSP2andSP34WithDSX
    dw ..SP234WithDSX
    dw ..SecondHalfSP1andSP234WithDSX
    dw ..SP1234WithDSX
    dw ..SecondHalfSP1andSP234WithPlayerAndDSX
    dw ..SP1234WithPlayerAndDSX

macro zone(init,end,restricted)
    LDA #(<end>-<init>)|$80
    STA DX_Dynamic_Tile_Size+<init>
    STA DX_Dynamic_Tile_Size+<end>
    LDA #<init>|<restricted>
    STA DX_Dynamic_Tile_Offset+<init>
    STA DX_Dynamic_Tile_Offset+<end>
endmacro

..SecondHalfSSP4
    %zone($00,$6F,$80)
    %zone($70,$7F,$00)
RTS

..SP4
    %zone($00,$5F,$80)
    %zone($60,$7F,$00)
RTS

..SecondHalfSP3
    %zone($00,$4F,$80)
    %zone($50,$5F,$00)
    %zone($60,$7F,$80)
RTS

..FirstHalfSP3
    %zone($00,$3F,$80)
    %zone($40,$4F,$00)
    %zone($50,$7F,$80)
RTS

..SP3
    %zone($00,$3F,$80)
    %zone($40,$5F,$00)
    %zone($60,$7F,$80)
RTS

..SecondHalfSP2
    %zone($00,$2F,$80)
    %zone($30,$3F,$00)
    %zone($40,$7F,$80)
RTS

..FirstHalfSP2
    %zone($00,$1F,$80)
    %zone($20,$2F,$00)
    %zone($30,$7F,$80)
RTS

..SP2
    %zone($00,$1F,$80)
    %zone($20,$3F,$00)
    %zone($40,$7F,$80)
RTS

..SecondHalfSP1
    %zone($00,$0F,$80)
    %zone($10,$1F,$00)
    %zone($20,$7F,$80)
RTS

..FirstHalfSP1
    %zone($00,$0F,$00)
    %zone($10,$7F,$80)
RTS

..SP1
    %zone($00,$1F,$00)
    %zone($20,$7F,$80)
RTS

..FirstHalfSP1WithPlayer
    %zone($00,$06,$80)
    %zone($07,$08,$00)
    %zone($09,$09,$80)
    %zone($0A,$0F,$80)
    %zone($10,$7F,$80)
RTS

..SP11WithPlayer
    %zone($00,$06,$80)
    %zone($07,$08,$00)
    %zone($09,$09,$80)
    %zone($0A,$14,$80)
    %zone($15,$15,$80)
    %zone($16,$1E,$00)
    %zone($1F,$7F,$80)
RTS

..SecondHalfSP3ndSP4
    %zone($00,$4F,$80)
    %zone($50,$7F,$00)
RTS

..SP34
    %zone($00,$3F,$80)
    %zone($40,$7F,$00)
RTS

..SecondHalfSP2andSP34
    %zone($00,$2F,$80)
    %zone($30,$7F,$00)
RTS

..SP234
    %zone($00,$1F,$80)
    %zone($20,$7F,$00)
RTS

..SecondHalfSP1andSP234
    %zone($00,$0F,$80)
    %zone($10,$7F,$00)
RTS

..SP1234
    %zone($00,$7F,$00)
RTS

..SecondHalfSP1andSP234WithPlayer
    %zone($00,$0F,$80)
    %zone($07,$08,$00)
    %zone($09,$09,$80)
    %zone($0A,$14,$80)
    %zone($15,$15,$80)
    %zone($16,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$7F,$00)
RTS

..SP1234WithPlayer
    %zone($00,$06,$80)
    %zone($07,$08,$00)
    %zone($09,$09,$80)
    %zone($0A,$14,$80)
    %zone($15,$15,$80)
    %zone($16,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$7F,$00)
RTS

..SecondHalfSP3ndSP4WithDSX
    %zone($00,$4F,$80)
    %zone($50,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP34WithDSX
    %zone($00,$3F,$80)
    %zone($40,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SecondHalfSP2andSP34WithDSX
    %zone($00,$2F,$80)
    %zone($30,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP234WithDSX
    %zone($00,$1F,$80)
    %zone($20,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SecondHalfSP1andSP234WithDSX
    %zone($00,$0F,$80)
    %zone($10,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP1234WithDSX
    %zone($00,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SecondHalfSP1andSP234WithPlayerAndDSX
    %zone($00,$0F,$80)
    %zone($10,$14,$00)
    %zone($15,$15,$80)
    %zone($16,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP1234WithPlayerAndDSX
    %zone($00,$06,$80)
    %zone($07,$08,$00)
    %zone($09,$09,$80)
    %zone($0A,$14,$80)
    %zone($15,$15,$80)
    %zone($16,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$6F,$00)
    %zone($70,$7F,$80)
RTS

!Property = $04
!PoseOffset = $08

SetPropertyAndOffset:
	PHB : PHK : PLB
.SetPropertyAndOffset_Start
	LDA DX_Dynamic_Pose_Offset,x : BIT #$40
	BNE +
	LDY #$00
	BRA ++
	+	
		LDY #$01
	++
	STY !Property|!dp
	TAY
	LDA VRAMDisp,y : STA !PoseOffset
	PHX
	TXA : ASL : TAX
	LDA DX_Timer : STA DX_Dynamic_Pose_TimeLastUse,x
	LDA DX_Timer+1 : STA DX_Dynamic_Pose_TimeLastUse+1,x
	PLX
	PLB
RTL

VRAMDisp:
	db $00,$02,$04,$06,$08,$0A,$0C,$0E
	db $20,$22,$24,$26,$28,$2A,$2C,$2E
	db $40,$42,$44,$46,$48,$4A,$4C,$4E
	db $60,$62,$64,$66,$68,$6A,$6C,$6E
	db $80,$82,$84,$86,$88,$8A,$8C,$8E
	db $A0,$A2,$A4,$A6,$A8,$AA,$AC,$AE
	db $C0,$C2,$C4,$C6,$C8,$CA,$CC,$CE
	db $E0,$E2,$E4,$E6,$E8,$EA,$EC,$EE
	db $00,$02,$04,$06,$08,$0A,$0C,$0E
	db $20,$22,$24,$26,$28,$2A,$2C,$2E
	db $40,$42,$44,$46,$48,$4A,$4C,$4E
	db $60,$62,$64,$66,$68,$6A,$6C,$6E
	db $80,$82,$84,$86,$88,$8A,$8C,$8E
	db $A0,$A2,$A4,$A6,$A8,$AA,$AC,$AE
	db $C0,$C2,$C4,$C6,$C8,$CA,$CC,$CE
	db $E0,$E2,$E4,$E6,$E8,$EA,$EC,$EE

incsrc "../Implementation/DynamicXSystem.asm"

!baseSourseOffset = $47
!baseSourceOffsetBNK = $49
!doubledID = $4A
!SourceOffset = $4C
!SourceSize = $4E
!VRAMOffset = $50

!CurrentVRAMOffset = $00
!CurrentSourceOffset = $02
!CurrentSecondLineOffset = $04
!CurrentLine1Size = $06
!CurrentLine2Size = $08
!SentData = $0A

OffsetToVRAMOffset:
	dw $0000,$0020,$0040,$0060,$0080,$00A0,$00C0,$00E0
	dw $0200,$0220,$0240,$0260,$0280,$02A0,$02C0,$02E0
	dw $0400,$0420,$0440,$0460,$0480,$04A0,$04C0,$04E0
	dw $0600,$0620,$0640,$0660,$0680,$06A0,$06C0,$06E0
	dw $0800,$0820,$0840,$0860,$0880,$08A0,$08C0,$08E0
	dw $0A00,$0A20,$0A40,$0A60,$0A80,$0AA0,$0AC0,$0AE0
	dw $0C00,$0C20,$0C40,$0C60,$0C80,$0CA0,$0CC0,$0CE0
	dw $0E00,$0E20,$0E40,$0E60,$0E80,$0EA0,$0EC0,$0EE0
	dw $1000,$1020,$1040,$1060,$1080,$10A0,$10C0,$10E0
	dw $1200,$1220,$1240,$1260,$1280,$12A0,$12C0,$12E0
	dw $1400,$1420,$1440,$1460,$1480,$14A0,$14C0,$14E0
	dw $1600,$1620,$1640,$1660,$1680,$16A0,$16C0,$16E0
	dw $1800,$1820,$1840,$1860,$1880,$18A0,$18C0,$18E0
	dw $1A00,$1A20,$1A40,$1A60,$1A80,$1AA0,$1AC0,$1AE0
	dw $1C00,$1C20,$1C40,$1C60,$1C80,$1CA0,$1CC0,$1CE0
	dw $1E00,$1E20,$1E40,$1E60,$1E80,$1EA0,$1EC0,$1EE0

DynamicRoutine:
    REP #$30 ;A->16 bit
		AND #$00FF
        ASL : TAY
        LDA OffsetToVRAMOffset,y : STA !VRAMOffset

        LDA.B PoseIDBackup : ASL : STA !doubledID : CLC : ADC.B PoseIDBackup
        TAY

        LDA PoseResource,y : STA !baseSourseOffset
        LDA PoseResource+1,y : STA !baseSourseOffset+1

        LDA !doubledID : ASL
        STA !doubledID : TAY

        LDA #$0000 : STA !SourceOffset
        LDA PoseResourceSizePerLine,y : STA !SourceSize
    SEP #$20 ;A->8 bit
    JSR DynamicRoutineLine

    LDY !doubledID|!dp
    LDA PoseResourceSizePerLine+2,y : BNE +
        SEP #$30 : RTS
    +
    STA !SourceSize

    LDA PoseResourceSizePerLine,y : STA !SourceOffset
    LDA !doubledID : LSR : TAY
    LDA !VRAMOffset : CLC : ADC PoseSecondLineOffset,y : STA !VRAMOffset

    JSR DynamicRoutineLine
    SEP #$30
RTL

;-------------------------------------------------
;               DynamicRoutineLine
;-------------------------------------------------
DynamicRoutineLine:
	REP #$21 ;A->16 bit, carry clear
		LDA #$0000 : STA !SentData

		LDA !VRAMOffset : ADC #$6000 : STA !CurrentVRAMOffset
		CLC : ADC #$0100 : AND #$FF00 : STA !CurrentSecondLineOffset
		SEC : SBC !CurrentVRAMOffset : ASL : STA !CurrentLine1Size

		LDA !CurrentSecondLineOffset : CLC : ADC #$0100 : STA !CurrentSecondLineOffset
		LDA #$0200 : SEC : SBC !CurrentLine1Size : STA !CurrentLine2Size
		LDA !SourceOffset : CLC : ADC !baseSourseOffset : STA !CurrentSourceOffset

		LDA !CurrentVRAMOffset : AND #$00FF : BNE .Loop
			LDA !SourceSize : CMP #$0200 : BCS .EndLine
.Loop
		LDA !SourceSize : SEC : SBC !SentData : CMP !CurrentLine1Size : BCC .EndLine : BEQ .EndLine
		CMP #$0200 : BCC .TwoLinesWithEndLineReDir : BEQ .TwoLinesWithEndLineReDir
.TwoLines
	SEP #$20 ;A->8 bit
	LDA #$00 : XBA
	LDA DX_PPU_VRAM_Transfer_Length : ASL : TAX ;X = number of transfer*2
	LSR : INC A : INC A : STA DX_PPU_VRAM_Transfer_Length ;Number of transfer ++

	LDA !baseSourceOffsetBNK                        ;\
	STA DX_PPU_VRAM_Transfer_SourceBNK,x            ;|BNK (low byte) = source bnk
	STA DX_PPU_VRAM_Transfer_SourceBNK+$02,x        ;|BNK (low byte) = source bnk
	LDA #$00                                        ;|BNK (high byte) = 0
	STA DX_PPU_VRAM_Transfer_SourceBNK+$01,x        ;/
	STA DX_PPU_VRAM_Transfer_SourceBNK+$03,x        ;/

	REP #$21 ;A->16 bit, carry clear
		LDA !CurrentVRAMOffset : STA DX_PPU_VRAM_Transfer_Offset,x
		ADC #$0100 : STA !CurrentVRAMOffset
		LDA !CurrentSecondLineOffset : STA DX_PPU_VRAM_Transfer_Offset+2,x
		CLC : ADC #$0100 : STA !CurrentSecondLineOffset

		LDA !CurrentLine1Size : STA DX_PPU_VRAM_Transfer_SourceLength,x ;MapLength = Size
		LDA !CurrentLine2Size : STA DX_PPU_VRAM_Transfer_SourceLength+2,x
		
		LDA !CurrentSourceOffset : STA DX_PPU_VRAM_Transfer_Source,x ;MapAddr = Addr
		CLC : ADC !CurrentLine1Size : STA DX_PPU_VRAM_Transfer_Source+2,x ;MapAddr = Addr
		CLC : ADC !CurrentLine2Size : STA !CurrentSourceOffset

		LDA !SentData : CLC : ADC #$0200 : STA !SentData
		BRA .Loop
.TwoLinesWithEndLineReDir
		BRA .TwoLinesWithEndLine
.EndLine
		STA !CurrentLine1Size
	SEP #$20 ;A->8 bit
	LDA #$00 : XBA
	LDA DX_PPU_VRAM_Transfer_Length : ASL : TAX ;X = number of transfer*2
	LSR : INC A : STA DX_PPU_VRAM_Transfer_Length ;Number of transfer ++

	LDA !baseSourceOffsetBNK                        ;\
	STA DX_PPU_VRAM_Transfer_SourceBNK,x            ;|BNK (low byte) = source bnk
	LDA #$00                                        ;|BNK (high byte) = 0
	STA DX_PPU_VRAM_Transfer_SourceBNK+$01,x        ;/

	REP #$20 ;A->16 bit
		LDA !CurrentVRAMOffset : STA DX_PPU_VRAM_Transfer_Offset,x
		LDA !CurrentLine1Size : STA DX_PPU_VRAM_Transfer_SourceLength,x ;MapLength = Size
		LDA !CurrentSourceOffset : STA DX_PPU_VRAM_Transfer_Source,x ;MapAddr = Addr
		RTS
.TwoLinesWithEndLine
		SEC  : SBC !CurrentLine1Size : STA !CurrentLine2Size
	SEP #$20 ;A->8 bit
	LDA #$00 : XBA
	LDA DX_PPU_VRAM_Transfer_Length : ASL : TAX ;X = number of transfer*2
	LSR : INC A : INC A : STA DX_PPU_VRAM_Transfer_Length ;Number of transfer ++

	LDA !baseSourceOffsetBNK                        ;\
	STA DX_PPU_VRAM_Transfer_SourceBNK,x            ;|BNK (low byte) = source bnk
	STA DX_PPU_VRAM_Transfer_SourceBNK+$02,x        ;|BNK (low byte) = source bnk
	LDA #$00                                        ;|BNK (high byte) = 0
	STA DX_PPU_VRAM_Transfer_SourceBNK+$01,x        ;/
	STA DX_PPU_VRAM_Transfer_SourceBNK+$03,x        ;/

	REP #$21 ;A->16 bit, carry clear
		LDA !CurrentVRAMOffset : STA DX_PPU_VRAM_Transfer_Offset,x
		LDA !CurrentSecondLineOffset : STA DX_PPU_VRAM_Transfer_Offset+2,x

		LDA !CurrentLine1Size : STA DX_PPU_VRAM_Transfer_SourceLength,x ;MapLength = Size
		LDA !CurrentLine2Size : STA DX_PPU_VRAM_Transfer_SourceLength+2,x
		
		LDA !CurrentSourceOffset : STA DX_PPU_VRAM_Transfer_Source,x ;MapAddr = Addr
		ADC !CurrentLine1Size : STA DX_PPU_VRAM_Transfer_Source+2,x ;MapAddr = Addr
RTS
