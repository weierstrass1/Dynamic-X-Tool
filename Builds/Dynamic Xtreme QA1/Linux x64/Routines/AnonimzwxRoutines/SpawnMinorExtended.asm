; Routine used for spawning minor extended sprites with initial speed at the position (+offset)
; of the calling sprite and returns the sprite index in Y
; For a list of minor extended sprites see here: 
; https://www.smwcentral.net/?p=memorymap&a=detail&game=smw&region=ram&detail=11c9baba28dd

; Input:
;   A   = number
;   $00 = x offset
;   $01 = y offset
;   $02 = x speed
;   $03 = y speed
;   $04 = origin x pos  ; since this is a generic routine it can be called from any other sprite
;   $06 = origin y pos  ; type, so i opted for adding macros in _header.asm that helps to setup this

; Output:
;   Y = index to minor extended sprite ($FF means no sprite spawned)
;   C = Spawn status
;       Set = Spawn failed
;       Clear = Spawn successful

?SpawnMinorExtended:
    XBA
    
    LDY.b #!MinorExtendedSize-1
?.loop
    LDA !minor_extended_num,y
    BEQ ?.found
    DEY 
    BPL ?.loop
?.ret
    SEC
RTL

?.found
    XBA 
    sta !minor_extended_num,y
    
    LDA #$00
    XBA
    LDA $00
    REP #$20
    BPL ?+
    ORA #$FF00
    EOR #$FFFF
    INC A
?+
    CLC
    ADC $04
    SEP #$20
    STA !minor_extended_x_low,y
    XBA
    STA !minor_extended_x_high,y

    LDA #$00
    XBA
    LDA $01
    REP #$20
    BPL ?+
    ORA #$FF00
    EOR #$FFFF
    INC A
?+
    CLC
    ADC $06
    SEP #$20
    STA !minor_extended_y_low,y
    XBA
    STA !minor_extended_y_high,y

    LDA $02
    STA !minor_extended_x_speed,y
    LDA $03
    STA !minor_extended_y_speed,y
    
    CLC 
RTL