    !sa1 = 0
    !dp = $0000
    !addr = $0000
    !rom = $800000
    !Variables = $7F0B44 
    !Variables2 = $7F9C7B
    !MultiplicationResult = $4216
    !DivisionResult = $4214
    !RemainderResult = $4216
    !MaxSprites = $0C

if read1($00FFD5) == $23
    !sa1 = 1
    !dp = $3000
    !addr = $6000
    !rom = $000000
    !Variables = $418000  
    !Variables2 = $418B80
    !MultiplicationResult = $2306
    !DivisionResult = $2306
    !RemainderResult = $2308
    !MaxSprites = $16
endif

!VRAMTransferSize = 256
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
            namespace Palettes
                ID: skip 16
                Updated: skip 8
                DisableTimer: skip 8
            namespace off
        namespace off
        FreeRams:
org !Variables2
        namespace PPU
            namespace CGRAM
                namespace Transfer
                    Length: skip 1          
                    SourceLength: skip (!CGRAMTransferSize*2)  
                    Offset: skip !CGRAMTransferSize         
                    Source: skip (!CGRAMTransferSize*2)         
                    SourceBNK: skip (!CGRAMTransferSize*2)      
                namespace off
                LastPlayerPal: skip 2  
                namespace BufferTransfer
                    Length: skip 1          
                    SourceLength: skip !CGRAMBufferSize   
                    Offset: skip !CGRAMBufferSize         
                    Destination: skip (!CGRAMBufferSize*2)   
                    DestinationBNK: skip (!CGRAMBufferSize*2)
                namespace off
                PaletteCopy: skip 512
                BasePalette: skip 768       
                PaletteWriteMirror: skip 512
            namespace off
            namespace VRAM
                namespace Transfer
                    Length: skip 1          
                    SourceLength: skip !VRAMTransferSize
                    Offset: skip !VRAMTransferSize      
                    Source: skip !VRAMTransferSize      
                    SourceBNK: skip !VRAMTransferSize   
                namespace off
            namespace off
        namespace off
        FreeRams2:
    namespace off
namespace nested off
pullpc

!Routines #= ((read1($0082DE)<<16)+read2($008241))|!rom

!PoseWasLoaded = read3(!Routines+$00)
!TakeDynamicRequest = read3(!Routines+$03)
!Draw = read3(!Routines+$06)
!IsValid = read3(!Routines+$09)
!RemapOamTile = read3(!Routines+$0C)
!XIsValid = read3(!Routines+$0F)
!YIsValid = read3(!Routines+$12)
!Draw_Return = read3(!Routines+$15)
!ResourceTable = read3(!Routines+$18)
!GraphicRoutinesTable = read3(!Routines+$1B)
!AssignPalette = read3(!Routines+$1E)
!SetHSLBase = read3(!Routines+$21)
!SetRGBBase = read3(!Routines+$24)
!MixHSL = read3(!Routines+$27)
!MixRGB = read3(!Routines+$2A)
!SetPropertyAndOffset = read3(!Routines+$2D)

incsrc "./Macros/STDCall.asm"
incsrc "./Macros/MultAndDiv.asm"
incsrc "./Macros/DynamicSpritesMacros.asm"
incsrc "./Macros/Palettes.asm"
incsrc "./Macros/PPU.asm"
incsrc "./Macros/GraphicRoutinesMacros.asm"