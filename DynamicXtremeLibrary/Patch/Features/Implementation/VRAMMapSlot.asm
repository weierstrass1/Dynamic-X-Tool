;public bool IsRestricted { get => (Offset & 0x80) == 0x80; }
;Input:
;   x: Index of VRAMMapSlot
;Output: 
;   Z clear is it is not restricted, Z set if it is restricted
;   A = VRAMMapSlot offset
macro VRAMMapSlot_IsRestricted()
    LDA.l DX_Dynamic_Tile_Offset,x
    BIT #$80                        ;get => (Offset & 0x80) == 0x80;
endmacro
;public bool IsFree { get => (SizeOrPose & 0x80) == 0x80; }
;Input:
;   x: Index of VRAMMapSlot
;Output: 
;   Z clear is it is not free, Z set if it is free
;   A = VRAMMapSlot Size Or Pose
macro VRAMMapSlot_IsFree()
    LDA.l DX_Dynamic_Tile_Size,x
    BIT #$80                        ;get => (SizeOrPose & 0x80) == 0x80;
endmacro
;public byte GetSize(DynamicPoseDataBase poseDataBase, DynamicPoseHashmap hashmap)
;{
;    if (IsFree)
;        return (byte)((SizeOrPose & 0x7F) + 1);
;    DynamicPoseHashMapSlot poseslot = hashmap.Get(SizeOrPose);
;    return poseDataBase.Get(poseslot.ID).blocks16x16;
;}
;public byte GetScore(ushort TimeSpan, DynamicPoseHashmap Hashmap)
;{
;    if (IsFree)
;        return 0xFF;
;    return (byte)Math.Min(TimeSpan - Hashmap.Get(SizeOrPose).TimeLastUse, 0xFE);
;}
;This does 2 in 1 to save some cycles
;Input:
;   x: Index of VRAMMapSlot
;Output:
;   A is score
;   VRAMMapSlot_Size is size
VRAMMapSlot_GetSizeAndScore:
    LDA #$00
    XBA
    %VRAMMapSlot_IsFree()       ;if (IsFree)
    BEQ .IsNotFree

    AND #$7F
    INC A
    STA.b VRAMMapSlot_Size      ;return (byte)((SizeOrPose & 0x7F) + 1);
    LDA #$FF                    ;return 0xFF;
%ReturnLongShortDBG()          

.IsNotFree

    PHX
    REP #$30
    ASL
    TAX

    LDA.l DX_Dynamic_Pose_ID,X  ;DynamicPoseHashMapSlot poseslot = hashmap.Get(SizeOrPose);
    TAY

    SEP #$20
    LDA.w Pose16x16Blocks,y
    STA.b VRAMMapSlot_Size      ;return poseDataBase.Get(poseslot.ID).blocks16x16;

    REP #$20
    LDA DX_Timer
    SEC
    SBC DX_Dynamic_Pose_TimeLastUse,x
    CMP #$00FF
    SEP #$30
    BCC .NoCap
    LDA #$FE
.NoCap                          ;return (byte)Math.Min(TimeSpan - Hashmap.Get(SizeOrPose).TimeLastUse, 0xFE);
    PLX
%ReturnLongShortDBG()
;public byte GetSize(DynamicPoseDataBase poseDataBase, DynamicPoseHashmap hashmap)
;{
;    if (IsFree)
;        return (byte)((SizeOrPose & 0x7F) + 1);
;    DynamicPoseHashMapSlot poseslot = hashmap.Get(SizeOrPose);
;    return poseDataBase.Get(poseslot.ID).blocks16x16;
;}
;This do 2 in 1 to save some cycles
;Input:
;   x: Index of VRAMMapSlot
;Output:
;   A is size
macro VRAMMapSlot_GetSize()
?GetSize:
    LDA #$00
    XBA
    %VRAMMapSlot_IsFree()       ;if (IsFree)
    BEQ ?.IsNotFree

    AND #$7F
    INC A
    BRA ?.finish                ;return (byte)((SizeOrPose & 0x7F) + 1);

?.IsNotFree

    PHX
    REP #$30
    ASL
    TAX

    LDA.l DX_Dynamic_Pose_ID,X  ;DynamicPoseHashMapSlot poseslot = hashmap.Get(SizeOrPose);
    TAY
    SEP #$20

    LDA.w Pose16x16Blocks,y     ;return poseDataBase.Get(poseslot.ID).blocks16x16;
    SEP #$10
    PLX
?.finish
endmacro
