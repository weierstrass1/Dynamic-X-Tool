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
!ExtendedLastPoseIndex = !ExtendedPoseIndex+($0C)
!ExtendedAnimationIndex = !ExtendedPoseIndex+(2*$0C)
!ExtendedAnimationPoseIndex = !ExtendedPoseIndex+(3*$0C)
!ExtendedAnimationTimer = !ExtendedPoseIndex+(4*$0C)
!ExtendedPalette = !ExtendedPoseIndex+(5*$0C)
!ExtendedGlobalFlip = !ExtendedPoseIndex+(6*$0C)
!ExtendedLocalFlip = !ExtendedPoseIndex+(7*$0C)
!ExtendedLastPoseHashIndex = !ExtendedPoseIndex+(8*$0C)

!NormalRenderXDistanceOutOfScreen = !ExtendedPoseIndex+(9*$0C)
!NormalRenderYDistanceOutOfScreen = !NormalRenderXDistanceOutOfScreen+!MaxSprites

!ClusterRenderXDistanceOutOfScreen = !NormalRenderXDistanceOutOfScreen+(2*!MaxSprites)
!ClusterRenderYDistanceOutOfScreen = !ClusterRenderXDistanceOutOfScreen+$14

!ExtendedRenderXDistanceOutOfScreen = !ClusterRenderXDistanceOutOfScreen+(2*$14)
!ExtendedRenderYDistanceOutOfScreen = !ExtendedRenderXDistanceOutOfScreen+$0C

!NormalState = !ExtendedRenderXDistanceOutOfScreen+(2*$0C)

!ClusterState = !NormalState+!MaxSprites

!ExtendedState = !ClusterState+$14

!NormalHitboxTableB = !ExtendedState+$0C
!NormalHitboxTableH = !NormalHitboxTableB+!MaxSprites
!NormalHitboxTableL = !NormalHitboxTableH+(2*!MaxSprites)

!ClusterHitboxTableB = !NormalHitboxTableL+(3*!MaxSprites)
!ClusterHitboxTableH = !ClusterHitboxTableB+$14
!ClusterHitboxTableL = !ClusterHitboxTableH+(2*$14)

!ExtendedHitboxTableB = !ClusterHitboxTableL+(3*$14)
!ExtendedHitboxTableH = !ExtendedHitboxTableB+$0C
!ExtendedHitboxTableL = !ExtendedHitboxTableH+(2*$0C)

!NormalPlayerIsAbove = !ExtendedHitboxTableL+(3*$0C)

!ClusterPlayerIsAbove = !NormalPlayerIsAbove+!MaxSprites

!ExtendedPlayerIsAbove = !ClusterPlayerIsAbove+$14

!NormalSafeFrameLowByte = !ExtendedPlayerIsAbove+$0C
!NormalSafeFrameHighByte = !NormalSafeFrameLowByte+!MaxSprites

!ClusterSafeFrameLowByte = !NormalSafeFrameHighByte+!MaxSprites
!ClusterSafeFrameHighByte = !NormalSafeFrameLowByte+$14

!ExtendedSafeFrameLowByte = !ClusterSafeFrameHighByte+$14
!ExtendedSafeFrameHighByte = !ExtendedSafeFrameLowByte+$0C

!NormalHitboxXOffset = !NormalHitboxTableB
!NormalHitboxYOffset = !NormalHitboxTableH
!NormalHitboxWidth = !NormalHitboxTableL

!ClusterHitboxXOffset = !ClusterHitboxTableB
!ClusterHitboxYOffset = !ClusterHitboxTableH
!ClusterHitboxWidth = !ClusterHitboxTableL

!ExtendedHitboxXOffset = !ExtendedHitboxTableB
!ExtendedHitboxYOffset = !ExtendedHitboxTableH
!ExtendedHitboxWidth = !ExtendedHitboxTableL

!NormalHitboxHeight = DX_FreeRams2

!ClusterHitboxHeight = !NormalHitboxHeight+!MaxSprites

!ExtendedHitboxHeight = !ClusterHitboxHeight+$14

!NormalLastFlip = !ExtendedHitboxHeight+$0C

!ClusterLastFlip = !NormalLastFlip+!MaxSprites

!ExtendedLastFlip = !ClusterLastFlip+$14

!End = !ExtendedLastFlip+$0C