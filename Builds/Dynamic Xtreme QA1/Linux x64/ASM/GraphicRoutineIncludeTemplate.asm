namespace GraphicRoutines
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

!OAMTiles = $0202
!OAMProps = $0203
!OAMXPos = $0200
!OAMYPos = $0201
!OAMSize = $0420

if !sa1
!OAMTiles = $400002
!OAMProps = $400003
!OAMXPos = $400000
!OAMYPos = $400001
!OAMSize = $400000
endif

<grs>

namespace off
