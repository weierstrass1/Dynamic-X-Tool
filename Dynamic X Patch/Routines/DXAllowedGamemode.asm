?DXAllowedGameMode:
    PHX
    LDA $0100|!addr
    TAX
    LDA.l ?.GameModeAllowed,x
    PLX
    CMP #$00
RTL

?.GameModeAllowed
    ;   00  01  02  03  04  05  06  07
    db $00,$00,$00,$00,$00,$00,$00,$01
    ;   08  09  0A  0B  0C  0D  0E  0F
    db $00,$00,$00,$00,$00,$00,$01,$00
    ;   10  11  12  13  14  15  16  17
    db $00,$00,$00,$00,$01,$00,$00,$00
    ;   18  19  1A  1B  1C  1D  1E  1F
    db $00,$00,$00,$01,$00,$00,$00,$01
    ;   20  21  22  23  24  25  26  27
    db $00,$00,$00,$00,$00,$01,$00,$00
    ;   28  29  2A  2B  2C  2D  2E  2F
    db $00,$01,$00,$00,$00,$00,$00,$00