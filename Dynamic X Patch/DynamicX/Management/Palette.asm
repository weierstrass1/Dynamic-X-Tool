;Input:
;A = ID, 16 bits
;Output:
;Y = Palette, 8 bits 
;Carry set = Palette already loaded, Carry clear = Palette not loaded
AssignPalette:
    STA $00

.UnrolledFindPal
    !i = $00
while !i < $08
    LDA DX_Dynamic_Palettes_ID+$10+!i+!i
    CMP $00
    BNE +
    SEP #$20
    LDY #!i
    SEC
RTL
+
    !i #= !i+$01
endif

.UnrolledFindFreePal
    !i = $00
while !i < $08
    LDA DX_Dynamic_Palettes_ID+$10+!i+!i
    CMP #$FFFE
    BEQ +
    LDA DX_Dynamic_Palettes_Updated+$08+!i
    AND #$00FF
    BNE +
    LDA $00
    STA DX_Dynamic_Palettes_ID+$10+!i+!i
    SEP #$20
    LDY #!i
    CLC
RTL
+
    !i #= !i+$01
endif
    SEP #$20
    LDY #$FF
    CLC
RTL

if !PaletteEffects
;Get Max between 3 values
macro max(v1,v2,v3)
?max:
    LDA <v1>
    CMP <v2>
    BCS ?.v1maxcandidate

    LDA <v2>
    CMP <v3>
    BCC ?.v3max
?.v2max
    LDX #$0002
    BRA  ?.finish

?.v1maxcandidate
    CMP <v3>
    BCS ?.v1max

?.v3max
    LDX #$0004
    LDA <v3>
    BRA ?.finish
?.v1max
    LDX #$0000
?.finish
endmacro

;Get Min between 3 values
macro min(v1,v2,v3)
?min:
    LDA <v1>
    CMP <v2>
    BCC ?.v1mincandidate

    LDA <v2>
    CMP <v3>
    BCS ?.v3min
    BRA ?.v2min

?.v1mincandidate
    CMP <v3>
    BCC ?.v1min

?.v3min
    LDA <v3>

?.v2min
?.v1min
endmacro

;Split v into 3 differents channels c1,c2,c3
macro splitChannels(v,c1,c2,c3)
    
    REP #$20
    LDA <v>
    AND #$001F
    STA <c3>

    LDA <v>
    LSR
    LSR
    XBA
    AND #$001F
    STA <c1>

    LDA <v>
    LSR
    LSR
    LSR
    LSR
    LSR
    SEP #$20
    AND #$1F
    STA <c2>

endmacro

;Merge channels v1,v2,v3 into r
macro mergeChannels(v1,v2,v3,rh,rl)
    LDA <v2>
    ASL
    ASL
    ASL
    ASL
    ASL
    ORA <v3>
    STA <rl>

    LDA <v1>
    ASL
    ASL
    PHA 

    LDA <v2>
    LSR
    LSR
    LSR
    ORA $01,s
    STA <rh>

    PLA
endmacro

!R = $00
!G = $01
!B = $02

!Min = $04
!Max = $05

!H = $06
!S = $07
!L = $08

!C = $09
!Div = $0A

!C = $09
!X = $0A
!M = $0B

RGB2HSL:
    %max("!R","!G","!B")
    STA !Max

    %min("!R","!G","!B")
    STA !Min

    CLC
    ADC !Max
    LSR
    STA !L

    LDA !Max
    SEC
    SBC !Min
    BNE +
    STA !S
    STA !H
RTS
+
    STA !C

    LDA !Min
    CLC
    ADC !Max
    SEC
    SBC #$1F
    BPL +
    EOR #$FF
    INC A
+
    EOR #$FF
    INC A
    CLC
    ADC #$1F
    STA !Div

    %Mul(" !C", " #$1F")
    %DivWAfterMul("!Div")

    LDA !DivisionResult
    STA !S

    JMP (hcalc,x)

hcalc:
    dw rmax
    dw gmax
    dw bmax

X60degrees:
    dw $FF5B,$FF60,$FF65,$FF6B,$FF70,$FF75,$FF7B,$FF80,$FF85,$FF8B,$FF90,$FF95,$FF9B,$FFA0,$FFA5,$FFAB
    dw $FFB0,$FFB5,$FFBB,$FFC0,$FFC5,$FFCB,$FFD0,$FFD5,$FFDB,$FFE0,$FFE5,$FFEB,$FFF0,$FFF5,$FFFB,$0000
    dw $0005,$000B,$0010,$0015,$001B,$0020,$0025,$002B,$0030,$0035,$003B,$0040,$0045,$004B,$0050,$0055
    dw $005B,$0060,$0065,$006B,$0070,$0075,$007B,$0080,$0085,$008B,$0090,$0095,$009B,$00A0,$00A5
rmax:

    LDA #$00
    XBA
    LDA !G
    SEC
    SBC !B
    CLC
    ADC #$1F
    ASL
    TAX

    STZ $0A

    REP #$20
    LDA X60degrees,x
    BPL +
    EOR #$FFFF
    INC A
    INC $0A
+
    STA $0B
    SEP #$20
    
    %DivW(" $0C", " $0B", " !C")

    LDA $0A
    BEQ +

    LDA !DivisionResult
    EOR #$FF
    INC A
    CLC
    ADC #$20
    AND #$1F
    STA !H

RTS

+
    LDA !DivisionResult
    CLC
    ADC #$20
    AND #$1F
    STA !H

RTS

gmax:

    LDA #$00
    XBA
    LDA !B
    SEC
    SBC !R
    CLC
    ADC #$1F
    ASL
    TAX

    STZ $0A

    REP #$20
    LDA X60degrees,x
    BPL +
    EOR #$FFFF
    INC A
    INC $0A
+
    STA $0B
    SEP #$20
    
    %DivW(" $0C", " $0B", " !C")

    LDA $0A
    BEQ +

    LDA !DivisionResult
    EOR #$FF
    INC A
    CLC
    ADC #$0B
    STA !H

RTS

+
    LDA !DivisionResult
    CLC
    ADC #$0B
    STA !H

RTS

bmax:

    LDA #$00
    XBA
    LDA !R
    SEC
    SBC !G
    CLC
    ADC #$1F
    ASL
    TAX

    STZ $0A

    REP #$20
    LDA X60degrees,x
    BPL +
    EOR #$FFFF
    INC A
    INC $0A
+
    STA $0B
    SEP #$20
    
    %DivW(" $0C", " $0B", " !C")

    LDA $0A
    BEQ +

    LDA !DivisionResult
    EOR #$FF
    INC A
    CLC
    ADC #$15
    STA !H

RTS

+
    LDA !DivisionResult
    CLC
    ADC #$15
    STA !H

RTS

HSL2RGB:

    LDA #$00
    XBA
    LDA !L
    REP #$20
    ASL
    ASL
    ASL
    ASL
    ASL
    SEP #$20
    ORA !S
    TAX

    LDA #$00
    XBA
    LDA Cs,x
    STA !C
    REP #$20
    ASL
    ASL
    ASL
    ASL
    ASL
    SEP #$20
    ORA !H
    TAX

    LDA Xs,x
    STA !X

    LDA !C
    LSR
    EOR #$FF
    INC A
    CLC
    ADC !L
    STA !M

    LDA #$00
    XBA
    LDA !H
    ASL
    TAX

    JMP (hfunc,x)

Cs:
    incbin "cs.bin"
Xs:
    incbin "xs.bin"

hfunc:
    dw h1,h1,h1,h1,h1,h1
    dw h2,h2,h2,h2,h2
    dw h3,h3,h3,h3,h3,h3
    dw h4,h4,h4,h4,h4
    dw h5,h5,h5,h5,h5
    dw h6,h6,h6,h6,h6

h1:
    LDA !C
    CLC
    ADC !M
    STA !R

    LDA !X
    CLC
    ADC !M
    STA !G

    LDA !M
    STA !B
RTS

h2:
    LDA !X
    CLC
    ADC !M
    STA !R

    LDA !C
    CLC
    ADC !M
    STA !G

    LDA !M
    STA !B
RTS

h3:
    LDA !C
    CLC
    ADC !M
    STA !G

    LDA !X
    CLC
    ADC !M
    STA !B

    LDA !M
    STA !R
RTS

h4:
    LDA !X
    CLC
    ADC !M
    STA !G

    LDA !C
    CLC
    ADC !M
    STA !B

    LDA !M
    STA !R
RTS

h5:
    LDA !X
    CLC
    ADC !M
    STA !R

    LDA !C
    CLC
    ADC !M
    STA !B

    LDA !M
    STA !G
RTS

h6:
    LDA !C
    CLC
    ADC !M
    STA !R

    LDA !X
    CLC
    ADC !M
    STA !B

    LDA !M
    STA !G
RTS

macro getRatio(ratio,value)

    LDA <ratio>
    STA $6C

    LDA <value>
    STA $6D

    %MulW(" $6C"," $6D")
    REP #$20
    LDA !MultiplicationResult

endmacro

!Source = $45
!Dst = $48 
!iSource = $4B
!iDst = $4D
!length = $4F
!ratio1 = $8A
!ratio2 = $8C
!ratio3 = $8E
!V1 = $51
!V2 = $53
!V3 = $6A
!tmprl = $0E
!tmprh = $0F

SetHSLBase:
    PHB
    PHK
    PLB

    REP #$30

    LDA !length
    DEC A
    STA !length
    ASL
    STA !iSource    ;!iSource = length*2
    TAY
    CLC
    ADC !length
    STA !iDst      ;!iDest = length*3

    SEP #$20
.loop
    %splitChannels("[!Source],y",!B,!G,!R)

    JSR RGB2HSL

    REP #$20
    LDA !iDst
    TAY
    SEC
    SBC #$0003
    STA !iDst
    SEP #$20

    LDA !H
    STA [!Dst],y
    INY
    LDA !S
    STA [!Dst],y
    INY
    LDA !L
    STA [!Dst],y

    REP #$20
    LDA !iSource
    DEC A
    DEC A
    STA !iSource
    TAY
    SEP #$20
    BPL .loop

    SEP #$10
    PLB
RTL

MixHSL:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatio(!ratio1,!V1)
    STA !V1
    SEP #$20

    %getRatio(!ratio2,!V2)
    STA !V2
    SEP #$20

    %getRatio(!ratio3,!V3)
    STA !V3

    LDA #$001F
    SEC
    SBC !ratio1
    STA !ratio1

    LDA #$001F 
    SEC
    SBC !ratio2
    STA !ratio2

    LDA #$001F 
    SEC
    SBC !ratio3
    STA !ratio3

    LDA !length
    DEC A
    STA !length
    ASL
    STA !iDst       ;!iDest = length*2
    CLC
    ADC !length
    STA !iSource    ;!iSource = length*3
    TAY

    SEP #$20
.loop

    %getRatio(!ratio1,"[!Source],y")
    CLC
    ADC !V1
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$1F)
    LDA !DivisionResult
    STA !H

    INY

    %getRatio(!ratio2,"[!Source],y")
    CLC
    ADC !V2
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$1F)
    LDA !DivisionResult
    STA !S

    INY

    %getRatio(!ratio3,"[!Source],y")
    CLC
    ADC !V3
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$1F)
    LDA !DivisionResult
    STA !L

    JSR HSL2RGB

    %mergeChannels(!B,!G,!R,!tmprh,!tmprl)

    REP #$20
    LDA !iDst
    TAY
    DEC A
    DEC A
    STA !iDst

    LDA !tmprl
    STA [!Dst],y
    SEP #$20

    REP #$20
    LDA !iSource
    SEC
    SBC #$0003
    STA !iSource
    TAY 
    SEP #$20
    BMI .exit
    JMP .loop
.exit
    SEP #$10

    PLB
RTL

SetRGBBase:
    PHB
    PHK
    PLB

    REP #$30

    LDA !length
    DEC A
    STA !length
    ASL
    STA !iSource    ;!iSource = length*2
    TAY
    CLC
    ADC !length
    STA !iDst      ;!iDest = length*3

    SEP #$20
.loop
    %splitChannels("[!Source],y",!B,!G,!R)

    REP #$20
    LDA !iDst
    TAY
    SEC
    SBC #$0003
    STA !iDst
    SEP #$20

    LDA !R
    STA [!Dst],y
    INY
    LDA !G
    STA [!Dst],y
    INY
    LDA !B
    STA [!Dst],y

    REP #$20
    LDA !iSource
    DEC A
    DEC A
    STA !iSource
    TAY
    SEP #$20
    BPL .loop

    SEP #$10
    PLB
RTL

MixRGB:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatio(!ratio1,!V1)
    STA !V1
    SEP #$20

    %getRatio(!ratio2,!V2)
    STA !V2
    SEP #$20

    %getRatio(!ratio3,!V3)
    STA !V3

    LDA #$001F
    SEC
    SBC !ratio1
    STA !ratio1

    LDA #$001F 
    SEC
    SBC !ratio2
    STA !ratio2

    LDA #$001F
    SEC
    SBC !ratio3
    STA !ratio3

    LDA !length
    DEC A
    STA !length
    ASL
    STA !iDst       ;!iDest = length*2
    CLC
    ADC !length
    STA !iSource    ;!iSource = length*3
    TAY

    SEP #$20
.loop

    %getRatio(!ratio1,"[!Source],y")
    CLC
    ADC !V1
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$1F)
    LDA !DivisionResult
    STA !R

    INY

    %getRatio(!ratio2,"[!Source],y")
    CLC
    ADC !V2
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$1F)
    LDA !DivisionResult
    STA !G

    INY

    %getRatio(!ratio3,"[!Source],y")
    CLC
    ADC !V3
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$1F)
    LDA !DivisionResult
    STA !B

    %mergeChannels(!B,!G,!R,!tmprh,!tmprl)

    REP #$20
    LDA !iDst
    TAY
    DEC A
    DEC A
    STA !iDst

    LDA !tmprl
    STA [!Dst],y
    SEP #$20

    REP #$20
    LDA !iSource
    SEC
    SBC #$0003
    STA !iSource
    TAY 
    SEP #$20
    BMI .exit
    JMP .loop
.exit

    SEP #$10

    PLB
RTL
endif