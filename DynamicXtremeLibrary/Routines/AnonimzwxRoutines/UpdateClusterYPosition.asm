!ClusterYSpeed = !ClusterMiscTable3
!ClusterYAccel = !ClusterMiscTable5

!ClusterYFrac = !ClusterMiscTable7
!ClusterYSpeedFrac = !ClusterMiscTable9
?UpdateClusterYPosition:
    LDA !ClusterYAccel,x
    LSR
    LSR
    LSR
    LSR
    BIT #$08
    BEQ ?+
    ORA #$F0
?+
    CLC
    ADC !ClusterYSpeed,x
    STA !ClusterYSpeed,x

    LDA !ClusterYAccel,x
    BMI ?+
    AND #$0F
    BRA ?++
?+
    ORA #$F0
?++
    CLC 
    ADC !ClusterYSpeedFrac,x
    STA !ClusterYSpeedFrac,x
    BPL ?+
    CMP #$F1
    BCS ?.Position
    LSR
    LSR
    LSR
    LSR
    ORA #$F0
    CLC
    ADC !ClusterYSpeed,x
    STA !ClusterYSpeed,x

    LDA !ClusterYSpeedFrac,x
    ORA #$F0
    STA !ClusterYSpeedFrac,x
    BRA ?.Position
?+
    CMP #$10
    BCC ?.Position
    LSR
    LSR
    LSR
    LSR
    AND #$0F
    CLC
    ADC !ClusterYSpeed,x
    STA !ClusterYSpeed,x

    LDA !ClusterYSpeedFrac,x
    AND #$0F
    STA !ClusterYSpeedFrac,x
?.Position

    LDA !ClusterYHigh,x
    STA $01
    LDA !ClusterYLow,x
    STA $00

    LDA #$00
    XBA
    LDA !ClusterYSpeed,x
    LSR
    LSR
    LSR
    LSR
    BIT #$08
    BEQ ?+
    ORA #$F0
    XBA
    LDA #$FF
    XBA
?+
    REP #$20
    CLC
    ADC $00
    STA $00
    SEP #$20

    LDA !ClusterYSpeed,x
    BMI ?+
    AND #$0F
    BRA ?++
?+
    ORA #$F0
?++
    CLC 
    ADC !ClusterYFrac,x
    STA !ClusterYFrac,x
    BPL ?+
    CMP #$F1
    BCS ?.end1
    LSR
    LSR
    LSR
    LSR
    ORA #$F0
    XBA
    LDA #$FF
    XBA
    REP #$20
    CLC
    ADC $00
    STA $00
    SEP #$20

    LDA !ClusterYFrac,x
    ORA #$F0
    STA !ClusterYFrac,x
?.end1
    LDA $00
    STA !ClusterYLow,x
    LDA $01
    STA !ClusterYHigh,x
RTL
?+
    CMP #$10
    BCC ?.end2
    LSR
    LSR
    LSR
    LSR
    AND #$0F
    XBA
    LDA #$00
    XBA
    REP #$20
    CLC
    ADC $00
    STA $00
    SEP #$20

    LDA !ClusterYFrac,x
    AND #$0F
    STA !ClusterYFrac,x
?.end2
    LDA $00
    STA !ClusterYLow,x
    LDA $01
    STA !ClusterYHigh,x
RTL