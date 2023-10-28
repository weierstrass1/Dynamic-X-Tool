if read1($00FFD5) == $23
    fullsa1rom
else
    lorom
endif

incsrc "../DynamicX/ExtraDefines/DynamicXDefines.asm"

!OffsetBetweenSameHash = $000F

org !Routines+$00
    dl $000000
    dl TakeDynamicRequest|!rom

org !Routines+$2D
    dl SetPropertyAndOffset|!rom

org !Routines+$45
    dl DynamicPoseSpaceConfig|!rom

reset freespaceuse
freecode cleaned

Start:

;Defines
!INCREASE_PER_STEP = 15
!HASHMAP_SIZE = 128 ;tiene que ser factor de 2
!VRAMMAP_SIZE = 128 ;vrammap size
!DEBUG = 0

;Macros
macro ReturnLongShortDBG()
    if !DEBUG != 0
        RTL
    else
        RTS
    endif
endmacro

macro CallFunctionLongShortDBG(func)
    if !DEBUG != 0
        JSL <func>
    else
        JSR <func>
    endif
endmacro

pushpc : org !dp ;$0000 (S-CPU) o $3000 (SA-1). Se podria usar un namespace para evitar variables duplicadas
    HashCodeBackup: skip 1            ;$00
    HashIndexBackup: skip 1            ;$01
    PoseIDBackup: skip 2            ;$02
    HashSizeBackup: skip 1            ;$04
    VRAMMapCurrentSpace:
        .Offset: skip 1                ;$05
        .Score: skip 1                ;$06
        .Size: skip 1                ;$07
    VRAMMapBestSpace:
        .Offset: skip 1                ;$08
        .Score: skip 1                ;$09
        .Size: skip 1                ;$0A
    VRAMMapTMP_Size: skip 1            ;$0B
    VRAMMapSlot_Size: skip 1        ;$0C
    VRAMMapSlot_Score: skip 1        ;$0D
    VRAMMapLoop: skip 1                ;$0E
    VRAMMap_Adjacent: skip 1        ;$0F
pullpc

incsrc  "./VRAMMapSlot.asm"
incsrc  "./HashMap.asm"
incsrc  "./VRAMMap.asm"
incsrc  "./DynamicXSystem.asm"
incsrc "../DynamicX/Data/DynamicPoseData.asm"


DynamicPoseSpaceConfig:
    ASL
    TAX

    PHB
    PHK
    PLB
    JSR (.Configs,x)
    PLB
RTL

.Configs
    dw ..SecondHalfSSP4
    dw ..SP4
    dw ..SecondHalfSP3
    dw ..FirstHalfSP3
    dw ..SP3
    dw ..SecondHalfSP2
    dw ..FirstHalfSP2
    dw ..SP2
    dw ..SecondHalfSP1
    dw ..FirstHalfSP1
    dw ..SP1
    dw ..FirstHalfSP1WithPlayer
    dw ..SP11WithPlayer
    dw ..SecondHalfSP3ndSP4
    dw ..SP34
    dw ..SecondHalfSP2andSP34
    dw ..SP234
    dw ..SecondHalfSP1andSP234
    dw ..SP1234
    dw ..SecondHalfSP1andSP234WithPlayer
    dw ..SP1234WithPlayer
    dw ..SecondHalfSP3ndSP4WithDSX
    dw ..SP34WithDSX
    dw ..SecondHalfSP2andSP34WithDSX
    dw ..SP234WithDSX
    dw ..SecondHalfSP1andSP234WithDSX
    dw ..SP1234WithDSX
    dw ..SecondHalfSP1andSP234WithPlayerAndDSX
    dw ..SP1234WithPlayerAndDSX

macro zone(init,end,restricted)
    LDA #(<end>-<init>)|$80
    STA DX_Dynamic_Tile_Size+<init>
    STA DX_Dynamic_Tile_Size+<end>
    LDA #<init>|<restricted>
    STA DX_Dynamic_Tile_Offset+<init>
    STA DX_Dynamic_Tile_Offset+<end>
endmacro

..SecondHalfSSP4
    %zone($00,$6F,$80)
    %zone($70,$7F,$00)
RTS

..SP4
    %zone($00,$5F,$80)
    %zone($60,$7F,$00)
RTS

..SecondHalfSP3
    %zone($00,$4F,$80)
    %zone($50,$5F,$00)
    %zone($60,$7F,$80)
RTS

..FirstHalfSP3
    %zone($00,$3F,$80)
    %zone($40,$4F,$00)
    %zone($50,$7F,$80)
RTS

..SP3
    %zone($00,$3F,$80)
    %zone($40,$5F,$00)
    %zone($60,$7F,$80)
RTS

..SecondHalfSP2
    %zone($00,$2F,$80)
    %zone($30,$3F,$00)
    %zone($40,$7F,$80)
RTS

..FirstHalfSP2
    %zone($00,$1F,$80)
    %zone($20,$2F,$00)
    %zone($30,$7F,$80)
RTS

..SP2
    %zone($00,$1F,$80)
    %zone($20,$3F,$00)
    %zone($40,$7F,$80)
RTS

..SecondHalfSP1
    %zone($00,$0F,$80)
    %zone($10,$1F,$00)
    %zone($20,$7F,$80)
RTS

..FirstHalfSP1
    %zone($00,$0F,$00)
    %zone($10,$7F,$80)
RTS

..SP1
    %zone($00,$1F,$00)
    %zone($20,$7F,$80)
RTS

..FirstHalfSP1WithPlayer
    %zone($00,$04,$80)
    %zone($05,$0F,$00)
    %zone($10,$7F,$80)
RTS

..SP11WithPlayer
    %zone($00,$04,$80)
    %zone($05,$14,$00)
    %zone($15,$17,$80)
    %zone($18,$1E,$00)
    %zone($1F,$7F,$80)
RTS

..SecondHalfSP3ndSP4
    %zone($00,$4F,$80)
    %zone($50,$7F,$00)
RTS

..SP34
    %zone($00,$3F,$80)
    %zone($40,$7F,$00)
RTS

..SecondHalfSP2andSP34
    %zone($00,$2F,$80)
    %zone($30,$7F,$00)
RTS

..SP234
    %zone($00,$1F,$80)
    %zone($20,$7F,$00)
RTS

..SecondHalfSP1andSP234
    %zone($00,$0F,$80)
    %zone($10,$7F,$00)
RTS

..SP1234
    %zone($00,$7F,$00)
RTS

..SecondHalfSP1andSP234WithPlayer
    %zone($00,$0F,$80)
    %zone($10,$14,$00)
    %zone($15,$17,$80)
    %zone($18,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$7F,$00)
RTS

..SP1234WithPlayer
    %zone($00,$04,$80)
    %zone($05,$14,$00)
    %zone($15,$17,$80)
    %zone($18,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$7F,$00)
RTS

..SecondHalfSP3ndSP4WithDSX
    %zone($00,$4F,$80)
    %zone($50,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP34WithDSX
    %zone($00,$3F,$80)
    %zone($40,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SecondHalfSP2andSP34WithDSX
    %zone($00,$2F,$80)
    %zone($30,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP234WithDSX
    %zone($00,$1F,$80)
    %zone($20,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SecondHalfSP1andSP234WithDSX
    %zone($00,$0F,$80)
    %zone($10,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP1234WithDSX
    %zone($00,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SecondHalfSP1andSP234WithPlayerAndDSX
    %zone($00,$0F,$80)
    %zone($10,$14,$00)
    %zone($15,$17,$80)
    %zone($18,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$6F,$00)
    %zone($70,$7F,$80)
RTS

..SP1234WithPlayerAndDSX
    %zone($00,$04,$80)
    %zone($05,$14,$00)
    %zone($15,$17,$80)
    %zone($18,$1E,$00)
    %zone($1F,$1F,$80)
    %zone($20,$6F,$00)
    %zone($70,$7F,$80)
RTS


End:

print dec(snestopc(Start))
print freespaceuse