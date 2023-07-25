?UpdateNormalSpriteSpeedWithGravityAndFloorCheck:
    LDA !SpriteBlockedStatus_ASB0UDLR,x	
    AND #$24
    STA !Scratch45

    LDA !SpriteXLow,x
    PHA
    LDA !SpriteXHigh,x
    PHA
    LDA !SpriteYLow,x
    PHA
    LDA !SpriteYHigh,x
    PHA

    LDA !SpriteYSpeed,x
    BMI ?+

    LDA !SpriteBlockedStatus_ASB0UDLR,x
    AND #$24
    BEQ ?+

    LDA #$20
    STA !SpriteYSpeed,x

?+
    JSL $01802A|!rom
    
    LDA !SpriteYSpeed,x
    BMI ?+

    LDA !SpriteBlockedStatus_ASB0UDLR,x	
    AND #$24
    BNE ?+

    LDA !Scratch45
    BEQ ?+

    LDA !SpriteBlockedStatus_ASB0UDLR,x	
    ORA #$24
    STA !SpriteBlockedStatus_ASB0UDLR,x	

    PLA
    STA !SpriteYHigh,x
    PLA
    STA !SpriteYLow,x
    PLA
    STA !SpriteXHigh,x
    PLA
    STA !SpriteXLow,x
    SEC
RTL
?+
    PLA
    PLA
    PLA
    PLA
    CLC
RTL