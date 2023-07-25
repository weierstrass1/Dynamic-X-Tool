if !GraphicChange || !PaletteChange

org $00821D
	BRA MarioGFX
	dl Routines
org $00823A
MarioGFX:
if !PlayerFeatures == 0  
	JSR $A300
else
	BRA +
	NOP
+
endif
	autoclean JML DXBaseHijack1

org $0082D7
	autoclean JML DXBaseHijack2
else
org $00821D
	db $20,$00,$A3,$80,$1B
org $00823A
	db $20,$00,$A3,$20,$D2,$85,$20,$49,$84
org $0082D7
	db $20,$00,$A3,$2C,$9B,$0D
endif

if !ControllerOptimization || !PaletteChange
org $00806F
	autoclean JML DXGameModeHijack
	NOP
	NOP
else
	db $58,$E6,$13,$20,$22,$93
endif

if !ControllerOptimization
;Controller Optimization
org $0082F4
	BRA +
	NOP
+

org $008243
	BRA +
	NOP
+

org $0086C6
RTL
else
org $0082F4
	db $20,$50,$86

org $008243
	db $20,$50,$86

org $0086C6
RTS
endif

if !FixedColorOptimization
;Fixed Color Data NMI Optimization
org $00A4D1
	JSR $AE41

org $00AE41
	REP #$20
	LDA $0701|!addr
	ASL #3
	SEP #$21
	ROR #3
	XBA
	ORA #$40
	STA $2132
	LDA $0702|!addr
	LSR A
	SEC
	ROR
	STA $2132
	XBA
	STA $2132
RTS
	NOP #3
else
org $00A4D1
	db $20,$47,$AE
org $00AE41
DATA_00AE41:
	db $00,$05,$0A				;3
DATA_00AE44:
	db $20,$40,$80				;6
CODE_00AE47:
	LDX.B #$02					;8
--            
	REP #$20                  	;10
	LDA.W $0701|!addr           ;13
	LDY.W DATA_00AE41,X       	;16
-
	DEY                       	;17
	BMI +           			;19
	LSR                       	;20
	BRA -          				;22

+
	SEP #$20                  	;24
	AND.B #$1F                	;26
	ORA.W DATA_00AE44,X       	;29
	STA.W $2132               	;32
	DEX                       	;33
	BPL --           			;35
RTS                       		;36
endif

if !StatusBarOptimization
;Status Bar Optimizer
; Read these from ROM to account for DMA remap
!dma_reg #= read2($00A53F+1)
!dma_en  #= read1($00A55F+1)

; Hijack here to make it work with "RAM Toggled Status Bar"
; Code size: 50 bytes (leaves 18 free bytes at $008DE9)
; Code speed: about 0.78 scanlines faster (0.6 with FastROM enabled)
org $008DB1
    REP #$10			;2
    LDX.w #$5042		;5
	STX.w $2116			;8
    LDX.w #$1800		;11
	STX.w !dma_reg+0	;14
    LDX.w #$0EF9|!addr	;17
	STX.w !dma_reg+2	;20
    STZ.w !dma_reg+4	;23
    LDY.w #$001C		;26
	STY.w !dma_reg+5	;29
    LDA.b #!dma_en		;31
	STA.w $420B			;34
    LDX.w #$5063		;37
	STX.w $2116			;40
    DEY					;41
	STY.w !dma_reg+5	;44
    STA.w $420B			;47
    SEP #$10			;49
RTS						;50
	db $44,$58
elseif read1($008DB1+$32) == $44 && read1($008DB2+$32) == $58
org $008DB1
	STA.W $2116               ;  |Set Address for VRAM Read/Write to x5042 ; Address for VRAM Read/Write (Low Byte)
	LDA.B #$50                ;  | 
	STA.W $2117               ; /  ; Address for VRAM Read/Write (High Byte)
	LDX.B #$06                ; \  
-
	LDA.W DMAdata_StBr1,X     ;  |Load settings from DMAdata_StBr1 into DMA channel 1 
	STA.W $4310,X             ;  | 
	DEX                       ;  | 
	BPL -           		  ; /  
	LDA.B #$02                ; \ Activate DMA channel 1 
	STA.W $420B               ; /  ; Regular DMA Channel Enable
	STZ.W $2115               ; Set VRAM Address Increment Value to x00 ; VRAM Address Increment Value
	LDA.B #$63                ; \  
	STA.W $2116               ;  |Set Address for VRAM Read/Write to x5063 ; Address for VRAM Read/Write (Low Byte)
	LDA.B #$50                ;  | 
	STA.W $2117               ; /  ; Address for VRAM Read/Write (High Byte)
	LDX.B #$06                ; \  

-
	LDA.W DMAdata_StBr2,x     ;  |Load settings from DMAdata_StBr2 into DMA channel 1 
	STA.W $4310,x             ;  | 
	DEX                       ;  | 
	BPL -           		  ; /  
	LDA.B #$02                ; \ Activate DMA channel 1 
	STA.W $420B               ; /  ; Regular DMA Channel Enable
RTS                       ; Return 

DMAdata_StBr1:
	db $00,$18,$F9,$0E,$00,$1C,$00
DMAdata_StBr2:
	db $00,$18,$15,$0F,$00,$1B,$00
endif

if !PlayerFeatures

org $00F636
	autoclean JML PlayerDynamicRoutine
	RTS
	NOP
org $01E19D
	autoclean JML PodooboDMA
	NOP

org $01EEAA
	autoclean JML YoshiDMA

org $02EA34
	autoclean JML IDKDMA
else
org $00F636
	REP #$20                  ; Accum (16 bit) 
	LDX.B #$00                
	LDA $09

org $01E19D
	REP #$20                  ; Accum (16 bit) 
	LDA.W #$0008 

org $01EEAA
	REP #$20                  ; Accum (16 bit) 
	LDA $00      

org $02EA34
	REP #$20                  ; Accum (16 bit) 
	LDA $00  	
endif