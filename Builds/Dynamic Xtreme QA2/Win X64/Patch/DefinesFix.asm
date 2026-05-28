!Routines = Vector


!BufferTable = Data_BufferTable
!PaletteTables = Data_PaletteTable
!PaletteEffectsTable = Data_PaletteTable

!AllowedGameMode = Routines_AllowedGameMode

if !DynamicPoses
!DynamicPoseSpaceConfig = Routines_DynamicPoseSpaceConfig
!SetPropertyAndOffset = Routines_SetPropertyAndOffset
!TakeDynamicRequest = Routines_TakeDynamicRequest
!DynamicRoutine = Routines_DynamicRoutine
endif

if !DrawingSystem
!Draw = Routines_Draw
!Draw_Return = Routines_Draw_Return
!RemapOamTile = Routines_RemapOamTile
!IsValid = Routines_IsValid
!XIsValid = Routines_XIsValid
!YIsValid = Routines_YIsValid
endif

if !PalettesChange
!AssignPalette = Routines_AssignPalette
endif

if !PalettesEffects
!DoEffect = Routines_DoEffect
!DoEffectAndMerge = Routines_DoEffectAndMerge
!LoadPaletteOnBuffer = Routines_LoadPaletteOnBuffer
!RGBToHSL = Routines_RGBToHSL
!HSLToRGB = Routines_HSLToRGB
!RGBMerge = Routines_RGBMerge
!SetRGBBase = Routines_SetRGBBase
!SetHSLBase = Routines_SetHSLBase
!MixR = Routines_MixR
!MixG = Routines_MixG
!MixB = Routines_MixB
!MixH = Routines_MixH
!MixS = Routines_MixS
!MixL = Routines_MixL
!MixRG = Routines_MixRG
!MixRB = Routines_MixRB
!MixGB = Routines_MixGB
!MixHS = Routines_MixHS
!MixHL = Routines_MixHL
!MixSL = Routines_MixSL
!MixRGB = Routines_MixRGB
!MixHSL = Routines_MixHSL
!PalTransitionRGB = Routines_PalTransitionRGB
!PalTransitionHSL = Routines_PalTransitionHSL
!PalFunctionRGB = Routines_PalFunctionRGB
!PalFunctionHSL = Routines_PalFunctionHSL
!MixAndMergeR = Routines_MixAndMergeR
!MixAndMergeG = Routines_MixAndMergeG
!MixAndMergeB = Routines_MixAndMergeB
!MixAndMergeH = Routines_MixAndMergeH
!MixAndMergeS = Routines_MixAndMergeS
!MixAndMergeL = Routines_MixAndMergeL
!MixAndMergeRG = Routines_MixAndMergeRG
!MixAndMergeRB = Routines_MixAndMergeRB
!MixAndMergeGB = Routines_MixAndMergeGB
!MixAndMergeHS = Routines_MixAndMergeHS
!MixAndMergeHL = Routines_MixAndMergeHL
!MixAndMergeSL = Routines_MixAndMergeSL
!MixAndMergeRGB = Routines_MixAndMergeRGB
!MixAndMergeHSL = Routines_MixAndMergeHSL
!PalTransitionAndMergeRGB = Routines_PalTransitionAndMergeRGB
!PalTransitionAndMergeHSL = Routines_PalTransitionAndMergeHSL
!PalFunctionAndMergeRGB = Routines_PalFunctionAndMergeRGB
!PalFunctionAndMergeHSL = Routines_PalFunctionAndMergeHSL

!NumberOfPaletteEffects = read2(Data_PaletteEffectsTable_Length)
!PaletteEffectsTypes = Data_PaletteEffectsTable_Types
!PaletteEffectsChannels1 = Data_PaletteEffectsTable_Value1
!PaletteEffectsChannels2 = Data_PaletteEffectsTable_Value2
!PaletteEffectsChannels3 = Data_PaletteEffectsTable_Value3
!PaletteEffectsRatios1 = Data_PaletteEffectsTable_Ratio1
!PaletteEffectsRatios2 = Data_PaletteEffectsTable_Ratio2
!PaletteEffectsRatios3 = Data_PaletteEffectsTable_Ratio3
endif
