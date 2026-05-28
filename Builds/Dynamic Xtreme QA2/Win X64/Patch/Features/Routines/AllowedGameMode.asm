AllowedGameMode:
    PHX
    LDA $0100|!addr
    TAX
    LDA.l .GameModeAllowed,x
    PLX
    CMP #$00
RTL

.GameModeAllowed
    db $00,$00,$00,$00,$00,$00,$01,$01
    ;  g00,g01,g02,g03,g04,g05,g06,g07
    db $01,$01,$01,$00,$01,$01,$01,$00
    ;  g08,g09,g0A,g0B,g0C,g0D,g0E,g0F
    db $00,$00,$01,$01,$01,$01,$00,$00
    ;  g10,g11,g12,g13,g14,g15,g16,g17
    db $00,$00,$00,$00,$01,$01,$01,$01
    ;  g18,g19,g1A,g1B,g1C,g1D,g1E,g1F
    db $00,$00,$00,$00,$01,$01,$00,$00
    ;  g20,g21,g22,g23,g24,g25,g26,g27
    db $00,$00,$00,$00,$00,$00,$00,$00
    ;  g28,g29,g2A,g2B,g2C,g2D,g2E,g2F
