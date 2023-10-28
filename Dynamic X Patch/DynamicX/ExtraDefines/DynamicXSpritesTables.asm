!NormalGlobalFlip = !SpriteDirection

!NormalPoseIndex = DX_FreeRams
!NormalLastPoseIndex = DX_FreeRams+(!MaxSprites)
!NormalAnimationIndex = DX_FreeRams+(2*!MaxSprites)
!NormalAnimationPoseIndex = DX_FreeRams+(3*!MaxSprites)
!NormalAnimationTimer = DX_FreeRams+(4*!MaxSprites)
!NormalPalette = DX_FreeRams+(5*!MaxSprites)
!NormalLocalFlip = DX_FreeRams+(6*!MaxSprites)
!NormalLastPoseHashIndex = DX_FreeRams+(7*!MaxSprites)

!ClusterPoseIndex = DX_FreeRams+(8*!MaxSprites)
!ClusterLastPoseIndex = !ClusterPoseIndex+($14)
!ClusterAnimationIndex = !ClusterPoseIndex+(2*$14)
!ClusterAnimationPoseIndex = !ClusterPoseIndex+(3*$14)
!ClusterAnimationTimer = !ClusterPoseIndex+(4*$14)
!ClusterPalette = !ClusterPoseIndex+(5*$14)
!ClusterGlobalFlip = !ClusterPoseIndex+(6*$14)
!ClusterLocalFlip = !ClusterPoseIndex+(7*$14)
!ClusterLastPoseHashIndex = !ClusterPoseIndex+(8*$14)

!ExtendedPoseIndex = !ClusterPoseIndex+(9*$14)
!ExtendedLastPoseIndex = !ExtendedPoseIndex+($0A)
!ExtendedAnimationIndex = !ExtendedPoseIndex+(2*$0A)
!ExtendedAnimationPoseIndex = !ExtendedPoseIndex+(3*$0A)
!ExtendedAnimationTimer = !ExtendedPoseIndex+(4*$0A)
!ExtendedPalette = !ExtendedPoseIndex+(5*$0A)
!ExtendedGlobalFlip = !ExtendedPoseIndex+(6*$0A)
!ExtendedLocalFlip = !ExtendedPoseIndex+(7*$0A)
!ExtendedLastPoseHashIndex = !ExtendedPoseIndex+(8*$0A)

!NormalRenderXDistanceOutOfScreen = !ExtendedPoseIndex+(9*$0A)
!NormalRenderYDistanceOutOfScreen = !NormalRenderXDistanceOutOfScreen+!MaxSprites

!ClusterRenderXDistanceOutOfScreen = !NormalRenderXDistanceOutOfScreen+(2*!MaxSprites)
!ClusterRenderYDistanceOutOfScreen = !ClusterRenderXDistanceOutOfScreen+$14

!ExtendedRenderXDistanceOutOfScreen = !ClusterRenderXDistanceOutOfScreen+(2*$14)
!ExtendedRenderYDistanceOutOfScreen = !ExtendedRenderXDistanceOutOfScreen+$0A

!NormalState = !ExtendedRenderXDistanceOutOfScreen+(2*$0A)

!ClusterState = !NormalState+!MaxSprites

!ExtendedState = !ClusterState+$14

!NormalHitboxTableB = !ExtendedState+$0A
!NormalHitboxTableH = !NormalHitboxTableB+!MaxSprites
!NormalHitboxTableL = !NormalHitboxTableH+(2*!MaxSprites)

!ClusterHitboxTableB = !NormalHitboxTableL+(3*!MaxSprites)
!ClusterHitboxTableH = !ClusterHitboxTableB+$14
!ClusterHitboxTableL = !ClusterHitboxTableH+(2*$14)

!ExtendedHitboxTableB = !ClusterHitboxTableL+(3*$14)
!ExtendedHitboxTableH = !ExtendedHitboxTableB+$0A
!ExtendedHitboxTableL = DX_FreeRams2

!NormalPlayerIsAbove = !ExtendedHitboxTableL+$0A

!ClusterPlayerIsAbove = !NormalPlayerIsAbove+!MaxSprites

!ExtendedPlayerIsAbove = !ClusterPlayerIsAbove+$14

!NormalSafeFrameLowByte = !ExtendedPlayerIsAbove+$0A
!NormalSafeFrameHighByte = !NormalSafeFrameLowByte+!MaxSprites

!ClusterSafeFrameLowByte = !NormalSafeFrameHighByte+!MaxSprites
!ClusterSafeFrameHighByte = !NormalSafeFrameLowByte+$14

!ExtendedSafeFrameLowByte = !ClusterSafeFrameHighByte+$14
!ExtendedSafeFrameHighByte = !ExtendedSafeFrameLowByte+$0A

!NormalHitboxXOffset = !NormalHitboxTableB
!NormalHitboxYOffset = !NormalHitboxTableH
!NormalHitboxWidth = !NormalHitboxTableL

!ClusterHitboxXOffset = !ClusterHitboxTableB
!ClusterHitboxYOffset = !ClusterHitboxTableH
!ClusterHitboxWidth = !ClusterHitboxTableL

!ExtendedHitboxXOffset = !ExtendedHitboxTableB
!ExtendedHitboxYOffset = !ExtendedHitboxTableH
!ExtendedHitboxWidth = !ExtendedHitboxTableL

!NormalHitboxHeight = !ExtendedSafeFrameHighByte+$0A

!ClusterHitboxHeight = !NormalHitboxHeight+!MaxSprites

!ExtendedHitboxHeight = !ClusterHitboxHeight+$14

!NormalLastFlip = !ExtendedHitboxHeight+$0A

!ClusterLastFlip = !NormalLastFlip+!MaxSprites

!ExtendedLastFlip = !ClusterLastFlip+$14

!NormalVersion = !ExtendedLastFlip+$0A

!ClusterVersion = !NormalVersion+!MaxSprites

!ExtendedVersion = !ClusterVersion+$14

!NormalPaletteAssignment = !extra_prop_2

!ClusterPaletteAssignment = !ExtendedVersion+$0A

!ExtendedPaletteAssignment = !ClusterPaletteAssignment+$14

!NormalPaletteOption = !ExtraByte1

!ClusterPaletteOption = !ExtendedPaletteAssignment+$0A

!ExtendedPaletteOption = !ClusterPaletteOption+$14

!NormalLastVersion = !ExtendedPaletteOption+$0A

!ClusterLastVersion = !NormalLastVersion+!MaxSprites

!ExtendedLastVersion = !ClusterLastVersion+$14

!End = !ExtendedLastVersion+$0A
