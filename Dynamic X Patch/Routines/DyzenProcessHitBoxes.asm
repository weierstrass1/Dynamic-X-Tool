;A = hitbox id
;$45 = X 16 bits
;$47 = Y 16 bits
;$49 = Frame
;Output
;$00 = Left
;$02 = Right
;$08 = Top
;$0C = Bottom
;A = FF if it is the end of the hitbox 

!X = $59
!Y = $51
!Frame = $49
!HitboxTables = ($8A)           ;HitboxTables
!HitboxAdder = ($04)            ;HitboxAdder
!FrameHitboxesIndexer = ($06)   ;FrameHitboxesIndexer
!FrameHitBoxes = ($04)          ;FrameHitBoxes
!HitboxXOffset = ($06)          ;HitboxXOffset
!HitboxYOffset = ($0A)          ;HitboxYOffset
!HitboxWidth = ($0E)            ;HitboxWidth
!HitboxHeight = ($8C)           ;HitboxHeight
!HitboxAction1 = ($8E)          ;HitboxAction1
!HitboxAction2 = ($62)          ;HitboxAction2
!Actions = ($8A)                ;Actions

!Left = $45
!Top = $47
!Right = $49
!Bottom = $4B

!Left2 = $00
!Top2 = $08
!Right2 = $02
!Bottom2 = $0C

!HBStatus = $4D

!RoutineAddress = $146C|!addr

DyzenProcessHitBoxes:
    PHX
    TYX

    PHB

    LDA $8C
    PHA
    PLB

	STZ $50
	STZ $4F

    LDY #$00
    REP #$20
    LDA !HitboxTables,y
    STA $04         ;HitboxAdder
    LDY #$02
    LDA !HitboxTables,y
    STA $06         ;FrameHitboxesIndexer


    SEP #$20

    LDA #$00
    XBA
    TXA
    REP #$30
    ASL
    TAY

    LDA !Frame
	ASL
    CLC
    ADC !HitboxAdder,y
    TAY

    LDA !FrameHitboxesIndexer,y
    PHA

    LDY #$0004
    LDA !HitboxTables,y     
    STA $04         ;FrameHitBoxes
    LDY #$0008
    LDA !HitboxTables,y
    STA $06         ;HitboxXOffset
    LDY #$000A
    LDA !HitboxTables,y
    STA $0A         ;HitboxYOffset
    LDY #$000C
    LDA !HitboxTables,y
    STA $0E         ;HitboxWidth
    LDY #$000E
    LDA !HitboxTables,y
    STA $8C         ;HitboxHeight
    LDY #$0010
	LDA !HitboxTables,y
	STA $8E         ;HitboxAction1
	LDY #$0012
	LDA !HitboxTables,y
	STA $62         ;HitboxAction2
	LDY #$0014
	LDA !HitboxTables,y
	STA $8A         ;Actions

    PLY

-
    PHY
    SEP #$20
	LDA #$00
	XBA
	LDA !FrameHitBoxes,y		;FrameHitBoxes[Y]
    CMP #$FF
    BNE +

    PLY
    SEP #$10
    PLB
    PLX

    LDA $4F
    BEQ .clear
.set
    SEC
RTL
.clear
    CLC
RTL
+

    REP #$20
    ASL
    TAY

	LDA !HitboxXOffset,y		;Hitboxes[Y].XOffset
	CLC
	ADC !X				        ;A = Hitboxes[Y].XOffset + X
    STA !Left                   ;Left
    CLC
    ADC !HitboxWidth,y
    STA !Right                  ;Right

	LDA !HitboxYOffset,y		;Hitboxes[Y].YOffset
	CLC
	ADC !Y				        ;A = Hitboxes[Y].YOffset + Y
	STA !Top  			        ;Top
    CLC
    ADC !HitboxHeight,y
    STA !Bottom                 ;Bottom

.checkXAxys
	LDA !Right
	CMP !Left2
	BCC .left			        ;if HB 2 is at the left of HB 1

	LDA !Right2
	CMP !Left
	BCC .right			        ;if HB 2 is at the right of HB 1

.touchingHorizontal		    ;HB2 is touching HB1 in X axys
	LDA #$0003
	BRA +
.left
	LDA #$0002
	BRA +
.right
	LDA #$0001
+
	STA !HBStatus

.checkYAxys
	LDA !Bottom
	CMP !Top2
	BCC .up				;if HB 2 is above of HB 1

	LDA !Bottom2
	CMP !Top
	BCC .down			    ;if HB 2 is below of HB 1

.touchingVertical		    ;HB 2 is touching HB2 in Y axys
	LDA #$000C
	BRA +
.up
	LDA #$0008
	BRA +
.down
	LDA #$0004
+
	ORA !HBStatus
	STA !HBStatus

	PHY
.contact				;HB 2 is touching HB 1
	LDA !HBStatus
	CMP #$000F
	BNE .ThereIsntContact

.ThereIsContact

	LDA !HitboxAction1,y
	TAY 

	BRA .execRoutine

.ThereIsntContact

	LDA !HitboxAction2,y
	TAY

.execRoutine
	LDA !Actions,y
	STA !RoutineAddress

	PLA
	LSR
	SEP #$30
	TAY						;Y = Hitbox ID

	LDA !SpriteIndex
	TAX						;X = load sprite index

	PHB
	PLA
	STA !RoutineAddress+2

	PHK
	LDA.b #(.returnRoutine-1)>>8
	PHA
	LDA.b #(.returnRoutine-1)
	PHA
	JML [!RoutineAddress]
.returnRoutine

	REP #$10
	PLY
	INY
	JMP -