?CarryRelease:
	STZ !PlayerCarryingFlag
	STZ !PlayerCarryingFlagImage

	LDA !ButtonPressed_BYETUDLR
	BIT #$08
	BNE ?.TossUp
	AND #$04
	BEQ ?.Kick

	LDA #$09
	STA !SpriteStatus,x
	LDA #$10                ; \ Disable collisions between Mario
	STA !SpriteDecTimer2,x  ; / and this sprite for 16 frames?.
RTL

?.TossUp 
	LDA #$09
	STA !SpriteStatus,x
	LDA #$90              	; \ Sprite Y speed = -112
	STA !SpriteYSpeed,X    		; /
	LDA !PlayerXSpeed       	; \ Sprite X speed = 1/2 * Mario X speed
	STA !SpriteXSpeed,X    		;  |(The ASL moves the sign bit to the
	ASL                     	;  | carry flag, such that the ROR
	ROR !SpriteXSpeed,X    		; /  performs signed division by 2?.)
	BRA ?.StartKickPose
	
?.Kick
	LDA #$0A                	; \ Sprite action = Kicked 
	STA !SpriteStatus,x  		; / 
	LDY !PlayerDirection      	; \ Y = 0 if Mario faces left, no Yoshi
	LDA $187A|!addr         	;  |Y = 1 if Mario faces right, no Yoshi
	BEQ ?+           			;  |Y = 2 if Mario and Yoshi face left
	INY                       	;  |Y = 3 if Mario and Yoshi face right
	INY                       	; /
?+
    PHB
    PHK
    PLB
	LDA ?.ShellSpeedX,Y      	 ; \ Sprite X speed = Value from table
	STA !SpriteXSpeed,X    		; /   -46, 46, -52, 52 indexed by Y
	EOR !PlayerXSpeed       	; \ Skip ahead unless sign bits of Mario
	BMI ?.StartKickPose1       	; /   and sprite X speeds are equal
	LDA !PlayerXSpeed       	; \ Carry flag = sign bit of Mario X
	STA $00                   	;  |  speed
	ASL $00                   	; /
	ROR                       	; \ Sprite X speed = Sprite X speed +
	CLC                       	;  |  1/2 * Mario X speed
	ADC ?.ShellSpeedX,Y       	;  |
	STA !SpriteXSpeed,X    		; /
?.StartKickPose1
    PLB

?.StartKickPose
	LDA #$10                ; \ Disable collisions between Mario
	STA !SpriteDecTimer2,x  ; / and this sprite for 16 frames?.
	LDA #$0C                ; \ Display pose of Mario kicking for
	STA $149A|!addr    		; / 12 frames?.
	JSL $01AB6F|!rom
RTL                       	; Return from CODE_019F9B

?.ShellSpeedX
	db $D2,$2E,$CC,$34
