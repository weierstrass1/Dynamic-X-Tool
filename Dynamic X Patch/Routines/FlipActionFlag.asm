!SpriteActionFlag = !SpriteMiscTable2

?FlipActionFlag:

    LDA !SpriteActionFlag,x
    AND #$02
    BEQ ?+
RTL
?+
    LDA #$00
    XBA

    LDA !SpriteTweaker1686_DNCTSWYE,x
    AND #$18
    BEQ ?+
RTL
?+
    
    JSL $03B69F|!rom

    TXY
    DEY
?.loop 
    BMI ?.end
    
    LDA !SpriteStatus,y
    CMP #$08
    BNE ?.next

    LDA !SpriteTweaker1686_DNCTSWYE,y
    AND #$18
    BNE ?.next

    TYX

    JSL $03B6E5|!rom
    JSL $03B72B|!rom
    BCC ?.noContact

    LDA #$01
    XBA

    LDY !SpriteIndex

    JSR ?.CheckFlip
    BCC ?.skip1

    LDA #$06
    ORA !SpriteActionFlag,x
    STA !SpriteActionFlag,x

?.skip1
    TXY
    LDX !SpriteIndex

    JSR ?.CheckFlip
    BCC ?.skip2

    LDA #$06
    ORA !SpriteActionFlag,x
    STA !SpriteActionFlag,x
?.skip2

    DEY
    BRA ?.loop

?.noContact

    TXY
    LDX !SpriteIndex

?.next
    DEY
    BRA ?.loop
?.end

    LDA !SpriteActionFlag,x
    AND #$02
    BNE ?.return
    XBA
    BNE ?.return

    LDA !SpriteActionFlag,x
    AND #$FB
    STA !SpriteActionFlag,x

?.return
RTL

?.CheckFlip
    LDA !SpriteDecTimer5,x
    BNE ?..skip
    LDA !SpriteBlockedStatus_ASB0UDLR,x
    AND #$43
    BNE ?..skip
    LDA !SpriteActionFlag,x
    AND #$04
    BNE ?..skip
    LDA !SpriteXSpeed|!dp,y
    BEQ ?..flip
    EOR !SpriteXSpeed,x
    AND #$80
    BNE ?..flip
    LDA !SpriteXSpeed,x
    CMP !SpriteXSpeed|!dp,y
    BCS ?..skip
?..flip
    SEC
RTS
?..skip
    CLC
RTS