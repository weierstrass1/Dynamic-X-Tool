    !rom = $800000
    !dp = $0000
if read1($00FFD5) == $23
    sa1rom
    !rom = $000000
    !dp = $6000
endif

!Routines #= ((read1($0082DE)<<16)+read2($008241))|!rom

!XOffSet = $00
!YOffSet = $02
!Property = $04
!Tile = $05
!PoseID = $06
!PoseOffset = $08
!MaxTilePriority = $0A

!GraphicRoutine = $45
!Iterator = !GraphicRoutine+3
!maxtile_pointer = !Iterator+2

!maxtile_pointer_max        = $6180       ; 16 bytes
!maxtile_pointer_high       = $6190       ; 16 bytes
!maxtile_pointer_normal     = $61A0       ; 16 bytes
!maxtile_pointer_low        = $61B0       ; 16 bytes

!TileX = $0B
!TileY = $0D
!TileSize = $0F

org !Routines+$06
    dl Draw|!rom
org !Routines+$15
    dl Draw_Return|!rom

reset freespaceuse
freecode cleaned

Draw:
    STZ !MaxTilePriority+1
    REP #$30
    PHX
    LDA !PoseID
    ASL
    PHA
    CLC
    ADC !PoseID
    TAX

    LDA.l GraphicRoutine,x
    STA !GraphicRoutine
    LDA.l GraphicRoutine+1,x
    STA !GraphicRoutine+1

    LDX !MaxTilePriority
    LDA.l maxtilePointer,x
    AND #$00F0
    TAX
    LDA !maxtile_pointer_max,x
    STA !maxtile_pointer
    LDA !maxtile_pointer_max+2,x
    STA !maxtile_pointer+2
    LDA !maxtile_pointer_max+8,x
    STA !maxtile_pointer+4

    PLX
    LDA.l NumberOfTilesMinus1,x
    STA !Iterator
    CLC
    ADC.l TableOffset,x
    TAY
    SEP #$20
    
    JML [!GraphicRoutine|!dp]
.Return
    STZ $0B
    REP #$30

    LDX !MaxTilePriority
    LDA.l maxtilePointer,x
    AND #$00F0
    TAX
    LDA !maxtile_pointer
    STA !maxtile_pointer_max,x
    LDA !maxtile_pointer+2
    STA !maxtile_pointer_max+2,x

    PLX
    SEP #$30
RTL

maxtilePointer:
    db $00,$10,$20,$30

incsrc "../DynamicX/Data/PoseData.asm"

End:

print dec(snestopc(Draw))
print freespaceuse