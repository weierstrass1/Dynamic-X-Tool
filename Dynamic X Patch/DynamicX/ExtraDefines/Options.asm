!True = 1
!False = 0
!GraphicChange = !True
!PaletteChange = !True
!PaletteEffects = !True
!DynamicPoses = !True
!DrawingSystem = !True
!ControllerOptimization = !True
!FixedColorOptimization = !True
!ScrollingOptimization = !True
!StatusBarOptimization = !False
!PlayerFeatures = !True
!Desinstallation = 0

if !GraphicChange == 0
    !DynamicPoses = 0
endif
if !PaletteChange == 0
    !PaletteEffects  = 0
endif

if !GraphicChange == 0 && !PaletteChange == 0 && !DrawingSystem == 0 && !ControllerOptimization == 0 && !FixedColorOptimization == 0 && !ScrollingOptimization == 0 && !StatusBarOptimization == 0 && !PlayerFeatures == 0
    !Desinstallation = 1
endif