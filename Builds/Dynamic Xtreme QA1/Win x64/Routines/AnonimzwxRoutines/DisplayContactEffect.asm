!ContactEffect = $00
!UseDefaultStarEffect = 1 ;0 = DKC effect, 1 = smw effect

?DisplayContactEffect:
if !UseDefaultStarEffect
    BEQ ?+
RTL
else
    BEQ ?+

    LDA !PlayerFlashingTimer
    BEQ ?+
RTL
endif
?+
    LDA $71
    CMP #$09
    BNE ?+
RTL
?+
    LDA $09
    STA !Scratch46
    LDA $01
    STA !Scratch45  ;$45 = yoffset 16 bits player

    LDA $0B
    STA !Scratch48
    LDA $05
    STA !Scratch47  ;$47 = yoffset 16 bits sprite

    REP #$20
    LDA $00
    PHA
    LDA $02
    PHA 
    LDA $04
    PHA

    LDA $03
    AND #$00FF
    CLC
    ADC !Scratch45  
    STA !Scratch49  ;$49 = yoffset ?+ height 16 bits player

    LDA $07
    AND #$00FF
    CLC
    ADC !Scratch47
    STA !Scratch4B  ;$4B = yoffset ?+ height 16 bits sprite

    LDA !Scratch4B
    CMP !Scratch49
    BCC ?+
    LDA !Scratch49
?+                   
    STA !Scratch4D  ;$4D = Min yoffset ?+ height 16 bits sprite
    
    LDA !Scratch47
    CMP !Scratch45
    BCS ?+
    LDA !Scratch45
?+
    CLC
    ADC !Scratch4D
    LSR
    PHA  ;$01,s = Y Center of Collision Area 16 bits 

    SEP #$20

    LDA $08
    STA !Scratch46
    LDA $00
    STA !Scratch45  ;$45 = xoffset 16 bits player

    LDA $0A
    STA !Scratch48
    LDA $04
    STA !Scratch47  ;$47 = xoffset 16 bits sprite

    REP #$20
    LDA $02
    AND #$00FF
    CLC
    ADC !Scratch45  
    STA !Scratch49  ;$49 = xoffset ?+ width 16 bits player

    LDA $06
    AND #$00FF
    CLC
    ADC !Scratch47
    STA !Scratch4B  ;$4B = xoffset ?+ width 16 bits sprite

    LDA !Scratch47
    CMP !Scratch45
    BCS ?+
    LDA !Scratch45
?+
    STA !Scratch4D  ;$4D = Max xoffset 16 bits

    LDA !Scratch4B
    CMP !Scratch49
    BCC ?+
    LDA !Scratch49
?+                   ;A = Min Xoffset ?+ width 16 bits sprite

if !UseDefaultStarEffect
    CLC
    ADC !Scratch4D
    LSR
    SEC
    SBC #$0108
    STA $04  ;$4D = Center of Collision Area 16 bits 

    PLA
    SEC
    SBC #$0108
    STA $06
    SEP #$20

    STZ $00
    STZ $01
    STZ $03

    LDA #$0C
    STA $02
    LDA #$02
    %SpawnSmokeGeneric()

    REP #$20
    PLA
    STA $04
    PLA 
    STA $02
    PLA
    STA $00
    SEP #$20
RTL
else
    CLC
    ADC !Scratch4D
    LSR
    SEC
    SBC #$0108
    STA $00  ;$4D = Center of Collision Area 16 bits 

    PLA
    SEC
    SBC #$0108
    STA $02
    SEP #$20

    STZ !Scratch4
    STZ !Scratch5

    LDA #!ContactEffect+!ClusterOffset
    %AnonimzwxRoutinesSpawnCluster()
    BCC ?+

    LDA #$00
    STA !ClusterMiscTable1,y

?+
    REP #$20
    PLA
    STA $04
    PLA 
    STA $02
    PLA
    STA $00
    SEP #$20
RTL
endif