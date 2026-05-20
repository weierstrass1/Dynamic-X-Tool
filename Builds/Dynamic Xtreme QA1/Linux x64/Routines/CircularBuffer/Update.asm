?Update:
    LDA !Segments,x
    BNE +
RTL
+
    LDA !CurrentBufferValue,x
    DEC A
	DEC A
    AND #$7F
    STA !CurrentBufferValue,x

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
    LDA !CurrentBufferValue,x
    REP #$30
    TAX

    LDA $00
    STA !CircularBuffer,x
    LDA $02
    STA !CircularBuffer+$80,x

    SEP #$30
    LDX !SpriteIndex
RTL
