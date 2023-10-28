;Implementacion de Hashmap
namespace VRAMMap
{
;public Space GetBestSlot(byte size, ushort TimeSpan)
;{
;    Space current = new();
;    Space best = new();
;    bool adjacent = false;
;    for (byte i = VRAMMAP_SIZE - 1; i < VRAMMAP_SIZE; i = (byte)(current.Offset - 1))
;    {
;        if (checkSpace(i, size, ref adjacent, current, TimeSpan))
;            checkIfCurrentIsBest(size, ref adjacent, current, best);
;    }
;    return best;
;}
;GetBestSlot
;AXY->8 bit
;Input: 
;   VRAMMapTMP_Size
;Devuelve el mejor espacio en X (8-bit)
GetBestSlot:
    LDX.b #!VRAMMAP_SIZE-1 ;byte i = VRAMMAP_SIZE - 1;
    LDY.b #$00
    STZ.b VRAMMap_Adjacent
    LDA #$FF
    STA.b VRAMMapBestSpace_Size
    STA.b VRAMMapBestSpace_Offset
    LDA #$00
    STA.b VRAMMapBestSpace_Score    ;Best starts with size, Offset #$FF and Score #$00

.loop
    %CallFunctionLongShortDBG(checkSpace)                   ;if (checkSpace(i, size, ref adjacent, current, TimeSpan))
    BCC +
        %CallFunctionLongShortDBG(checkIfCurrentIsBest)     ;   checkIfCurrentIsBest(size, ref adjacent, current, best);
    +
    LDA DX_Dynamic_Tile_Offset,X
    AND #$7F
    TAX : DEX                           ;i = (byte)(current.Offset - 1)
    BPL .loop                           ;i < VRAMMAP_SIZE;
%ReturnLongShortDBG()
;private bool checkSpace(byte i, byte size, ref bool adjacent, Space current, ushort TimeSpan)
;{
;    VRAMMapSlot slot = slots[i];
;    current.Offset = (byte)(slot.Offset & 0x7F);
;    if (slot.IsRestricted)
;    {
;        adjacent = false;
;        return false;
;    }
;    byte slotSize = slot.GetSize(PoseDataBase, Hashmap);
;    byte score = slot.GetScore(TimeSpan, Hashmap);
;    if(adjacent)
;    {
;        slotSize += current.Size;
;        score = Math.Min(score, current.Score);
;    }
;    current.Size = slotSize;
;    current.Score = score;
;    if(score < 2)
;    {
;        adjacent = false;
;        return false;
;    }
;    if (slotSize < size)
;        adjacent = true;
;    return true;
;}
;checkSpace(byte i, byte size, ref bool adjacent, Space current, ushort TimeSpan)
;X = current VRAMMapSlot
;VRAMMapCurrentSpace
;VRAMMapTMP_Size = Required Size
;VRAMMap_Adjacent
checkSpace:
    LDA.l DX_Dynamic_Tile_Offset,X : AND.b #$7F : STA.b VRAMMapCurrentSpace_Offset ;current.offset = slot.offset & 0x7F
    %VRAMMapSlot_IsRestricted()
    BEQ +                           ;if (slot.IsRestricted)
        STZ.b VRAMMap_Adjacent      ;   adjacent = false
        CLC
        %ReturnLongShortDBG()       ;   return false
    +

    %CallFunctionLongShortDBG(VRAMMapSlot_GetSizeAndScore)  ;byte slotSize = slot.GetSize(PoseDataBase, Hashmap);
    STA.b VRAMMapSlot_Score                                 ;score = slot.GetSize(PoseDataBase, Hashmap);

    ;if(adjacent)
    LDA.b VRAMMap_Adjacent : BEQ +
        ;slotSize += current.Size;
        LDA.b VRAMMapCurrentSpace_Size : CLC : ADC.b VRAMMapSlot_Size : STA.b VRAMMapSlot_Size
        ;Math.Min(score, current.Score);
        LDA.b VRAMMapSlot_Score : CMP.b VRAMMapCurrentSpace_Score : BCC + 
            LDA.b VRAMMapCurrentSpace_Score
            STA.b VRAMMapSlot_Score
    +

    LDA.b VRAMMapSlot_Size : STA.b VRAMMapCurrentSpace_Size     ;current.Size = slotSize
    LDA.b VRAMMapSlot_Score : STA.b VRAMMapCurrentSpace_Score   ;current.Score = score;
    
    CMP #$02 : BCS +                                    ; if(score < 2)
        STZ.b VRAMMap_Adjacent                          ;   adjacent = false
        CLC
        %ReturnLongShortDBG()                           ;   return false
    +
    LDA.b VRAMMapSlot_Size : CMP.b VRAMMapTMP_Size : BCS +    ;if (slotSize < size)
        INC.b VRAMMap_Adjacent              ;   adjacent = true;
    +
    SEC                                     ;return true
%ReturnLongShortDBG() ;return true
;private bool checkIfCurrentIsBest(byte size, ref bool adjacent, Space current, Space best)
;{
;    if (current.Size < size)
;        return false;
;    adjacent = false;
;    if (current.Score < best.Score)
;        return false;
;    if (current.Score == best.Score && current.Size >= best.Size)
;        return false;
;    best.Offset = current.Offset;
;    best.Size = current.Size;
;    best.Score = current.Score;
;    return true;
;}
checkIfCurrentIsBest:

    LDA.b VRAMMapCurrentSpace_Size
    CMP.b VRAMMapTMP_Size
    BCS +                           ;if (current.Size < size)
    CLC
%ReturnLongShortDBG()               ;return false;
+
    STZ.b VRAMMap_Adjacent          ;adjacent = false;
    LDA.b VRAMMapCurrentSpace_Score
    CMP.b VRAMMapBestSpace_Score
    BCS +                           ;if (current.Score < best.Score)
    CLC
%ReturnLongShortDBG()               ;return false;
+
    BNE ++                          ;if (current.Score == best.Score && current.Size >= best.Size)
    LDA.b VRAMMapCurrentSpace_Size
    CMP.b VRAMMapBestSpace_Size
    BCC +
    CLC
%ReturnLongShortDBG()               ;return false;
++
    LDA.b VRAMMapCurrentSpace_Size
+
    STA.b VRAMMapBestSpace_Size         ;best.Size = current.Size;
    LDA.b VRAMMapCurrentSpace_Score
    STA.b VRAMMapBestSpace_Score    
    LDA.b VRAMMapCurrentSpace_Offset    ;best.Score = current.Score;
    STA.b VRAMMapBestSpace_Offset       ;best.Offset = current.Offset;
    SEC
%ReturnLongShortDBG()
;public void RemovePosesInSpace(Space space)
;{
;    byte limit = (byte)(space.Offset + space.Size);
;    VRAMMapSlot slot;
;    byte size;
;    for (byte i = space.Offset; i < limit; i += size) 
;    {
;        slot = slots[i];
;        size = slot.GetSize(PoseDataBase, Hashmap);
;        if (!slot.IsFree)
;            Hashmap.Remove(slot.SizeOrPose);
;    }
;}
;Input:
;   VRAMMapBestSpace
RemovePosesInSpace:
    LDA.b VRAMMapBestSpace_Offset : TAX ;i = space.Offset
    CLC : ADC.b VRAMMapBestSpace_Size : STA.b VRAMMapLoop ;limit = space.Offset + space.Size
.loop
    %VRAMMapSlot_GetSize()
    STA.b VRAMMapSlot_Size ;size = slot.GetSize(PoseDataBase, Hashmap);

    %VRAMMapSlot_IsFree() ;if (!slot.IsFree)
    BNE +
        PHX
        LDA.l DX_Dynamic_Tile_Pose,x : ASL : TAX
        %CallFunctionLongShortDBG(DynamicPoseHashmap_Remove) ;Hashmap.Remove(slot.SizeOrPose);
        PLX
    +

    TXA : CLC : ADC.b VRAMMapSlot_Size : TAX ;i += size
    CMP.b VRAMMapLoop : BCC .loop ;i < limit
%ReturnLongShortDBG()

;public void RemoveSpace(Space space)
;{
;    RemovePosesInSpace(space);
;    var slot = slots[space.Offset];
;    slot.Offset = space.Offset;
;    slot.SizeOrPose = (byte)((space.Size - 1) | 0x80);
;
;    slot = slots[space.Offset + space.Size - 1];
;    slot.Offset = space.Offset;
;    slot.SizeOrPose = (byte)((space.Size - 1) | 0x80);
;}
;Input:
;   VRAMMapBestSpace
RemoveSpace:
    %CallFunctionLongShortDBG(RemovePosesInSpace) ;RemovePosesInSpace(space);

    LDA.b VRAMMapBestSpace_Offset : TAX ;var slot = slots[space.Offset];
    STA.l DX_Dynamic_Tile_Offset,x ;slot.Offset = space.Offset;
    LDA.b VRAMMapBestSpace_Size : DEC A : ORA.b #$80 : STA.l DX_Dynamic_Tile_Pose,x ;slot.SizeOrPose = (byte)((space.Size - 1) | 0x80);

    LDA.b VRAMMapBestSpace_Offset : CLC : ADC.b VRAMMapBestSpace_Size : DEC A : TAX ;var slot = slots[space.Offset + space.Size - 1];
    LDA.b VRAMMapBestSpace_Offset : STA.l DX_Dynamic_Tile_Offset,x ;slot.Offset = space.Offset;
    LDA.b VRAMMapBestSpace_Size : DEC A : ORA.b #$80 : STA.l DX_Dynamic_Tile_Pose,x ;slot.SizeOrPose = (byte)((space.Size - 1) | 0x80);
%ReturnLongShortDBG()
;public void AddPoseInSpace(byte hashmapIndex, Space space)
;{
;    var slot = slots[space.Offset];
;    slot.SizeOrPose = hashmapIndex;
;    slot.Offset = space.Offset;
;
;    byte nextSlotIndex = (byte)(space.Offset + slot.GetSize(PoseDataBase, Hashmap));
;    slot = slots[nextSlotIndex - 1];
;    slot.SizeOrPose = hashmapIndex;
;    slot.Offset = space.Offset;
;
;    byte size = (byte)(nextSlotIndex - space.Offset);
;    if (size == space.Size)
;        return;
;    size = (byte)((space.Size - size - 1) | 0x80);
;    slot = slots[nextSlotIndex];
;    slot.SizeOrPose = size;
;    slot.Offset = nextSlotIndex;
;
;    slot = slots[nextSlotIndex + (size & 0x7F)];
;    slot.SizeOrPose = size;
;    slot.Offset = nextSlotIndex;
;}
;Input:
;   HashIndexBackup
;   VRAMMapBestSpace
;   VRAMMapTMP_Size
AddPoseInSpace:

    LDA.b VRAMMapBestSpace_Offset
    TAX                             ;var slot = slots[space.Offset];
    STA.l DX_Dynamic_Tile_Offset,x  ;slot.Offset = space.Offset;
    LDA.b HashIndexBackup
    STA.l DX_Dynamic_Tile_Pose,x    ;slot.SizeOrPose = hashmapIndex;

    LDA.b VRAMMapTMP_Size
    CLC
    ADC.b VRAMMapBestSpace_Offset   ;byte nextSlotIndex = (byte)(space.Offset + slot.GetSize(PoseDataBase, Hashmap));
    DEC A
    TAX                             ;slot = slots[nextSlotIndex - 1];
    LDA.b VRAMMapBestSpace_Offset
    STA.l DX_Dynamic_Tile_Offset,x  ;slot.Offset = space.Offset; 
    LDA.b HashIndexBackup
    STA.l DX_Dynamic_Tile_Pose,x    ;slot.SizeOrPose = hashmapIndex;

    LDA.b VRAMMapBestSpace_Size
    SEC
    SBC.b VRAMMapTMP_Size
    BNE +                           ;if (size == space.Size)
%ReturnLongShortDBG()               ;   return;
+
    DEC A
    STA.b VRAMMapSlot_Size          ;size = (byte)((space.Size - size - 1) | 0x80);
    INX                             ;slot = slots[nextSlotIndex];
    ORA #$80
    STA.l DX_Dynamic_Tile_Size,x    ;slot.SizeOrPose = size;
    TXA                             
    STA.l DX_Dynamic_Tile_Offset,x  ;slot.Offset = nextSlotIndex;

    CLC
    ADC.b VRAMMapSlot_Size
    PHX
    TAX                             ;slot = slots[nextSlotIndex + (size & 0x7F)];

    LDA.b VRAMMapSlot_Size
    ORA #$80
    STA.l DX_Dynamic_Tile_Size,x    ;slot.SizeOrPose = size;
    PLA
    STA.l DX_Dynamic_Tile_Offset,x  ;slot.Offset = nextSlotIndex;
    
%ReturnLongShortDBG()

}
namespace off