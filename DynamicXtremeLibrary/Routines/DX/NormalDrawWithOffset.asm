!XOffSet = $00
!YOffSet = $02
!Property = $04
!Tile = $05
!PoseID = $06
!PoseOffset = $08
!MaxTilePriority = $0A

!PropParam = $52
!MaxTilePriorityParam = $0A
!XOffsetParam = $8A
!YOffsetParam = $8B

!NormalXLow = !SpriteXLow
!NormalYLow = !SpriteYLow
!NormalXHigh = !SpriteXHigh
!NormalYHigh = !SpriteYHigh

;$52 = Property (YXCC ----)
;$0A = Max Tile Priority ($00 = Maximum, $01 = High, $02 = Normal, $03 = Lowest)
;$8A = X Offset
;$8B = Y Offset
;A = Base Pose ID, 16 bits
?DXNormalDrawWithOffset:
    JSR ?.draw
RTL

?.draw
    %StandardSpriteDraw(1, "Normal")