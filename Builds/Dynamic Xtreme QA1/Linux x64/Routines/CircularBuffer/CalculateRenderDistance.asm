!MaxXDist = $00
!MaxYDist = $02
!BufferIndexer = $8A
!SegmentIter = $8E

?CalculateRenderDistance:
    LDA !SpriteIndex
    STA !BufferIndexer+1

	LDA !SpriteXLow,x
	STA $08
	LDA !SpriteXHigh,x
	STA $09

	LDA !SpriteYLow,x
	STA $0A
	LDA !SpriteYHigh,x
	STA $0B

    LDA.b #$10
	STA !MaxXDist
	STZ !MaxXDist+1
	LDA.b #$10
	STA !MaxYDist
	STZ !MaxYDist+1

   	LDA !BufferDelay,x
    ASL
    STA $F0
    CLC
    ADC !CurrentBufferValue,x
    AND #$7F
    STA !BufferIndexer

    LDA !Segments,x
    DEC A
    STA !SegmentIter
?.Loop

	JSR ?Calculate

    LDX !SpriteIndex
    LDA !BufferIndexer
    CLC
    ADC $F0
    AND #$7F
    STA !BufferIndexer

    DEC !SegmentIter
    BPL ?.Loop
	
	LDA !MaxXDist
	STA !RenderXDistanceOutOfScreen,x

	LDA !MaxYDist
	STA !RenderYDistanceOutOfScreen,x
RTL

?Calculate:

	REP #$30
	LDX !BufferIndexer

	LDA !CircularBuffer,x
	SEC
	SBC $08
	BPL ?+
	EOR #$FFFF
	INC A
?+
	CLC
	ADC $04
	CMP !MaxXDist
	BCC ?+
	STA !MaxXDist
?+

	LDA !CircularBuffer+$80,x
	SEC
	SBC $0A
	BPL ?+
	EOR #$FFFF
	INC A
?+
	CLC
	ADC $06
	CMP !MaxYDist
	BCC ?+
	STA !MaxYDist
?+
	SEP #$30
RTS
