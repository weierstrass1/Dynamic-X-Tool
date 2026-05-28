!ClusterXSpeed = !ClusterMiscTable2
!ClusterXAccel = !ClusterMiscTable4

!ClusterXFrac = !ClusterMiscTable6
!ClusterXSpeedFrac = !ClusterMiscTable8
?UpdateClusterXPosition:
    LDA !ClusterXAccel,x
    LSR
    LSR
    LSR
    LSR
    BIT #$08
    BEQ ?+
    ORA #$F0
?+
    CLC
    ADC !ClusterXSpeed,x
    STA !ClusterXSpeed,x

    LDA !ClusterXAccel,x
    BMI ?+
    AND #$0F
    BRA ?++
?+
    ORA #$F0
?++
    CLC 
    ADC !ClusterXSpeedFrac,x
    STA !ClusterXSpeedFrac,x
    BPL ?+
    CMP #$F1
    BCS ?.Position
    LSR
    LSR
    LSR
    LSR
    ORA #$F0
    CLC
    ADC !ClusterXSpeed,x
    STA !ClusterXSpeed,x

    LDA !ClusterXSpeedFrac,x
    ORA #$F0
    STA !ClusterXSpeedFrac,x
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
    ADC !ClusterXSpeed,x
    STA !ClusterXSpeed,x

    LDA !ClusterXSpeedFrac,x
    AND #$0F
    STA !ClusterXSpeedFrac,x
?.Position

    LDA !ClusterXHigh,x
    STA $01
    LDA !ClusterXLow,x
    STA $00

    LDA #$00
    XBA
    LDA !ClusterXSpeed,x
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

    LDA !ClusterXSpeed,x
    BMI ?+
    AND #$0F
    BRA ?++
?+
    ORA #$F0
?++
    CLC 
    ADC !ClusterXFrac,x
    STA !ClusterXFrac,x
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

    LDA !ClusterXFrac,x
    ORA #$F0
    STA !ClusterXFrac,x
?.end1
    LDA $00
    STA !ClusterXLow,x
    LDA $01
    STA !ClusterXHigh,x
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

    LDA !ClusterXFrac,x
    AND #$0F
    STA !ClusterXFrac,x
?.end2
    LDA $00
    STA !ClusterXLow,x
    LDA $01
    STA !ClusterXHigh,x
RTL