
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
	db "DX"
elseif read2($008DB1+$32) == $5844
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
