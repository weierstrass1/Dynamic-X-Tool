;Input:
;A = Time Displacement
;Output:
; GGI
;GG = 00 => no graphics, 01 => graphics flash, otherwise graphics
;I = 0 => no interaction, 1 => interaction
?GetGhostState:
    CLC
    ADC $14
    CMP #$60
    BCS ?+
?.visible   ;#$00-#$5F
    ;     GGI
    LDA #%00000101
RTL
?+
    CMP #$80
    BCS ?+
?.flash1    ;#$60-#$7F
    ;     GGI
    LDA #%00000011
RTL
?+
    CMP #$E0
    BCS ?.flash2
?.invisible ;#$80-#$DF
    ;     GGI
    LDA #%00000000
RTL
?.flash2    ;#$E0-#$FF
    ;     GGI
    LDA #%00000010
RTL