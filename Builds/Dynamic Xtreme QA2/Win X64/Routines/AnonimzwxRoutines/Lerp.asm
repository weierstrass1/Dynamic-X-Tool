;$00-$01 = Val 1
;$02-$03 = Val 2
;$04 = Lerp Value (between 0 and 40)
?Lerp:
    STZ $05
    REP #$20
    LDA $02
    SEC
    SBC $00
    BPL ?+
    INC $05
    EOR #$FFFF
    INC A
?+
    STA $06
    SEP #$20

    %MulW($06,$04)
    REP #$20
    LDA !MultiplicationResult
    STA $08
    SEP #$20

    %MulW($07,$04)
    LDA !MultiplicationResult
    CLC
    ADC $09
    STA $09

    REP #$20
    LDA $08
    LSR
    LSR
    LSR
    LSR
    LSR
    LSR
    STA $08
    SEP #$20

    LDA $05
    BEQ ?+
    REP #$20
    LDA $08
    EOR #$FFFF
    INC A
    STA $08
    SEP #$20
?+
    REP #$21
    LDA $00
    ADC $08
    STA $00
    SEP #$20
RTL