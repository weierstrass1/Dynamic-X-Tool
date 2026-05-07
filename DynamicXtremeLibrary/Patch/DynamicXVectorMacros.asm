macro AddIfStatement(prefix, condition, routine)
    if <condition>
        dl <prefix>_<routine>
    else
        dl $000000
    endif
endmacro

macro AddRoutineIfStatement(condition, routine)
    %AddIfStatement("Routines", <condition>, <routine>)
endmacro

macro AddDataIfStatement(condition, routine)
    %AddIfStatement("Data", <condition>, <routine>)
endmacro

macro DataVector()
    %AddDataIfStatement("!GraphicsChange == 1 || !PalettesChange == 1", "ResourceTable")
    %AddDataIfStatement("!PalettesChange == 1", "PaletteTables")
    %AddDataIfStatement("!PalettesEffects == 1", "PaletteEffectsTable")
endmacro

macro AddRoutineIfStatement()
    %AddIfStatement("1 == 1", "AllowedGameMode")
endmacro


macro DynamicPosesRoutinesVector()
    %AddRoutineIfStatement("!DynamicPoses == 1", "DynamicPoseSpaceConfig")
    %AddRoutineIfStatement("!DynamicPoses == 1", "SetPropertyAndOffset")
    %AddRoutineIfStatement("!DynamicPoses == 1", "TakeDynamicRequest")
    %AddRoutineIfStatement("!DynamicPoses == 1", "DynamicRoutine")
    %AddRoutineIfStatement("!DynamicPoses == 1", "PoseWasLoaded")
endmacro

macro DrawingSystemRoutinesVector()
    %AddRoutineIfStatement("!DrawingSystem == 1", "Draw")
    %AddRoutineIfStatement("!DrawingSystem == 1", "Draw_Return")
    %AddRoutineIfStatement("!DrawingSystem == 1", "RemapOamTile")
    %AddRoutineIfStatement("!DrawingSystem == 1", "IsValid")
    %AddRoutineIfStatement("!DrawingSystem == 1", "XIsValid")
    %AddRoutineIfStatement("!DrawingSystem == 1", "YIsValid")
endmacro

macro PaletteChangeRoutinesVector()
    %AddRoutineIfStatement("!PaletteChange == 1", "AssignPalette")
endmacro

macro PaletteEffectsRoutinesVector()
    %AddRoutineIfStatement("!PaletteEffects == 1", DoEffect)
    %AddRoutineIfStatement("!PaletteEffects == 1", DoEffectAndMerge)
    %AddRoutineIfStatement("!PaletteEffects == 1", LoadPaletteOnBuffer)
    %AddRoutineIfStatement("!PaletteEffects == 1", RGBToHSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", HSLToRGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", RGBMerge)
    %AddRoutineIfStatement("!PaletteEffects == 1", SetRGBBase)
    %AddRoutineIfStatement("!PaletteEffects == 1", SetHSLBase)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixR)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixG)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixH)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixS)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixL)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixRG)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixRB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixHS)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixHL)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixRGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixHSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalTransitionRGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalTransitionHSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalFunctionRGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalFunctionHSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeR)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeG)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeH)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeS)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeL)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeRG)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeRB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeHS)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeHL)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeRGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", MixAndMergeHSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalTransitionAndMergeRGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalTransitionAndMergeHSL)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalFunctionAndMergeRGB)
    %AddRoutineIfStatement("!PaletteEffects == 1", PalFunctionAndMergeHSL)
endmacro
