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
    !Variables2 = $418AFF  
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
        SyncTimer: skip 2
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
                LastGlobalEffectID: skip 32
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

!FG1Line0 = $0000
!FG1Line1 = $0100
!FG1Line2 = $0200
!FG1Line3 = $0300
!FG1Line4 = $0400
!FG1Line5 = $0500
!FG1Line6 = $0600
!FG1Line7 = $0700

!FG2Line0 = $0800
!FG2Line1 = $0900
!FG2Line2 = $0A00
!FG2Line3 = $0B00
!FG2Line4 = $0C00
!FG2Line5 = $0D00
!FG2Line6 = $0E00
!FG2Line7 = $0F00

!BG1Line0 = $1000
!BG1Line1 = $1100
!BG1Line2 = $1200
!BG1Line3 = $1300
!BG1Line4 = $1400
!BG1Line5 = $1500
!BG1Line6 = $1600
!BG1Line7 = $1700

!FG3Line0 = $1800
!FG3Line1 = $1900
!FG3Line2 = $1A00
!FG3Line3 = $1B00
!FG3Line4 = $1C00
!FG3Line5 = $1D00
!FG3Line6 = $1E00
!FG3Line7 = $1F00

!BG2Line0 = $2000
!BG2Line1 = $2100
!BG2Line2 = $2200
!BG2Line3 = $2300
!BG2Line4 = $2400
!BG2Line5 = $2500
!BG2Line6 = $2600
!BG2Line7 = $2700

!BG3Line0 = $2800
!BG3Line1 = $2900
!BG3Line2 = $2A00
!BG3Line3 = $2B00
!BG3Line4 = $2C00
!BG3Line5 = $2D00
!BG3Line6 = $2E00
!BG3Line7 = $2F00

!Layer1Tilemap = $3000
!Layer2Tilemap = $3800

!LG1Line0 = $4000
!LG1Line1 = $4080
!LG1Line2 = $4100
!LG1Line3 = $4180
!LG1Line4 = $4200
!LG1Line5 = $4280
!LG1Line6 = $4300
!LG1Line7 = $4380

!LG2Line0 = $4400
!LG2Line1 = $4480
!LG2Line2 = $4500
!LG2Line3 = $4580
!LG2Line4 = $4600
!LG2Line5 = $4680
!LG2Line6 = $4700
!LG2Line7 = $4780

!LG3Line0 = $4800
!LG3Line1 = $4880
!LG3Line2 = $4900
!LG3Line3 = $4980
!LG3Line4 = $4A00
!LG3Line5 = $4A80
!LG3Line6 = $4B00
!LG3Line7 = $4B80

!LG4Line0 = $4C00
!LG4Line1 = $4C80
!LG4Line2 = $4D00
!LG4Line3 = $4D80
!LG4Line4 = $4E00
!LG4Line5 = $4E80
!LG4Line6 = $4F00
!LG4Line7 = $4F80

!SP1Line0 = $6000
!SP1Line1 = $6100
!SP1Line2 = $6200
!SP1Line3 = $6300
!SP1Line4 = $6400
!SP1Line5 = $6500
!SP1Line6 = $6600
!SP1Line7 = $6700

!SP2Line0 = $6800
!SP2Line1 = $6900
!SP2Line2 = $6A00
!SP2Line3 = $6B00
!SP2Line4 = $6C00
!SP2Line5 = $6D00
!SP2Line6 = $6E00
!SP2Line7 = $6F00

!SP3Line0 = $7000
!SP3Line1 = $7100
!SP3Line2 = $7200
!SP3Line3 = $7300
!SP3Line4 = $7400
!SP3Line5 = $7500
!SP3Line6 = $7600
!SP3Line7 = $7700

!SP4Line0 = $7800
!SP4Line1 = $7900
!SP4Line2 = $7A00
!SP4Line3 = $7B00
!SP4Line4 = $7C00
!SP4Line5 = $7D00
!SP4Line6 = $7E00
!SP4Line7 = $7F00

!AN2Buffer1 = $7EC100
!AN2Buffer2 = $7EAD00

!GFX32Buffer = $7E2000
!PlayerPaletteColor2To5 = $00B280
!PlayerBasePalette = $00B2C8

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
!DynamicPoseSpaceConfig = read3(!Routines+$45)
!GameModeTable = !Routines+$48
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

if !DynamicPoses
!DynamicSpaceConfigSecondHalfSSP4 = $00
!DynamicSpaceConfigSP4 = $01
!DynamicSpaceConfigSecondHalfSP3 = $02
!DynamicSpaceConfigFirstHalfSP3 = $03
!DynamicSpaceConfigSP3 = $04
!DynamicSpaceConfigSecondHalfSP2 = $05
!DynamicSpaceConfigFirstHalfSP2 = $06
!DynamicSpaceConfigSP2 = $07
!DynamicSpaceConfigSecondHalfSP1 = $08
!DynamicSpaceConfigFirstHalfSP1 = $09
!DynamicSpaceConfigSP1 = $0A
!DynamicSpaceConfigFirstHalfSP1WithPlayer = $0B
!DynamicSpaceConfigSP11WithPlayer = $0C
!DynamicSpaceConfigSecondHalfSP3ndSP4 = $0D
!DynamicSpaceConfigSP34 = $0E
!DynamicSpaceConfigSecondHalfSP2andSP34 = $0F
!DynamicSpaceConfigSP234 = $10
!DynamicSpaceConfigSecondHalfSP1andSP234 = $11
!DynamicSpaceConfigSP1234 = $12
!DynamicSpaceConfigSecondHalfSP1andSP234WithPlayer = $13
!DynamicSpaceConfigSP1234WithPlayer = $14
!DynamicSpaceConfigSecondHalfSP3ndSP4WithDSX = $15
!DynamicSpaceConfigSP34WithDSX = $16
!DynamicSpaceConfigSecondHalfSP2andSP34WithDSX = $17
!DynamicSpaceConfigSP234WithDSX = $18
!DynamicSpaceConfigSecondHalfSP1andSP234WithDSX = $19
!DynamicSpaceConfigSP1234WithDSX = $1A
!DynamicSpaceConfigSecondHalfSP1andSP234WithPlayerAndDSX = $1B
!DynamicSpaceConfigSP1234WithPlayerAndDSX = $1C
endif