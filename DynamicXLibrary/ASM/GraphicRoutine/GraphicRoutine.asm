!rom = $800000
if read1($00FFD5) == $23
    sa1rom
    !rom = $000000
endif

!Routines #= ((read1($0082DE)<<16)+read2($008241))|!rom

!PoseWasLoaded = read3(!Routines+$00)
!TakeDynamicRequest = read3(!Routines+$03)
!Draw = read3(!Routines+$06)
!IsValid = read3(!Routines+$09)
!RemapOamTile = read3(!Routines+$0C)
!XIsValid = read3(!Routines+$0F)
!YIsValid = read3(!Routines+$12)
!Draw_Return = read3(!Routines+$15)
!ResourceTable = read3(!Routines+$18)
!GraphicRoutinesTable = read3(!Routines+$1B)

org !GraphicRoutinesTable+<address>
    dl snestopc(GraphicRoutine)+$1F8

freecode cleaned

!XOffSet = $00
!YOffSet = $02
!Property = $04
!Tile = $05
!PoseID = $06
!PoseOffset = $08
!MaxTilePriority = $0A

!GraphicRoutine = $45
!Iterator = !GraphicRoutine+3
!maxtile_pointer = !Iterator+2

!maxtile_pointer_max        = $6180       ; 16 bytes
!maxtile_pointer_high       = $6190       ; 16 bytes
!maxtile_pointer_normal     = $61A0       ; 16 bytes
!maxtile_pointer_low        = $61B0       ; 16 bytes

!TileX = $0B
!TileY = $0D
!TileSize = $0F

GraphicRoutine:
    PHB
    PHK
    PLB

    <Position_PreLoop>
    <Tile_PreLoop>

    <LoopSection>

<Tables>

print dec(snestopc(GraphicRoutine)-8)