if read1($00FFD5) == $23
    sa1rom
endif

incsrc "../DynamicX/DynamicXDefines.asm"

!OffsetBetweenSameHash = $000F

org !Routines+$00
    dl PoseWasLoaded|!rom
    dl TakeDynamicRequest|!rom

org !Routines+$2D
    dl SetPropertyAndOffset|!rom

reset freespaceuse
freecode cleaned

Start:
;This routine creates a request to 
;Load a Dynamic Pose in VRAM.
;Input:
;   Y = Pose ID (16 bits)
;Output:
;   Carry Set if the request succeed.
;   Carry Clear if the request failed.
TakeDynamicRequest:

    LDA #$00
    XBA

    JSR FindPose
    BCC .NotFound

    SEP #$30
    TXA
    LSR
    TAX

    JSL SetPropertyAndOffset
    SEC
RTL
.NotFound
    STX $45

    PHB
    PHK
    PLB

    REP #$20
    PHY
    TYA
    ASL
    TAY

    ;If it is not possible to load more data in vram
    ;during this frame then return carry clear
    LDA PoseSize,y
    PLY
    CLC
    ADC DX_Dynamic_CurrentDataSend
    CMP DX_Dynamic_MaxDataPerFrame
    SEP #$20
    BEQ .NoDataSizeRestriction
    BCC .NoDataSizeRestriction
.MaxDataPerFrameSurpassed   
    SEP #$30
    PLB
    CLC
RTL
.NoDataSizeRestriction

    JSR FindSpace
    BCS +

    PLB
    CLC
RTL

+
    REP #$20
    TYA
    PHY
    ASL
    TAY
    LDA PoseSize,y
    PLY
    CLC
    ADC DX_Dynamic_CurrentDataSend
    STA DX_Dynamic_CurrentDataSend
    SEP #$20

    JSR ClearSlot

    JSR UpdateSpace

    LDX $45
    JSR GetSlotPosition
    JSR UpdateSlot

    JSR DynamicRoutine

    LDX $45

    SEP #$30

    JSL SetPropertyAndOffset

    PLB
    SEC
RTL

;Check if a pose is already loaded.
;
;Input:
;   A must be 8 bits
;   Y must be 16 bits
;   Y = Pose ID (16 bits)
;Output:
;   Carry Set if the pose was found, clear if not.
FindPose:
    ;If hashmap is empty then return carry clear
    LDA DX_Dynamic_Pose_Length
    BNE .NotEmpty
    TYA
    AND #$7F
    TAX
    CLC
RTS
.NotEmpty
    LDA #$00
    XBA
    TYA
    AND #$7F
    TAX

    ;If Hash Value wasn't found in the hashmap then return carry clear
    LDA DX_Dynamic_Pose_HashSize,x
    BNE .HashValueFound
    CLC
RTS
.HashValueFound
    STY $0000|!dp
    REP #$20
    STA $02
    LDA $00
    AND #$007F
    STA $04

    ;$00 = Pose ID
    ;$02 = Number of values with the same hash value, it is used to limit the loop
    ;$04 = Hash Value

    ;Starts checking current position
    BRA .cmp
.loop
    ;add F to the position
    TXA
    LSR
    CLC
    ADC #!OffsetBetweenSameHash
    AND #$007F
    ASL
    TAX
.cmp
    ;If the current position has the requested pose
    ;return carry set
    LDA DX_Dynamic_Pose_ID,x
    CMP $00
    BNE .next

    SEP #$20
    SEC
RTS
.next
    ;If the current position use a different hash value
    ;then skip decrease $02 and back to loop.
    AND #$007F
    CMP $04
    BNE .loop
    
    ;If the current position use the same hash map then
    ;decrease $02, if after this $02 is zero then exit loop
    ;returning carry clear
    DEC $02
    BNE .loop

    SEP #$20
    CLC
RTS

PoseWasLoaded:
    PHB
    PHK
    PLB

    LDA #$00
    XBA

    JSR FindPose
    BCC .NotFound

    PLB
    SEC
RTL
.NotFound
    PLB
    CLC
RTL

!Property = $04
!PoseOffset = $08

SetPropertyAndOffset:
    PHB
    PHK
    PLB
    LDA DX_Dynamic_Pose_Offset,x
    BIT #$40
    BNE +

    LDY #$00
    BRA ++
+
    LDY #$01
++
    STY !Property|!dp

    TAY
    LDA VRAMDisp,y
    STA !PoseOffset

    PHX
    TXA
    ASL
    TAX
    
    LDA DX_Timer
    STA DX_Dynamic_Pose_TimeLastUse,x
    LDA DX_Timer+1
    STA DX_Dynamic_Pose_TimeLastUse+1,x
    PLX
    PLB
RTL

VRAMDisp:
    db $00,$02,$04,$06,$08,$0A,$0C,$0E
    db $20,$22,$24,$26,$28,$2A,$2C,$2E
    db $40,$42,$44,$46,$48,$4A,$4C,$4E
    db $60,$62,$64,$66,$68,$6A,$6C,$6E
    db $80,$82,$84,$86,$88,$8A,$8C,$8E
    db $A0,$A2,$A4,$A6,$A8,$AA,$AC,$AE
    db $C0,$C2,$C4,$C6,$C8,$CA,$CC,$CE
    db $E0,$E2,$E4,$E6,$E8,$EA,$EC,$EE
    db $00,$02,$04,$06,$08,$0A,$0C,$0E
    db $20,$22,$24,$26,$28,$2A,$2C,$2E
    db $40,$42,$44,$46,$48,$4A,$4C,$4E
    db $60,$62,$64,$66,$68,$6A,$6C,$6E
    db $80,$82,$84,$86,$88,$8A,$8C,$8E
    db $A0,$A2,$A4,$A6,$A8,$AA,$AC,$AE
    db $C0,$C2,$C4,$C6,$C8,$CA,$CC,$CE
    db $E0,$E2,$E4,$E6,$E8,$EA,$EC,$EE

;Input:
;   Y = Pose ID (16 bits)
FindSpace:

    LDA #$00
    XBA
    LDX #$FFFF
    STX $08
    STX $0C
    LDX #$0000
    STX $0A
    ;$08 = Size of Current Space
    ;$09 = X Offset of Current Space
    ;$0A = Score of Current Space

    ;$0B = Score of Best Space
    ;$0C = Size of Best Space
    ;$0D = X Offset of Best Space

    ;$0E = Is Free Space
    ;$0F = Is Adjacent
    STZ $0F

    ;Starts at the end of the vram
    LDX #$007F
.loop
    JSR ProcessSlot
    BCC +
    JSR ProcessSpace
    BCC +

    ;If current best is the same size of the space and is a free space then
    ;return
    LDA $0C
    CMP Pose16x16Blocks,y
    BNE +

    LDA $0B
    CMP #$FF
    BNE +

    SEC
RTS

+
    DEX
    BPL .loop

    LDA $0B
    BNE +
    CLC
RTS
+
    SEC
RTS

;Determine the Score, Size and Offset of current slot
ProcessSlot:

    LDA DX_Dynamic_Tile_Size,x
    BPL .NotFreeSpace
.FreeSpace

    ;If it is negative then is restricted (can't be used)
    LDA DX_Dynamic_Tile_Offset,x
    BPL ..NotRestricted
..Restricted
    ;If it is restricted then just skip it
    AND #$7F
    TAX

    STZ $0F
    CLC
RTS
..NotRestricted

    ;Here check size and separate in 3 cases
    ;Less space than necessary
    ;More space than necessary
    ;Equal space
    LDA DX_Dynamic_Tile_Size,x
    AND #$7F
    INC A
    CMP Pose16x16Blocks,y
    BCS ..BiggerSpace
..SmallerSpace
    STA $04

    LDA $0F
    BNE ..Adjacent
..NotAdjacent

    LDA #$01
    STA $0F

    LDA $04
    BRA ..BiggerSpace

..Adjacent

    LDA $04
    CLC
    ADC $08
    STA $08

    BRA ..Finish

..BiggerSpace

    STA $08

    LDA #$FF
    STA $0A

..Finish

    LDA DX_Dynamic_Tile_Offset,x
    AND #$7F
    TAX
    STA $09
    SEC
RTS

.NotFreeSpace
    PHX
    LDA DX_Dynamic_Tile_Pose,x
    ASL
    TAX

    REP #$20
    ;If this space can't be replaced then skip it
    LDA DX_Timer
    SEC 
    SBC DX_Dynamic_Pose_TimeLastUse,x
    CMP #$0002
    BCS +

-
    PLX
    SEP #$20
    LDA DX_Dynamic_Tile_Offset,x
    TAX

    STZ $08

    STZ $0F
    CLC
RTS
+
    CMP #$0100
    BCC +
    LDA #$FFFF
+
    ;Calculates Min(FF,Timer-TimeLastUse) and use it as Score
    SEP #$20
    ;Compares if the score is higher or equal than old score
    ;If it is lower then skip this space
    CMP $0B
    BCC -
    STA $05

    REP #$20
    LDA DX_Dynamic_Pose_ID,x
    TAX
    SEP #$20
    ;Compare the space of the block with the space of the requested pose
    LDA Pose16x16Blocks,x
    PLX
    CMP Pose16x16Blocks,y
    BCS ..BiggerSpace
..SmallerSpace
    STA $04

    LDA $0F
    BNE ..Adjacent
..NotAdjacent

    LDA #$01
    STA $0F

    LDA $04
    BRA ..BiggerSpace

..Adjacent

    LDA $04
    CLC
    ADC $08
    STA $08

    LDA $05
    CMP $0A
    BCC ..Finish
    BRA ..Finish2

..BiggerSpace
    STA $08

    LDA $05
..Finish
    STA $0A
..Finish2

    LDA DX_Dynamic_Tile_Offset,x
    TAX
    STA $09
    SEC
RTS

;Determines if the current space can be the best space
ProcessSpace:

    LDA $08
    CMP Pose16x16Blocks,y
    BCS .IsACandidate   ;If $08 is less than Pose Space then skip
    CLC
RTS
.IsACandidate

    STZ $0F

    LDA $0A
    CMP $0B
    BCS .IsBestScore    ;If score is lower than best score then skip
RTS
.IsBestScore 
    BNE .IsBestOption   ;If has the same score analize the size of the space

    LDA $08
    CMP $0C
    BCC .IsBestOption   ;If space is more or equal than current best then skip
    CLC
RTS

.IsBestOption

    LDA $08
    STA $0C

    LDA $0A
    STA $0B

    LDA $09
    STA $0D

    SEC
RTS

ClearSlot:
    STZ $0E
    LDX $0D
    STZ $08
    LDA #$00
    XBA

.Loop
    LDA DX_Dynamic_Tile_Size,x
    BPL .NotFreeSpace
.FreeSpace
    AND #$7F
    INC A
    CLC
    ADC $08
    STA $08
    CMP $0C
    BCC .next
RTS
.NotFreeSpace
    PHX
    REP #$20
    PHA
    ASL
    TAX
    PHX
    LDA DX_Dynamic_Pose_ID,x
    AND #$007F
    TAX

    SEP #$20
    LDA DX_Dynamic_Pose_HashSize,x
    DEC A
    STA DX_Dynamic_Pose_HashSize,x
    PLX

    LDA #$FF
    STA DX_Dynamic_Pose_ID,x
    STA DX_Dynamic_Pose_ID+1,x

    LDA DX_Dynamic_Pose_Length
    DEC A
    STA DX_Dynamic_Pose_Length

    PLX
    ;Compare the space of the block with the space of the requested pose
    LDA Pose16x16Blocks,x
    PLX
    STA $04

    LDA $04
    CLC
    ADC $08
    STA $08
    CMP $0C
    BCC .next
RTS

.next
    LDA $0D
    CLC
    ADC $08
    TAX
    BRA .Loop

GetSlotPosition:
    REP #$20
    TXA
    ASL
    TAX
.Loop
    LDA DX_Dynamic_Pose_ID,x
    CMP #$FFFF
    BEQ .leave

    TXA
    CLC
    ADC #!OffsetBetweenSameHash
    TAX
    BRA .Loop
.leave
    SEP #$20
RTS

UpdateSpace:
    STZ $0E

    LDA #$00
    XBA
    LDA $0D
    TAX

    LDA $45
    STA DX_Dynamic_Tile_Pose,x

    LDA $0D
    STA DX_Dynamic_Tile_Offset,x

    TXA
    CLC
    ADC Pose16x16Blocks,y
    DEC A
    TAX

    LDA $45
    STA DX_Dynamic_Tile_Pose,x

    LDA $0D
    STA DX_Dynamic_Tile_Offset,x

    LDA $0C
    CMP Pose16x16Blocks,y
    BNE +
RTS
+
    INX
    TXA
    STA $05

    LDA $0C
    SEC
    SBC Pose16x16Blocks,y
    STA $04
    DEC A
    ORA #$80
    STA DX_Dynamic_Tile_Size,x

    LDA $05
    STA DX_Dynamic_Tile_Offset,x

    TXA
    CLC
    ADC $04
    TAX
    DEX

    LDA $04
    DEC A
    ORA #$80
    STA DX_Dynamic_Tile_Size,x

    LDA $05
    STA DX_Dynamic_Tile_Offset,x
RTS

UpdateSlot:
    REP #$20
    TYA
    AND #$007F
    TAX
    SEP #$20

    LDA DX_Dynamic_Pose_Length
    INC A
    STA DX_Dynamic_Pose_Length

    LDA DX_Dynamic_Pose_HashSize,x
    INC A
    STA DX_Dynamic_Pose_HashSize,x

    LDX $45

    STZ $0E

    REP #$20
    TXA
    ASL
    TAX

    TYA
    STA DX_Dynamic_Pose_ID,x

    LDA DX_Timer
    STA DX_Dynamic_Pose_TimeLastUse,x

    SEP #$20
    LDX $45

    LDA $0D
    STA DX_Dynamic_Pose_Offset,x
RTS

!baseSourseOffset = $47
!baseSourceOffsetBNK = $49
!doubledID = $4A
!SourceOffset = $4C
!SourceSize = $4E
!VRAMOffset = $50

!CurrentVRAMOffset = $00
!CurrentSourceOffset = $02
!CurrentSecondLineOffset = $04
!CurrentLine1Size = $06
!CurrentLine2Size = $08
!SentData = $0A

OffsetToVRAMOffset:
    dw $0000,$0020,$0040,$0060,$0080,$00A0,$00C0,$00E0
    dw $0200,$0220,$0240,$0260,$0280,$02A0,$02C0,$02E0
    dw $0400,$0420,$0440,$0460,$0480,$04A0,$04C0,$04E0
    dw $0600,$0620,$0640,$0660,$0680,$06A0,$06C0,$06E0
    dw $0800,$0820,$0840,$0860,$0880,$08A0,$08C0,$08E0
    dw $0A00,$0A20,$0A40,$0A60,$0A80,$0AA0,$0AC0,$0AE0
    dw $0C00,$0C20,$0C40,$0C60,$0C80,$0CA0,$0CC0,$0CE0
    dw $0E00,$0E20,$0E40,$0E60,$0E80,$0EA0,$0EC0,$0EE0
    dw $1000,$1020,$1040,$1060,$1080,$10A0,$10C0,$10E0
    dw $1200,$1220,$1240,$1260,$1280,$12A0,$12C0,$12E0
    dw $1400,$1420,$1440,$1460,$1480,$14A0,$14C0,$14E0
    dw $1600,$1620,$1640,$1660,$1680,$16A0,$16C0,$16E0
    dw $1800,$1820,$1840,$1860,$1880,$18A0,$18C0,$18E0
    dw $1A00,$1A20,$1A40,$1A60,$1A80,$1AA0,$1AC0,$1AE0
    dw $1C00,$1C20,$1C40,$1C60,$1C80,$1CA0,$1CC0,$1CE0
    dw $1E00,$1E20,$1E40,$1E60,$1E80,$1EA0,$1EC0,$1EE0
    

DynamicRoutine:
    LDX $45
    
    PHY

    LDA #$00
    XBA
    LDA DX_Dynamic_Pose_Offset,x
    REP #$20
    ASL
    TAY
    LDA OffsetToVRAMOffset,y
    STA !VRAMOffset

    PLA
    STA $04
    ASL
    STA !doubledID
    CLC
    ADC $04
    TAY

    LDA PoseResource,y
    STA !baseSourseOffset
    LDA PoseResource+1,y
    STA !baseSourseOffset+1

    LDA !doubledID
    ASL
    STA !doubledID
    TAY

    LDA #$0000
    STA !SourceOffset

    LDA PoseResourceSizePerLine,y
    STA !SourceSize

    SEP #$20

    JSR DynamicRoutineLine

    LDY !doubledID|!dp

    LDA PoseResourceSizePerLine+2,y
    BNE +
    SEP #$30
RTS
+
    STA !SourceSize

    LDA PoseResourceSizePerLine,y
    STA !SourceOffset

    LDA !doubledID
    LSR
    TAY

    LDA !VRAMOffset
    CLC
    ADC PoseSecondLineOffset,y
    STA !VRAMOffset

    JSR DynamicRoutineLine
    SEP #$30
RTS

DynamicRoutineLine:
    REP #$20
    LDA #$0000
    STA !SentData

    LDA !VRAMOffset
    CLC
    ADC #$6000
    STA !CurrentVRAMOffset
    CLC
    ADC #$0100
    AND #$FF00
    STA !CurrentSecondLineOffset
    SEC
    SBC !CurrentVRAMOffset
    ASL
    STA !CurrentLine1Size

    LDA !CurrentSecondLineOffset
    CLC
    ADC #$0100
    STA !CurrentSecondLineOffset

    LDA #$0200
    SEC
    SBC !CurrentLine1Size
    STA !CurrentLine2Size

    LDA !SourceOffset
    CLC
    ADC !baseSourseOffset                   ;\
    STA !CurrentSourceOffset

    LDA !CurrentVRAMOffset
    AND #$00FF
    BNE .Loop
    LDA !SourceSize
    CMP #$0200
    BCS .EndLine

.Loop
    LDA !SourceSize
    SEC
    SBC !SentData
    CMP !CurrentLine1Size
    BCC .EndLine
    BEQ .EndLine
    CMP #$0200
    BCC .TwoLinesWithEndLineReDir
    BEQ .TwoLinesWithEndLineReDir
.TwoLines
    SEP #$20
    LDA #$00
    XBA
    LDA DX_PPU_VRAM_Transfer_Length   ;\
    ASL                         ;|
    TAX                         ;/X = number of transfer*2
    LSR
    INC A
    INC A                       ;|
    STA DX_PPU_VRAM_Transfer_Length   ;|Number of transfer ++

    LDA !baseSourceOffsetBNK                        ;\
    STA DX_PPU_VRAM_Transfer_SourceBNK,x            ;|BNK (low byte) = source bnk
    STA DX_PPU_VRAM_Transfer_SourceBNK+$02,x        ;|BNK (low byte) = source bnk
    LDA #$00                                        ;|BNK (high byte) = 0
    STA DX_PPU_VRAM_Transfer_SourceBNK+$01,x        ;/
    STA DX_PPU_VRAM_Transfer_SourceBNK+$03,x        ;/

    REP #$20
    LDA !CurrentVRAMOffset
    STA DX_PPU_VRAM_Transfer_Offset,x   ;/
    CLC
    ADC #$0100
    STA !CurrentVRAMOffset
    LDA !CurrentSecondLineOffset
    STA DX_PPU_VRAM_Transfer_Offset+2,x
    CLC
    ADC #$0100
    STA !CurrentSecondLineOffset

    LDA !CurrentLine1Size                    ;|MapLength = Size
    STA DX_PPU_VRAM_Transfer_SourceLength,x  ;/
    LDA !CurrentLine2Size
    STA DX_PPU_VRAM_Transfer_SourceLength+2,x  ;/
    
    LDA !CurrentSourceOffset
    STA DX_PPU_VRAM_Transfer_Source,x       ;/MapAddr = Addr
    CLC
    ADC !CurrentLine1Size
    STA DX_PPU_VRAM_Transfer_Source+2,x       ;/MapAddr = Addr
    CLC
    ADC !CurrentLine2Size
    STA !CurrentSourceOffset

    LDA !SentData
    CLC
    ADC #$0200
    STA !SentData
    BRA .Loop
.TwoLinesWithEndLineReDir
    BRA .TwoLinesWithEndLine
.EndLine
    STA !CurrentLine1Size
    SEP #$20
    LDA #$00
    XBA
    LDA DX_PPU_VRAM_Transfer_Length   ;\
    ASL                         ;|
    TAX                         ;/X = number of transfer*2
    LSR
    INC A                       ;|
    STA DX_PPU_VRAM_Transfer_Length   ;|Number of transfer ++

    LDA !baseSourceOffsetBNK                        ;\
    STA DX_PPU_VRAM_Transfer_SourceBNK,x            ;|BNK (low byte) = source bnk
    LDA #$00                                        ;|BNK (high byte) = 0
    STA DX_PPU_VRAM_Transfer_SourceBNK+$01,x        ;/

    REP #$20
    LDA !CurrentVRAMOffset
    STA DX_PPU_VRAM_Transfer_Offset,x   ;/

    LDA !CurrentLine1Size                    ;|MapLength = Size
    STA DX_PPU_VRAM_Transfer_SourceLength,x  ;/
    
    LDA !CurrentSourceOffset
    STA DX_PPU_VRAM_Transfer_Source,x       ;/MapAddr = Addr
RTS
.TwoLinesWithEndLine
    SEC 
    SBC !CurrentLine1Size
    STA !CurrentLine2Size
    SEP #$20
    LDA #$00
    XBA
    LDA DX_PPU_VRAM_Transfer_Length   ;\
    ASL                         ;|
    TAX                         ;/X = number of transfer*2
    LSR
    INC A
    INC A                       ;|
    STA DX_PPU_VRAM_Transfer_Length   ;|Number of transfer ++

    LDA !baseSourceOffsetBNK                        ;\
    STA DX_PPU_VRAM_Transfer_SourceBNK,x            ;|BNK (low byte) = source bnk
    STA DX_PPU_VRAM_Transfer_SourceBNK+$02,x        ;|BNK (low byte) = source bnk
    LDA #$00                                        ;|BNK (high byte) = 0
    STA DX_PPU_VRAM_Transfer_SourceBNK+$01,x        ;/
    STA DX_PPU_VRAM_Transfer_SourceBNK+$03,x        ;/

    REP #$20
    LDA !CurrentVRAMOffset
    STA DX_PPU_VRAM_Transfer_Offset,x   ;/
    LDA !CurrentSecondLineOffset
    STA DX_PPU_VRAM_Transfer_Offset+2,x

    LDA !CurrentLine1Size                    ;|MapLength = Size
    STA DX_PPU_VRAM_Transfer_SourceLength,x  ;/
    LDA !CurrentLine2Size
    STA DX_PPU_VRAM_Transfer_SourceLength+2,x  ;/
    
    LDA !CurrentSourceOffset
    STA DX_PPU_VRAM_Transfer_Source,x       ;/MapAddr = Addr
    CLC
    ADC !CurrentLine1Size
    STA DX_PPU_VRAM_Transfer_Source+2,x       ;/MapAddr = Addr
RTS

incsrc "../DynamicX/Data/DynamicPoseData.asm"

End:

print dec(snestopc(Start))
print freespaceuse