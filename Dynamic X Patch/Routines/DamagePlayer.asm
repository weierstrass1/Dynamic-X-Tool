DamagePlayer:	
	PHX

	LDA $187A|!addr		;if the player is not riding yoshi then damage the player
	BEQ +				;otherwise dismount yoshi
	JSR .FindYoshi
	BCC +
	JSR .DismountYoshi
	PLX
RTL
+
	JSL $00F5B7|!rom
	PLX
RTL

.FindYoshi
	LDX $18DF|!addr
	BEQ .crawlForYoshi
	DEX
	BRA .found
.crawlForYoshi:
	LDX.w $1692|!addr
	; Start Slot according to sprite data
	LDA.l $02A773|!rom,x
	SEC
	SBC #$FE ; spaces 2 reserved slots, have to interact with them too
	TAX
.loop:
	LDA !SpriteNumber,x
	CMP #$35
	BNE .continueLoop
	
	LDA !SpriteStatus,x
	BNE .found
.continueLoop
	DEX
	BPL .loop
.returnClear:
	CLC
	RTS
.found
	SEC
	RTS

.DismountYoshi
	LDA #$10                
	STA !SpriteDecTimer6,x             
	LDA #$03                ; \ Play sound effect 
	STA $1DFA|!addr         ; / 
	LDA #$13                ; \ Play sound effect 
	STA $1DFC|!addr         ; / 
	LDA #$02                
	STA !SpriteMiscTable3,x     
	STZ $187A|!addr         
	LDA #$C0                
	STA !PlayerYSpeed       
	STZ !PlayerXSpeed       
	%SubHorzPos()       
	LDA XSpeedDismountTable,y       
	STA !SpriteXSpeed,X    
	STZ !SpriteMiscTable10,x             
	STZ !SpriteMiscTable6,X             
	STZ $18AE|!addr               
	STZ $0DC1|!addr      
	LDA #$30                ; \ Mario invincible timer = #$30 
	STA $1497|!addr         ; / 
	JSR CODE_01EDCC         
RTS                       ; Return 

XSpeedDismountTable:
	db $E8,$18

CODE_01EDCC:
	LDY.B #$00                
	LDA !SpriteYLow,X       
	SEC                       
	SBC YoshiOffset,Y       
	STA !PlayerY         
	STA $D3                   
	LDA !SpriteYHigh,X     
	SBC #$00                
	STA !PlayerY+$01       
	STA $D4                   
RTS                       ; Return 

YoshiOffset:
	db $04,$10