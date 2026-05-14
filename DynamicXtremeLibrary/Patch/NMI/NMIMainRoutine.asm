DynamicXtreme:

    LDX $0100|!addr

    LDA.l GameModeTable,x
    BNE +
RTS
+
    LDA #$01
    STA $4200
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

    LDA $9D
    ORA $13D4|!addr
    AND #$00FF
    BNE +
    LDA DX_SyncTimer
    INC A
    STA DX_SyncTimer
+

	LDA #$4300
	TCD                 ;direct page = 4300 for speed

    SEP #$30

if !GraphicsChange
    JSR VRAMDMA
endif
if !PalettesChange
    JSR CGRAMDMA
endif

    PLD

    LDA #$81
    STA $4200
RTS

if !GraphicsChange
incsrc "VRAMDMA.asm"
endif
if !PalettesChange
incsrc "ColorPaletteChange.asm"
endif
