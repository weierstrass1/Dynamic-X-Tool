!True = 1
!False = 0

!GraphicsChange = !True
!PalettesChange = !True
!DynamicPoses = !True
!DrawingSystem = !True
!PalettesEffects = !True
!Free7E2000 = !True
!ControllerOptimization = !True
!FixedColorOptimization = !True
!KevinLMVRAMOptimization = !True
!StatusBarOptimization = !True
!PlayerFeatures = !True
!YoshiFeatures = !True

if !GraphicsChange == !False
	!DynamicPoses = !False
	!PlayerFeatures = !False
endif
if !PalettesChange == !False
	!PalettesEffects = !False
	!PlayerFeatures = !False
endif
if !PlayerFeatures == !False
	!YoshiFeatures = !False
endif
