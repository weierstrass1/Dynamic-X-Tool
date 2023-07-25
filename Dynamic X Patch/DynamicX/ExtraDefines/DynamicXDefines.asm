incsrc "Options.asm"

    !normalBnk = $7E0000
    !sa1 = 0
    !dp = $0000
    !addr = $0000
    !rom = $800000
    !Variables = $7F9C7B 
    !Variables2 #= $7F9C7B+$800
    !MultiplicationResult = $4216
    !DivisionResult = $4214
    !RemainderResult = $4216
    !MaxSprites = $0C

if read1($00FFD5) == $23
    !normalBnk = $400000
    !sa1 = 1
    !dp = $3000
    !addr = $6000
    !rom = $000000
    !Variables = $40C000
    !Variables2 = $418B80  
    !MultiplicationResult = $2306
    !DivisionResult = $2306
    !RemainderResult = $2308
    !MaxSprites = $16
endif

if !Desinstallation == 0

!VRAMTransferSize = 64
!CGRAMTransferSize = 64
!CGRAMBufferSize = 64

pushpc
namespace nested on

org !Variables
    namespace DX
        Timer: skip 2
        namespace Dynamic
            ;Number of bytes send with DMA during the current SNES Frame
            CurrentDataSend: skip 2
            MaxDataPerFrame: skip 2
            if !DynamicPoses
            namespace Tile
                Pose:
                Size: skip 128
                Offset: skip 128
            namespace off
            namespace Pose
                ;Number of elements in the hash map
                Length: skip 1
                ;how many number with the same hash value has each possible
                ;hash value
                HashSize: skip 128
                ;Current Pose ID that was loaded to VRAM
                ID: skip 256
                ;VRAM Offset of the Pose
                ;   Format: 0 PPPPPPP
                ;       PPPPPPP: VRAM Offset (every unit is a 16x16 block)
                Offset: skip 128
                ;SNES Frames since the last time that the pose was used
                TimeLastUse: skip 256
            namespace off
            endif
            if !PaletteChange
            namespace Palettes
                GlobalBGEnable: skip 1
                GlobalSPEnable: skip 1
                GlobalEffectID: skip 2
                LastGlobalEffectID: skip 16
                ID: skip 32
                Updated: skip 16
                DisableTimer: skip 16
            namespace off
            endif
            if !PlayerFeatures
            namespace Player
                CustomPlayer: skip 1
                LastCustomPlayer: skip 1
                namespace GFX
                    Enable: skip 1
                    Addr: skip 2
                    BNK: skip 2
                namespace off
                namespace Palette
                    Enable: skip 1
                    Addr: skip 2
                    BNK: skip 1
                namespace off
            namespace off
            endif
        namespace off
        FreeRams:
org !Variables2
        namespace PPU
            if !PaletteChange
            if !PaletteEffects
            namespace FixedColor
                Enable: skip 1
                RGBBase: skip 3
                HSLBase: skip 3
                CopyLoaded: skip 1
                Copy: skip 2
                RGBBaseLoaded: skip 1
                HSLBaseLoaded: skip 1
                LastGlobalEffectID: skip 2
            namespace off
            endif
            namespace CGRAM
                namespace Transfer
                    Length: skip 1          
                    SourceLength: skip (!CGRAMTransferSize*2)  
                    Offset: skip !CGRAMTransferSize         
                    Source: skip (!CGRAMTransferSize*2)         
                    SourceBNK: skip (!CGRAMTransferSize*2)      
                namespace off
                if !PaletteEffects
                namespace BufferTransfer
                    Length: skip 1          
                    SourceLength: skip !CGRAMBufferSize   
                    Offset: skip !CGRAMBufferSize         
                    Destination: skip (!CGRAMBufferSize*2)   
                    DestinationBNK: skip (!CGRAMBufferSize*2)
                namespace off
                PaletteCopy: skip 512
                BaseRGBPalette: skip 768
                BaseHSLPalette: skip 768       
                PaletteWriteMirror: skip 512
                BGPaletteCopyLoaded: skip 1
                SPPaletteCopyLoaded: skip 1
                BGBaseRGBPaletteLoaded: skip 1
                SPBaseRGBPaletteLoaded: skip 1
                BGBaseHSLPaletteLoaded: skip 1
                SPBaseHSLPaletteLoaded: skip 1
                endif
                if !PlayerFeatures
                LastPlayerPal: skip 2  
                endif
            namespace off
            endif
            if !GraphicChange
            namespace VRAM
                namespace Transfer
                    Length: skip 1          
                    SourceLength: skip !VRAMTransferSize
                    Offset: skip !VRAMTransferSize      
                    Source: skip !VRAMTransferSize      
                    SourceBNK: skip !VRAMTransferSize   
                namespace off
            namespace off
            endif
        namespace off
        FreeRams2:
    namespace off
namespace nested off
pullpc
endif

!Routines #= (read3($00821F))|!rom

if !DynamicPoses
!PoseWasLoaded = read3(!Routines+$00)
!TakeDynamicRequest = read3(!Routines+$03)
endif
if !DrawingSystem
!Draw = read3(!Routines+$06)
!IsValid = read3(!Routines+$09)
!RemapOamTile = read3(!Routines+$0C)
!XIsValid = read3(!Routines+$0F)
!YIsValid = read3(!Routines+$12)
!Draw_Return = read3(!Routines+$15)
endif
if !DynamicPoses
!ResourceTable = read3(!Routines+$18)
endif
if !DrawingSystem
!GraphicRoutinesTable = read3(!Routines+$1B)
endif
if !PaletteChange
!AssignPalette = read3(!Routines+$1E)
if !PaletteEffects
!SetHSLBase = read3(!Routines+$21)
!SetRGBBase = read3(!Routines+$24)
!MixHSL = read3(!Routines+$27)
!MixRGB = read3(!Routines+$2A)
endif
endif
if !DynamicPoses
!SetPropertyAndOffset = read3(!Routines+$2D)
endif
if !PaletteEffects
!PaletteEffectsTable = read3(!Routines+$30)
!PaletteEffectsPatch = read3(!Routines+$33)
!ApplyPaletteEffect = read3(!Routines+$36)
!LoadPaletteOnBuffer = read3(!Routines+$39)
!AllPaletteEffects = read3(!PaletteEffectsTable)
!NumberOfPaletteEffects = read2(!AllPaletteEffects)
!PaletteEffectsTypes = !AllPaletteEffects+$02
!PaletteEffectsChannels1 = !PaletteEffectsTypes+!NumberOfPaletteEffects
!PaletteEffectsChannels2 = !PaletteEffectsChannels1+!NumberOfPaletteEffects
!PaletteEffectsChannels3 = !PaletteEffectsChannels2+!NumberOfPaletteEffects
!PaletteEffectsRatios1 = !PaletteEffectsChannels3+!NumberOfPaletteEffects
!PaletteEffectsRatios2 = !PaletteEffectsRatios1+!NumberOfPaletteEffects
!PaletteEffectsRatios3 = !PaletteEffectsRatios2+!NumberOfPaletteEffects
endif
if !PaletteChange || !ControllerOptimization
!PreFrameHijack = read3(!Routines+$3C)
!PostFrameHijack = read3(!Routines+$3F)
endif
!PaletteTables = read3(!Routines+$42)
!GameModeTable = !Routines+$45
if !Desinstallation == 0
incsrc "./Macros/STDCall.asm"
incsrc "./Macros/MultAndDiv.asm"
endif
if !DynamicPoses
incsrc "./Macros/DynamicSpritesMacros.asm"
endif
if !PaletteChange
if !PaletteEffects
incsrc "./Macros/Palettes.asm"
endif
incsrc "./Macros/PPU.asm"
endif
if !DrawingSystem
incsrc "./Macros/GraphicRoutinesMacros.asm"
endif