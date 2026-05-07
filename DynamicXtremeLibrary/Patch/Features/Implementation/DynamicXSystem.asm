!OffsetBetweenSameHash = $000F

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

;public bool TakeDynamicRequest(ushort id)
;{
;    if (Hashmap.FindPose(id, out byte hashmapindex))
;        return true;
;    DynamicPose pose = PoseDataBase.Get(id);
;    ushort currentDataSent = (ushort)(pose.Size + CurrentDataSent);
;    if (currentDataSent > MaximumDataPerFrame)
;        return false;
;    Space bestSpace = VRAMMap.GetBestSlot(pose.Blocks16x16, TimeSpan);
;    if (bestSpace.Offset == 0xFF)
;        return false;
;    VRAMMap.RemoveSpace(bestSpace);
;    Hashmap.FindFreeSpace(ref hashmapindex);
;    CurrentDataSent = currentDataSent;
;    Hashmap.Add(hashmapindex, new(pose.ID, bestSpace.Offset, TimeSpan));
;    VRAMMap.AddPoseInSpace(hashmapindex, bestSpace);
;    return true;
;}
;Input:
;   Y 16 bits: Pose ID
TakeDynamicRequest:
	JSL DynamicPoseHashmap_FindPose
	BCC +                           ;if (Hashmap.FindPose(id, out byte hashmapindex))
		LDA.b HashIndexBackup
		ASL
		TAX
		LDA.l DX_Timer
		STA.l DX_Dynamic_Pose_TimeLastUse,x
		LDA.l DX_Timer+1
		STA.l DX_Dynamic_Pose_TimeLastUse+1,x
		LDX.b HashIndexBackup
		SEC : RTL ;return true;
	+
	PHB : PHK : PLB

	REP #$30 ;AXY->16 bit
			LDA.b PoseIDBackup : ASL : TAY ;DynamicPose pose = PoseDataBase.Get(id);
			LDA.w PoseSize,y : CLC : ADC.l DX_Dynamic_CurrentDataSend
			CMP.l DX_Dynamic_MaxDataPerFrame : BEQ + : BCC + ;if (currentDataSent > MaximumDataPerFrame)
				SEP #$30
				PLB
				CLC : RTL  ;return false;
			+
			PHA ;ushort currentDataSent = (ushort)(pose.Size + CurrentDataSent);
			LDY.b PoseIDBackup
		SEP #$20 ;A->8 bit
		LDA.w Pose16x16Blocks,y : STA.b VRAMMapTMP_Size
	SEP #$10 ;XY->8 bit
	%CallFunctionLongShortDBG(VRAMMap_GetBestSlot) ;Space bestSpace = VRAMMap.GetBestSlot(pose.Blocks16x16, TimeSpan);
	LDA.b VRAMMapBestSpace_Offset : CMP #$FF : BNE + ;if (bestSpace.Offset == 0xFF)
		PLA : PLA : PLB
		CLC : RTL  ;return false;
	+
	%CallFunctionLongShortDBG(VRAMMap_RemoveSpace)	;VRAMMap.RemoveSpace(bestSpace);
	JSL DynamicPoseHashmap_FindFreeSpace			;Hashmap.FindFreeSpace(ref hashmapindex);
	REP #$20 ;A->16 bit
		PLA : STA.l DX_Dynamic_CurrentDataSend ;CurrentDataSent = currentDataSent;
	SEP #$20 ;A->8 bit

	LDX.b HashIndexBackup
	LDA.b VRAMMapBestSpace_Offset : STA.l DX_Dynamic_Pose_Offset,x
	TXA : ASL : TAX
	%CallFunctionLongShortDBG(DynamicPoseHashmap_Add)	;Hashmap.Add(hashmapindex, new(pose.ID, bestSpace.Offset, TimeSpan));
	%CallFunctionLongShortDBG(VRAMMap_AddPoseInSpace)	;VRAMMap.AddPoseInSpace(hashmapindex, bestSpace);

	LDX.b HashIndexBackup
	PHX
	LDA DX_Dynamic_Pose_Offset,x
	JSL DynamicRoutine
	PLX
	PLB
	SEC
RTL                                                             ;return true;
	
incsrc "../Data/DynamicPoseData.asm"
