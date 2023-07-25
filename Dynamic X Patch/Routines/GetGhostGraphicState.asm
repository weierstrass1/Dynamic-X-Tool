?GetGhostGraphicState:
    PHA
    %GetGhostState()
    BIT #$04
    BNE ?.Draw
    BIT #$02
    BNE ?.Flash
?.noDraw
    PLA
?.noDraw2
    CLC
RTL
?.Flash
    PLA
    CLC 
    ADC $13
    AND #$01
    BEQ ?.noDraw2
    SEC
RTL
?.Draw
    PLA
    SEC
RTL