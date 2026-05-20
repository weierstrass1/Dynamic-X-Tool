?Clear:
    LDA !Segments,x
    BNE ?+
RTL
?+
    LDA !SpriteXHigh,x
    STA $01
    LDA !SpriteXLow,x
    STA $00

    LDA !SpriteYHigh,x
    STA $03
    LDA !SpriteYLow,x
    STA $02

    LDA !SpriteIndex
    XBA
    LDA #$00
    REP #$30
    TAX

    LDA $00
    !i = 0
    while !i < 128
    STA !CircularBuffer+!i,x
    !i #= !i+2
    endif

    LDA $02
    !i = 0
    while !i < 128
    STA !CircularBuffer+$80+!i,x
    !i #= !i+2
    endif

    SEP #$30
    LDX !SpriteIndex
RTL
