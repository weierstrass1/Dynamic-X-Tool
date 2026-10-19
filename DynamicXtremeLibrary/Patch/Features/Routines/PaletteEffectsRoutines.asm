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

macro splitChannels(v,c1,c2,c3)
    
    LDA <v>
    AND #$1F
    STA <c3>

    REP #$20

    LDA <v>
    LSR
    LSR
    XBA
    AND #$001F
    STA <c1>

    LDA <v>
    ROL
    ROL
    ROL
    SEP #$20
    XBA
    AND #$1F
    STA <c2>

endmacro

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

!Target = $51

!ratio1Inv = $D5
!ratio2Inv = $D6
!ratio3Inv = $D7

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

    REP #$21
    LDA !iDst
    TAY
    ADC #$FFFD
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

    REP #$21
    LDA !iSource
    ADC #$FFFE
    STA !iSource
    TAY
    SEP #$20
    BPL .loop

    SEP #$10
    PLB
RTL

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

    REP #$21
    LDA !iDst
    TAY
    ADC #$FFFD
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

    REP #$21
    LDA !iSource
    ADC #$FFFE
    STA !iSource
    TAY
    SEP #$20
    BPL .loop

    SEP #$10
    PLB
RTL

macro getRatio(ratio,value)

    %MulW(" <ratio>"," <value>")
    REP #$20
    LDA !MultiplicationResult

endmacro

macro getRatioedValue(ratio,value)

    %getRatio("<ratio>","<value>")
    STA <value>

    LDA #$007F
    SEC
    SBC <ratio>
    STA <ratio>
endmacro

macro interpolate(ratio,value,adder)
    %getRatio("<ratio>","<value>")
    CLC
    ADC <adder>
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$7F)
    LDA !DivisionResult
endmacro

macro interpolate2(ratio1,value1,ratio2,value2)
    %getRatio("<ratio2>","<value2>")
    STA !tmprl
    SEP #$20

    %getRatio("<ratio1>","<value1>")
    CLC
    ADC !tmprl
    STA !tmprl
    SEP #$20
    %DivW(!tmprh, !tmprl, #$7F)
    LDA !DivisionResult
endmacro

macro interpolateHue(ratio1,value1,ratio2,value2)
    LDA <value2>
    SEC
    SBC <value1>
    BPL ?+
    CLC
    ADC #$7F
?+
    STA !tmprl

    LDA <value1>
    CLC
    ADC #$7F
    SEC
    SBC <value2>
    CMP !tmprl
    LDA <value2>
    BCS ?+
    CLC
    ADC #$7F
?+
    STA !tmprl
    %interpolate2("<ratio1>","<value1>","<ratio2>",!tmprl)
    AND #$7F
endmacro

macro mergeAndWrite(C0, C1, C2)
    %mergeChannels("<C0>","<C1>","<C2>",!tmprh,!tmprl)

    REP #$20
    LDA !iDst
    TAY
    DEC A
    DEC A
    STA !iDst

    LDA !tmprl
    STA [!Dst],y
    SEP #$20
endmacro

macro PreMergeLoop()
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
endmacro

macro LoopEnd()
    REP #$20
    LDA !iSource
    SEC
    SBC #$0003
    STA !iSource
    TAY 
    SEP #$20
    BMI .exit
    JMP .loop
endmacro

MixAndMergeR:
    STZ !ratio1+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA !R

    INY

    LDA [!Source],y
    STA !G

    INY

    LDA [!Source],y
    STA !B

    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10   
RTL

MixAndMergeG:
    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA !R

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !G

    INY

    LDA [!Source],y
    STA !B
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10   
RTL

MixAndMergeB:
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA !R

    INY

    LDA [!Source],y
    STA !G

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !B
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10   
RTL

MixAndMergeRG:
    STZ !ratio1+1
    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)
    SEP #$20

    %getRatioedValue(!ratio2,!V2)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA !R

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !G

    INY

    LDA [!Source],y
    STA !B
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10
RTL

MixAndMergeRB:
    STZ !ratio1+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA !R

    INY

    LDA [!Source],y
    STA !G

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !B
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10
RTL

MixAndMergeGB:
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA !R

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !G

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !B
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10
RTL

MixAndMergeRGB:

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)
    SEP #$20

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA !R

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !G

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !B
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10
RTL

MixAndMergeH:
    PHB
    PHK
    PLB

    STZ !ratio1+1

    REP #$30

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA !H

    INY

    LDA [!Source],y
    STA !S

    INY

    LDA [!Source],y
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10  
    PLB 
RTL

MixAndMergeS:
    PHB
    PHK
    PLB

    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA !H

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !S

    INY

    LDA [!Source],y
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixAndMergeL:
    PHB
    PHK
    PLB

    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA !H

    INY

    LDA [!Source],y
    STA !S

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10  
    PLB 
RTL

MixAndMergeHS:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio1Inv
    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA !H

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !S

    INY

    LDA [!Source],y
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixAndMergeHL:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio1Inv
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio3,!V3)

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA !H

    INY

    LDA [!Source],y
    STA !S

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixAndMergeSL:
    PHB
    PHK
    PLB

    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA !H

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !S

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10  
    PLB 
RTL

MixAndMergeHSL:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio1Inv
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA !H

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA !S

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

PalTransitionAndMergeRGB:

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$30

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    LDA #$007F
    SEC
    SBC !ratio2
    STA !ratio2Inv

    LDA #$007F
    SEC
    SBC !ratio3
    STA !ratio3Inv

    %PreMergeLoop()
.loop

    %interpolate2(!ratio1,"[!Source],y",!ratio1Inv,"[!Target],y")
    STA !R

    INY

    %interpolate2(!ratio2,"[!Source],y",!ratio2Inv,"[!Target],y")
    STA !G

    INY

    %interpolate2(!ratio3,"[!Source],y",!ratio3Inv,"[!Target],y")
    STA !B
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10
RTL

PalTransitionAndMergeHSL:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$30

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    LDA #$007F
    SEC
    SBC !ratio2
    STA !ratio2Inv

    LDA #$007F
    SEC
    SBC !ratio3
    STA !ratio3Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,"[!Target],y")
    STA !H

    INY

    %interpolate2(!ratio2,"[!Source],y",!ratio2Inv,"[!Target],y")
    STA !S

    INY

    %interpolate2(!ratio3,"[!Source],y",!ratio3Inv,"[!Target],y")
    STA !L

    JSR HSL2RGB
    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10
    PLB
RTL

PalFunctionAndMergeRGB:
RTL

PalFunctionAndMergeHSL:
RTL

MixR:
    STZ !ratio1+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10   
RTL

MixG:
    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10   
RTL

MixB:
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10   
RTL

MixRG:
    STZ !ratio1+1
    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)
    SEP #$20

    %getRatioedValue(!ratio2,!V2)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10
RTL

MixRB:
    STZ !ratio1+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10
RTL

MixGB:
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10
RTL

MixRGB:

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio1,!V1)
    SEP #$20

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    %interpolate(!ratio1,"[!Source],y",!V1)
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10
RTL

MixH:
    PHB
    PHK
    PLB

    STZ !ratio1+1

    REP #$30

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixS:
    PHB
    PHK
    PLB

    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixL:
    PHB
    PHK
    PLB

    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio3,!V3)

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixHS:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio1Inv
    STZ !ratio2+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixHL:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio1Inv
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio3,!V3)

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA [!Dst],y

    INY

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixSL:
    PHB
    PHK
    PLB

    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

MixHSL:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio1Inv
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$10

    %getRatioedValue(!ratio2,!V2)
    SEP #$20

    %getRatioedValue(!ratio3,!V3)

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,!V1)
    STA [!Dst],y

    INY

    %interpolate(!ratio2,"[!Source],y",!V2)
    STA [!Dst],y

    INY

    %interpolate(!ratio3,"[!Source],y",!V3)
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10   
    PLB
RTL

PalTransitionRGB:

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$30

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    LDA #$007F
    SEC
    SBC !ratio2
    STA !ratio2Inv

    LDA #$007F
    SEC
    SBC !ratio3
    STA !ratio3Inv

    %PreMergeLoop()
.loop

    %interpolate2(!ratio1,"[!Source],y",!ratio1Inv,"[!Target],y")
    STA [!Dst],y

    INY

    %interpolate2(!ratio2,"[!Source],y",!ratio2Inv,"[!Target],y")
    STA [!Dst],y

    INY

    %interpolate2(!ratio3,"[!Source],y",!ratio3Inv,"[!Target],y")
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10
RTL

PalTransitionHSL:
    PHB
    PHK
    PLB

    STZ !ratio1+1
    STZ !ratio2+1
    STZ !ratio3+1

    REP #$30

    LDA #$007F
    SEC
    SBC !ratio1
    STA !ratio1Inv

    LDA #$007F
    SEC
    SBC !ratio2
    STA !ratio2Inv

    LDA #$007F
    SEC
    SBC !ratio3
    STA !ratio3Inv

    %PreMergeLoop()
.loop

    %interpolateHue(!ratio1,"[!Source],y",!ratio1Inv,"[!Target],y")
    STA [!Dst],y

    INY

    %interpolate2(!ratio2,"[!Source],y",!ratio2Inv,"[!Target],y")
    STA [!Dst],y

    INY

    %interpolate2(!ratio3,"[!Source],y",!ratio3Inv,"[!Target],y")
    STA [!Dst],y

    JSR HSL2RGB

    %LoopEnd()
.exit

    SEP #$10
    PLB
RTL

PalFunctionRGB:
RTL

PalFunctionHSL:
RTL

RGBMerge:
    REP #$30
    %PreMergeLoop()
.loop

    LDA [!Source],y
    STA !R

    INY

    LDA [!Source],y
    STA !G

    INY

    LDA [!Source],y
    STA !B

    INY

    %mergeAndWrite(!B, !G, !R)

    %LoopEnd()
.exit

    SEP #$10
RTL

RGBToHSL:
    PHB
    PHK
    PLB

    REP #$30
    %PreMergeLoop()
.loop

    PHY
    LDA [!Source],y
    STA !R

    INY

    LDA [!Source],y
    STA !G

    INY

    LDA [!Source],y
    STA !B

    INY

    JSR RGB2HSL
    PLY

    LDA !H
    STA [!Dst],y

    INY

    LDA !S
    STA [!Dst],y

    INY

    LDA !L
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10
    PLB
RTL

HSLToRGB:
    PHB
    PHK
    PLB

    REP #$30
    %PreMergeLoop()
.loop

    PHY
    LDA [!Source],y
    STA !H

    INY

    LDA [!Source],y
    STA !S

    INY

    LDA [!Source],y
    STA !L

    INY

    JSR HSL2RGB
    PLY

    LDA !R
    STA [!Dst],y

    INY

    LDA !G
    STA [!Dst],y

    INY

    LDA !B
    STA [!Dst],y

    %LoopEnd()
.exit

    SEP #$10
    PLB
RTL

;Input:
;   $02-$04 = Palette Address (24 bits)
;   $05 = Number Of Colors
;	$06-$08 = Destination (24 bits)
;Output:
;   The palette is loaded on Destination
;   This changes Y value
LoadPaletteOnBuffer:
	LDA $05
	REP #$30
	AND #$00FF
	DEC A
	ASL
	TAY

.Loop

	LDA [$02],y
	STA [$06],y
	DEY
	DEY
	BPL .Loop
	SEP #$30
RTL

;Input:
;	A = Effect ID, 16 bits
;   $00 = Color ID (PPPP CCCC)
;       PPPP : Palette
;       CCCC : Color on the Palette
;	$45 = Source Buffer address (3 bytes per color),24 bits
;	$48 = Destination Buffer Address (2 bytes per color BGR555 format), 24 bits
;	$4F = Number of colors to change, 8 bits.
;Output:
;	Result of color effect in Destinarion buffer
DoEffectAndMerge:
	BNE +

	LDA $00
	AND #$00FF
	STA $00
	ASL
	PHA
	CLC
	ADC !Dst
	STA !Dst

	PLA
	CLC
	ADC $00
	CLC
	ADC !Source
	STA !Source

	JSL !RGBMerge
	
RTL
+
	DEC A
	TAX

	LDA $00
	AND #$00FF
	STA $00
	ASL
	PHA
	CLC
	ADC !Dst
	STA !Dst

	PLA
	CLC
	ADC $00
	CLC
	ADC !Source
	STA !Source

	AND #$00FF
	SEP #$20
	LDA !PaletteEffectsRatios1,x
	STA !ratio1

	LDA !PaletteEffectsRatios2,x
	STA !ratio2

	LDA !PaletteEffectsRatios3,x
	STA !ratio3

	LDA !PaletteEffectsChannels1,x
	STA !V1

	LDA !PaletteEffectsChannels2,x
	STA !V2

	LDA !PaletteEffectsChannels3,x
	STA !V3

	LDA !PaletteEffectsTypes,x
	SEP #$10
	ASL
	TAX

	JSR (.Effects,x)

RTL

.Effects
	dw .MixRGB
	dw .MixHSL
	dw .MixRG
	dw .MixHS
	dw .MixRB
	dw .MixHL
	dw .MixGB
	dw .MixSL
	dw .MixR
	dw .MixH
	dw .MixG
	dw .MixS
	dw .MixB
	dw .MixL
	dw .PalTransitionRGB
	dw .PalTransitionHSL
	dw .PalFunctionRGB
	dw .PalFunctionHSL

.MixRGB
	JSL !MixAndMergeRGB
RTS

.MixHSL
	JSL !MixAndMergeHSL
RTS

.MixRG
	JSL !MixAndMergeRG
RTS

.MixHS
	JSL !MixAndMergeHS
RTS

.MixRB
	JSL !MixAndMergeRB
RTS

.MixHL
	JSL !MixAndMergeHL
RTS

.MixGB
	JSL !MixAndMergeGB
RTS

.MixSL
	JSL !MixAndMergeSL
RTS

.MixR
	JSL !MixAndMergeR
RTS

.MixH
	JSL !MixAndMergeH
RTS

.MixG
	JSL !MixAndMergeG
RTS

.MixS
	JSL !MixAndMergeS
RTS

.MixB
	JSL !MixAndMergeB
RTS

.MixL
	JSL !MixAndMergeL
RTS

.PalTransitionRGB
	LDA !V1
	STA !Target
	LDA !V2
	STA !Target+1
	LDA !V3
	STA !Target+2
	JSL !PalTransitionAndMergeRGB
RTS

.PalTransitionHSL
	LDA !V1
	STA !Target
	LDA !V2
	STA !Target+1
	LDA !V3
	STA !Target+2
	JSL !PalTransitionAndMergeHSL
RTS

.PalFunctionRGB
	JSL !PalFunctionAndMergeRGB
RTS

.PalFunctionHSL
	JSL !PalFunctionAndMergeHSL
RTS

;Input:
;	A = Effect ID, 16 bits
;   $00 = Color ID (PPPP CCCC)
;       PPPP : Palette
;       CCCC : Color on the Palette
;	$45 = Source Buffer address (3 bytes per color),24 bits
;	$48 = Destination Buffer Address (3 bytes per color), 24 bits
;	$4F = Number of colors to change, 8 bits.
;Output:
;	Result of color effect in Destinarion buffer
DoEffect:
	BNE +

	LDA $00
	AND #$00FF
	STA $00
	ASL
	CLC
	ADC $00
	PHA
	CLC
	ADC !Dst
	STA !Dst

	PLA
	CLC
	ADC !Source
	STA !Source

	LDA $4F
	AND #$00FF
	STA $4F
	ASL
	CLC
	ADC $4F
	STA $4F
	REP #$10
	LDY #$0000
-
	LDA [!Source],y
	STA [!Dst],y
	INY
	LDA [!Source],y
	STA [!Dst],y
	INY
	INY
	CPY $4F
	BCC -
	SEP #$30
RTL
+
	DEC A
	TAX

	LDA $00
	AND #$00FF
	STA $00
	ASL
	CLC
	ADC $00
	PHA
	CLC
	ADC !Dst
	STA !Dst

	PLA
	CLC
	ADC !Source
	STA !Source

	AND #$00FF
	SEP #$20
	LDA !PaletteEffectsRatios1,x
	STA !ratio1

	LDA !PaletteEffectsRatios2,x
	STA !ratio2

	LDA !PaletteEffectsRatios3,x
	STA !ratio3

	LDA !PaletteEffectsChannels1,x
	STA !V1

	LDA !PaletteEffectsChannels2,x
	STA !V2

	LDA !PaletteEffectsChannels3,x
	STA !V3

	LDA !PaletteEffectsTypes,x
	SEP #$10
	ASL
	TAX

	JSR (.Effects,x)

RTL

.Effects
	dw .MixRGB
	dw .MixHSL
	dw .MixRG
	dw .MixHS
	dw .MixRB
	dw .MixHL
	dw .MixGB
	dw .MixSL
	dw .MixR
	dw .MixH
	dw .MixG
	dw .MixS
	dw .MixB
	dw .MixL
	dw .PalTransitionRGB
	dw .PalTransitionHSL
	dw .PalFunctionRGB
	dw .PalFunctionHSL

.MixRGB
	JSL !MixRGB
RTS

.MixHSL
	JSL !MixHSL
RTS

.MixRG
	JSL !MixRG
RTS

.MixHS
	JSL !MixHS
RTS

.MixRB
	JSL !MixRB
RTS

.MixHL
	JSL !MixHL
RTS

.MixGB
	JSL !MixGB
RTS

.MixSL
	JSL !MixSL
RTS

.MixR
	JSL !MixR
RTS

.MixH
	JSL !MixH
RTS

.MixG
	JSL !MixG
RTS

.MixS
	JSL !MixS
RTS

.MixB
	JSL !MixB
RTS

.MixL
	JSL !MixL
RTS

.PalTransitionRGB
	LDA !V1
	STA !Target
	LDA !V2
	STA !Target+1
	LDA !V3
	STA !Target+2
	JSL !PalTransitionRGB
RTS

.PalTransitionHSL
	LDA !V1
	STA !Target
	LDA !V2
	STA !Target+1
	LDA !V3
	STA !Target+2
	JSL !PalTransitionHSL
RTS

.PalFunctionRGB
	JSL !PalFunctionRGB
RTS

.PalFunctionHSL
	JSL !PalFunctionHSL
RTS
