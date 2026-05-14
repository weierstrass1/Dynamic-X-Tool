macro AddIfStatement(prefix, condition, routine)
    if <condition>
        dl <prefix>_<routine>
    else
        dl $000000
    endif
endmacro

macro AddRoutineIfStatement(condition, routine)
    %AddIfStatement("Routines", "<condition>", <routine>)
endmacro

macro AddDataIfStatement(condition, routine)
    %AddIfStatement("Data", "<condition>", <routine>)
endmacro

macro DataVector()
    %AddDataIfStatement("!GraphicsChange || !PalettesChange", "BufferTable")
    %AddDataIfStatement("!PalettesChange", "PaletteTable")
    %AddDataIfStatement("!PalettesEffects", "PaletteEffectsTable")
endmacro

macro AllowedGameModeVector()
    %AddRoutineIfStatement("1", "AllowedGameMode")
endmacro


macro DynamicPosesRoutinesVector()
    %AddRoutineIfStatement("!DynamicPoses", "DynamicPoseSpaceConfig")
    %AddRoutineIfStatement("!DynamicPoses", "SetPropertyAndOffset")
    %AddRoutineIfStatement("!DynamicPoses", "TakeDynamicRequest")
    %AddRoutineIfStatement("!DynamicPoses", "DynamicRoutine")
endmacro

macro DrawingSystemRoutinesVector()
    %AddRoutineIfStatement("!DrawingSystem", "Draw")
    %AddRoutineIfStatement("!DrawingSystem", "Draw_Return")
    %AddRoutineIfStatement("!DrawingSystem", "RemapOamTile")
    %AddRoutineIfStatement("!DrawingSystem", "IsValid")
    %AddRoutineIfStatement("!DrawingSystem", "XIsValid")
    %AddRoutineIfStatement("!DrawingSystem", "YIsValid")
endmacro

macro PaletteChangeRoutinesVector()
    %AddRoutineIfStatement("!PalettesChange", "AssignPalette")
endmacro

macro PaletteEffectsRoutinesVector()
    %AddRoutineIfStatement("!PalettesEffects", DoEffect)
    %AddRoutineIfStatement("!PalettesEffects", DoEffectAndMerge)
    %AddRoutineIfStatement("!PalettesEffects", LoadPaletteOnBuffer)
    %AddRoutineIfStatement("!PalettesEffects", RGBToHSL)
    %AddRoutineIfStatement("!PalettesEffects", HSLToRGB)
    %AddRoutineIfStatement("!PalettesEffects", RGBMerge)
    %AddRoutineIfStatement("!PalettesEffects", SetRGBBase)
    %AddRoutineIfStatement("!PalettesEffects", SetHSLBase)
    %AddRoutineIfStatement("!PalettesEffects", MixR)
    %AddRoutineIfStatement("!PalettesEffects", MixG)
    %AddRoutineIfStatement("!PalettesEffects", MixB)
    %AddRoutineIfStatement("!PalettesEffects", MixH)
    %AddRoutineIfStatement("!PalettesEffects", MixS)
    %AddRoutineIfStatement("!PalettesEffects", MixL)
    %AddRoutineIfStatement("!PalettesEffects", MixRG)
    %AddRoutineIfStatement("!PalettesEffects", MixRB)
    %AddRoutineIfStatement("!PalettesEffects", MixGB)
    %AddRoutineIfStatement("!PalettesEffects", MixHS)
    %AddRoutineIfStatement("!PalettesEffects", MixHL)
    %AddRoutineIfStatement("!PalettesEffects", MixSL)
    %AddRoutineIfStatement("!PalettesEffects", MixRGB)
    %AddRoutineIfStatement("!PalettesEffects", MixHSL)
    %AddRoutineIfStatement("!PalettesEffects", PalTransitionRGB)
    %AddRoutineIfStatement("!PalettesEffects", PalTransitionHSL)
    %AddRoutineIfStatement("!PalettesEffects", PalFunctionRGB)
    %AddRoutineIfStatement("!PalettesEffects", PalFunctionHSL)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeR)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeG)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeB)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeH)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeS)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeL)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeRG)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeRB)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeGB)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeHS)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeHL)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeSL)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeRGB)
    %AddRoutineIfStatement("!PalettesEffects", MixAndMergeHSL)
    %AddRoutineIfStatement("!PalettesEffects", PalTransitionAndMergeRGB)
    %AddRoutineIfStatement("!PalettesEffects", PalTransitionAndMergeHSL)
    %AddRoutineIfStatement("!PalettesEffects", PalFunctionAndMergeRGB)
    %AddRoutineIfStatement("!PalettesEffects", PalFunctionAndMergeHSL)
endmacro
