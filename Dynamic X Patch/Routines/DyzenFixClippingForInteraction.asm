?DyzenFixClippingForInteraction:
    LDA $08
    PHA
    LDA $01
    STA $08                         ;$08 = Y Low Byte
    PLA
    STA $01                         ;$01 = X High Byte

    ;$00 = X (16 bits)
    ;$08 = Y (16 bits)

    LDA $03
    STA $0C                         

    STZ $03                         ;$02 = Width 16 bits
    STZ $0D                         ;$0C = Height 16 bits

    REP #$20
    LDA $00
    CLC
    ADC $02
    STA $02

    LDA $08
    CLC
    ADC $0C
    STA $0C
    SEP #$20

RTL                       ; Return 