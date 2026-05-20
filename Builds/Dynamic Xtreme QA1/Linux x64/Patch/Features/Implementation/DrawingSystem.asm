macro GRPositionPreLoop(DefaultXdisp, DefaultYdisp)
if <DefaultXdisp> && <DefaultYdisp>
    STZ !TileX
    STZ !TileY
    STZ !TileSize
    JSL Routines_IsValid
    BCS +
    PLB
JML Routines_Draw_Return
+
    LDA !TileSize
    STA !Iterator+1
elseif <DefaultXdisp>
    STZ !TileX
    STZ !TileSize
    JSL Routines_XIsValid
    BCS +
    PLB
JML Routines_Draw_Return
+
    LDA !TileSize
    STA !Iterator
elseif <DefaultYdisp>
    STZ !TileY
    JSL Routines_YIsValid
    BCS +
    PLB
JML Routines_Draw_Return
+
endif
endmacro

macro GRSA1Start()
    LDX !maxtile_pointer+0
	CPX !maxtile_pointer+4
	BEQ ..End
endmacro

macro GRLoromStart()
    LDA #$00
    XBA
    LDA !maxtile_pointer
    BPL +
    PLB
    JML !Draw_Return
+
    TAX
    LDA DX_Drawing_OAMMap,x
    BNE ..next2
    
    REP #$20
    TXA
    ASL
    ASL
    TAX
    SEP #$20
    LDA !OAMYPos,x
    CMP #$F0
    BNE ..next2
endmacro

macro GRStart()
if !sa1
    %GRSA1Start()
else
    %GRLoromStart()
endif
endmacro

macro GRSizeInLoop(DefaultSize, Size16)
if <DefaultSize>
    if <Size16>
        LDA #$02
    else
        LDA #$00
    endif
else
    LDA .Sizes,y
endif
endmacro

macro GRPositionInLoop(DefaultXdisp, DefaultYdisp, XDisp, YDisp, DefaultSize, Size16)
if <DefaultXdisp> && <DefaultYdisp>
    LDA !TileX
    STA !OAMXPos,x
    LDA !TileY
    STA !OAMYPos,x

    %GRSizeInLoop("<DefaultSize>", "<Size16>")
    CLC
    ADC !Iterator+1
    STA !TileSize
elseif <DefaultXdisp>
    LDA <YDisp>,y
    STA !TileY
    JSL Routines_YIsValid
    BCC ..next3
    LDA !TileX
    STA !OAMXPos,x
    LDA !TileY
    STA !OAMYPos,x

    %GRSizeInLoop("<DefaultSize>", "<Size16>")
    CLC
    ADC !Iterator+1
    STA !TileSize
elseif <DefaultYdisp>
    LDA <XDisp>,y
    STA !TileX

    %GRSizeInLoop("<DefaultSize>", "<Size16>")
    STA !TileSize
    JSL Routines_XIsValid
    BCC ..next3
    LDA !TileX
    STA !OAMXPos,x
    LDA !TileY
    STA !OAMYPos,x
else
    LDA <XDisp>,y
    STA !TileX
    LDA <YDisp>,y
    STA !TileY

    %GRSizeInLoop("<DefaultSize>", "<Size16>")
    STA !TileSize
    JSL Routines_IsValid
    BCC ..next3
    LDA !TileX
    STA !OAMXPos,x
    LDA !TileY
    STA !OAMYPos,x
endif
endmacro

macro GRPropertyInLoop(DefaultProp, lda)
if <lda>
    LDA !Property
else
    ORA !Property
endif
if <DefaultProp> == 0
    EOR .Properties,y
endif
endmacro

macro TileInloop(IsDynamic, DefaultTile, DefaultProp)
if <IsDynamic>
    if <DefaultTile>
        LDA !PoseOffset
        STA !OAMTiles,x
        %GRPropertyInLoop("<DefaultProp>", 1)
        STA !OAMProps,x
    else
        LDA .Tiles,y
        JSL !RemapOamTile
        %GRPropertyInLoop("<DefaultProp>", 0 )
        STA !OAMProps,x
        LDA $8A
        STA !OAMTiles,x
    endif
else
    if <DefaultTile>
        STA !OAMTiles,x
        %GRPropertyInLoop("<DefaultProp>", 1)
        STA !OAMProps,x
    else
        LDA .Tiles,y
        STA !OAMTiles,x
        %GRPropertyInLoop("<DefaultProp>", 1)
        STA !OAMProps,x
    endif
endif
endmacro

macro GRSizeSetup()
if !sa1
    DEX #4
    STX !maxtile_pointer+0

    LDX !maxtile_pointer+2
    LDA !TileSize
    STA !OAMSize,x
    DEX
    STX !maxtile_pointer+2
else
    LDA #$00
    XBA
    LDA !maxtile_pointer
    TAX
    DEC A
    STA !maxtile_pointer

    LDA !TileSize
    STA !OAMSize,x
endif
endmacro

macro GRLoopNext()
if !sa1
..next3
endif
..next
    DEY

    DEC !Iterator
    BPL ..Loop

if !sa1 == 0
    BRA ..End

.next2
    LDA !maxtile_pointer
    DEC A
    STA !maxtile_pointer
    BPL ..Loop
    BRA ..End

..next3
    LDA !maxtile_pointer
    DEC A
    STA !maxtile_pointer

    DEY
    DEC !Iterator
    BPL ..Loop
endif
endmacro

macro GRLoopSection(IsDynamic, DefaultXdisp, DefaultYdisp, XDisp, YDisp, DefaultTile, DefaultProp, DefaultSize, Size16)
    %GRStart()

    %GRPositionInLoop("<DefaultXdisp>", "<DefaultYdisp>", "<XDisp>", "<YDisp>", "<DefaultSize>", "<Size16>")

    %TileInloop("<IsDynamic>", "<DefaultTile>", "<DefaultProp>")

    %GRSizeSetup()

    %GRLoopNext()
endmacro

macro GRSection(IsDynamic, DefaultXdisp, DefaultYdisp, XDisp, YDisp, DefaultTile, DefaultProp, DefaultSize, Size16)
..Loop
    %GRLoopSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", "<XDisp>", "<YDisp>", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
..End
    PLB
JML Routines_Draw_Return
endmacro

macro GraphicRoutine(IsDynamic, DefaultXdisp, DefaultYdisp, FlipX, FlipY, DefaultTile, DefaultProp, DefaultSize, Size16)
    PHB
    PHK
    PLB

    %GRPositionPreLoop("<DefaultXdisp>", "<DefaultYdisp>")

if <FlipX> && <FlipY>
        LDA !Property
        AND #$C0
        BEQ .NoFlip
        JMP .Flip
    .NoFlip
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacements", ".YDisplacements", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
    .Flip
        CMP #$C0
        BNE .FlipXY
        JMP .FlipXOrY
    .FlipXY
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacementsFlip", ".YDisplacementsFlip", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
    .FlipXOrY
        BIT #$80
        BEQ .FlipX
        JMP .FlipY
    .FlipX
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacementsFlip", ".YDisplacements", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
    .FlipY
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacements", ".YDisplacementsFlip", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
elseif <FlipX>
        LDA !Property
        AND #$40
        BEQ .NoFlip
        JMP .FlipX
    .NoFlip
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacements", ".YDisplacements", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
    .FlipX
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacementsFlip", ".YDisplacements", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
elseif <FlipY>
        LDA !Property
        AND #$80
        BEQ .NoFlip
        JMP .FlipY
    .NoFlip
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacements", ".YDisplacements", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
    .FlipY
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacements", ".YDisplacementsFlip", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
else
    .NoFlip
        %GRSection("<IsDynamic>", "<DefaultXdisp>", "<DefaultYdisp>", ".XDisplacements", ".YDisplacements", "<DefaultTile>", "<DefaultProp>", "<DefaultSize>", "<Size16>")
endif
endmacro
