macro AutoUnrolledRoll(imin, imax)
    REP #$20
!i = <imin>
!j = 2**<imin>
while !i < <imax>
    LDA $00
    CMP.l DX_Dynamic_Palettes_ID+$10+(!i*2)
    BNE +
    LDA DX_Dynamic_Palettes_DisableTimer+$08+!i
    AND #$00FF
    BEQ +
    LDA DX_Dynamic_Palettes_EffectEnabled
    EOR $D5
    AND.w #!j
    BNE +    
    SEP #$20

    LDA #$02
    STA.l DX_Dynamic_Palettes_DisableTimer+$08+!i

    LDA.b #(!i*2)
    SEC
RTS
+
!i #= !i+1
!j #= !j*2
endif

!i = <imin>
while !i < <imax>
    LDA DX_Dynamic_Palettes_ID+$10+(!i*2)
    CMP #$FFFE
    BEQ +
    CMP #$FFFF
    BEQ ++
    LDA DX_Dynamic_Palettes_DisableTimer+$08+!i
    AND #$00FF
    BNE +
++
    LDA $00
    STA DX_Dynamic_Palettes_ID+$10+(!i*2)
    SEP #$20

    LDA #$02
    STA.l DX_Dynamic_Palettes_DisableTimer+$08+!i

    DEY
    LDA.b #(!i*2)
    SEC
RTS
+
!i #= !i+1
endif
    SEP #$20
    CLC
RTS
endmacro

;Inputs:
;A low = Assignment Option
;A High = Palette Option
;$00 = Palette ID
;Outputs:
;A = Palette
;Carry Clear = Success, Carry Set = Fail
AssignPalette:
    LSR
    LSR
    LSR
    AND #$06
    TAX

    LDY #$00

    XBA
    JSR (.AssignmentsRoutines,x)
RTL
.AssignmentsRoutines
    dw .Automatic
    dw .Manual
    dw .NoAssignment
    dw .Automatic

.Automatic
    BEQ +
    JMP .AutomaticUpperOrLower
+
    %AutoUnrolledRoll(0, 8)
.AutomaticUpperOrLower
    BIT #$40
    REP #$20
    BEQ ..PalUpper
    JMP ..PalLower
..PalUpper
    %AutoUnrolledRoll(0, 4)
..PalLower
    %AutoUnrolledRoll(4, 8)

.Manual
.NoAssignment
    LSR
    LSR
    LSR
    AND #$0E
    PHA
    TAX

    LDA $00
    STA.l DX_Dynamic_Palettes_ID+$10,x
    LDA $01
    STA.l DX_Dynamic_Palettes_ID+$11,x

    TXA
    LSR
    TAX

    LDA #$02
    STA.l DX_Dynamic_Palettes_DisableTimer+$08,x

    DEY
    PLA
    SEC
RTS
