..Loop
	LDX !maxtile_pointer+0
	CPX !maxtile_pointer+4
	BEQ ..End

    <Position_InLoop>

    <Tile_InLoop>

    DEX #4
    STX !maxtile_pointer+0

    LDX !maxtile_pointer+2
    LDA !TileSize
    STA $400000,x
    DEX
    STX !maxtile_pointer+2

..next
    DEY

    DEC !Iterator
    BPL ..Loop

..End
    PLB
    JML !Draw_Return