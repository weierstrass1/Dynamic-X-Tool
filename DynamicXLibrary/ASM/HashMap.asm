;Implementacion de Hashmap
namespace DynamicPoseHashmap
{

;-----------------------------------------------------------------------------------------
;             DynamicPoseHashmap.FindPose     JSL DynamicPoseHashmap_FindPose
;-----------------------------------------------------------------------------------------

;-- ENTRADA:
;A -> 8-bit, XY -> 16-bit
;Y (16-bit) = Pose ID

;-- SALIDA:
;Carry clear si no se encontro y Carry set si se encontro
;HashIndexBackup es el slot devuelto.
FindPose:
		;getHashCode
		STY.w PoseIDBackup : TYA : AND.b #!HASHMAP_SIZE-1
	SEP #$10 ;XY -> 8
	STA.b HashCodeBackup : STA.b HashIndexBackup
	TAX

	LDA.l DX_Dynamic_Pose_Length : BEQ .ReturnFalseCarryClear ;if (Length == 0) return false
	LDA.l DX_Dynamic_Pose_HashSize,X : BEQ .ReturnFalseCarryClear ;if (hashSize[hashCode]) return false
	STA.b HashSizeBackup

	;X = hashCode * 2
	LDA.b HashCodeBackup : ASL : TAX
.hashLoop
	REP #$20 ;A->16
		LDA.l DX_Dynamic_Pose_ID,X : CMP.w #$FFFF : BEQ .incrementHashLoopAndContinue ;if (slot is null)
		CMP.b PoseIDBackup ;z = (A == PoseIDBackup)
	SEP #$20 ;A->8
	BEQ ReturnHashIndexAndTrue ;slot.ID == id

	AND.b #!HASHMAP_SIZE-1 : CMP.b HashCodeBackup : BNE .incrementHashLoopAndContinue_8bit ;DynamicPoseHashMapSlot.GetHashCode(slot.ID) != hashCode
	DEC.b HashSizeBackup : BNE .incrementHashLoopAndContinue_8bit ;i--, i > 0 -> incrementHashLoopAndContinue_8bit
;no se encontro, devolver X / 2 y Carry Clear
.couldNotBeFound
	TXA : LSR : STA.b HashIndexBackup
.ReturnFalseCarryClear
	CLC
RTL

;Incrementar resultado hash y continuar.
.incrementHashLoopAndContinue
	SEP #$20 ;A->8
.incrementHashLoopAndContinue_8bit
	;Seguir la busqueda
	TXA : CLC : ADC.b #!INCREASE_PER_STEP*2 : AND.b #(!HASHMAP_SIZE-1)*2 : TAX
BRA .hashLoop

;se encontro, devolver X / 2 y Carry Set
;este label es tambien utilizado en FindFreeSpace para devolver el slot que se encontro.
ReturnHashIndexAndTrue:
	TXA : LSR : STA.b HashIndexBackup : SEC
RTL

;---------------------------------------------------------------------------------------------
;          DynamicPoseHashmap.FindFreeSpace     JSL DynamicPoseHashmap_FindFreeSpace
;---------------------------------------------------------------------------------------------
;-- ENTRADA:
;AXY -> 8-bit
;X = hashmapIndex

;-- SALIDA:
;Carry clear si no se encontro y Carry set si se encontro
;HashIndexBackup es el slot devuelto.
FindFreeSpace:
	LDA.l DX_Dynamic_Pose_Length : CMP.b #!HASHMAP_SIZE : BCS FindPose_ReturnFalseCarryClear ;Length >= HASHMAP_SIZE

	;X = hashmapIndex * 2
	LDA.b HashIndexBackup : ASL : TAX
.hashLoop
	REP #$20 ;A->16
		LDA.l DX_Dynamic_Pose_ID,X : CMP.w #$FFFF ;z = if (slot is null)
	SEP #$20 ;A->8
	BEQ ReturnHashIndexAndTrue

	;Seguir la busqueda
	TXA : CLC : ADC.b #!INCREASE_PER_STEP*2 : AND.b #(!HASHMAP_SIZE-1)*2 : TAX
BRA .hashLoop

;------------------------------------------------------------------------------------------------------------
;                DynamicPoseHashmap.Add     %CallFunctionLongShortDBG(DynamicPoseHashmap_Add)
;------------------------------------------------------------------------------------------------------------
;public void Add(byte hashmapIndex, DynamicPoseHashMapSlot slot)
;{
;	if (slots[hashmapIndex] is not null)
;		throw new Exception("Slot is Already used");
;	slots[hashmapIndex] = slot;
;	Length++;
;	hashSize[DynamicPoseHashMapSlot.GetHashCode(slot.ID)]++;
;}
;-- ENTRADA:
;PoseIDBackup = Pose ID (16-bit)
;X = hashmapIndex * 2
Add:
	REP #$20
		if !DEBUG != 0 ;-- TEST: slots[hashmapIndex] is not null
			;if (slots[hashmapIndex] is not null)
			;	throw new Exception("Slot is Already used");
			LDA.l DX_Dynamic_Pose_ID,X : CMP.w #$FFFF : BEQ +
			SEP #$20
			LDA #$FF : STA TEST_STATUS
			%ReturnLongShortDBG()
		+
		endif
	;slots[hashmapIndex] = slot;
	LDA.l DX_Timer
	STA.l DX_Dynamic_Pose_TimeLastUse,x
	LDA.b PoseIDBackup
	STA.l DX_Dynamic_Pose_ID,x
	SEP #$20
	AND.b #!HASHMAP_SIZE-1 : TAX ;DynamicPoseHashMapSlot.GetHashCode()
	LDA.l DX_Dynamic_Pose_HashSize,X : INC A : STA.l DX_Dynamic_Pose_HashSize,X ;hashSize[DynamicPoseHashMapSlot.GetHashCode(slot.ID)]++;
	LDA.l DX_Dynamic_Pose_Length : INC A : STA.l DX_Dynamic_Pose_Length 		;Length++;
%ReturnLongShortDBG()

;------------------------------------------------------------------------------------------------------------------
;                DynamicPoseHashmap.Remove     %CallFunctionLongShortDBG(DynamicPoseHashmap_Remove)
;------------------------------------------------------------------------------------------------------------------
;-- ENTRADA:
;XY -> 8-bit
;X = hashmapIndex * 2
Remove:
	REP #$20
		LDA.l DX_Dynamic_Pose_ID,X
		if !DEBUG != 0 ;-- TEST: slots[hashmapIndex] is null
			CMP.w #$FFFF : BNE +
			SEP #$20
			LDA #$FF : STA TEST_STATUS
			%ReturnLongShortDBG()
		+
		endif
		PHA
		LDA.w #$FFFF : STA.l DX_Dynamic_Pose_ID,X ;slots[hashmapIndex] = null;
		PLA
	SEP #$20
	AND.b #!HASHMAP_SIZE-1 : TAX ;DynamicPoseHashMapSlot.GetHashCode()
	LDA.l DX_Dynamic_Pose_HashSize,X : DEC A : STA.l DX_Dynamic_Pose_HashSize,X ;hashSize[]--
	LDA.l DX_Dynamic_Pose_Length : DEC A : STA.l DX_Dynamic_Pose_Length ;Length--;
%ReturnLongShortDBG()

}
namespace off
