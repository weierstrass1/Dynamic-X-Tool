macro Draw(PoseID, FlipAndObjectPriority, Palette, XOffset, YOffset, MaxTilePriority)
    LDA !Property
    ORA <FlipAndObjectPriority>
    ORA <Palette>
    STA !Property

    REP #$20
    LDA <XOffset>
    STA !XOffSet

    LDA <YOffset>
    STA !YOffSet

    LDA <PoseID>
    STA !PoseID
    SEP #$20

    LDA <MaxTilePriority>
    STA !MaxTilePriority

    JSL !Draw
endmacro

macro DrawWithoutOffset(PoseID, FlipAndObjectPriority, Palette, MaxTilePriority)
    LDA !Property
    ORA <FlipAndObjectPriority>
    ORA <Palette>
    STA !Property

    REP #$20
    LDA <PoseID>
    STA !PoseID
    SEP #$20

    LDA <MaxTilePriority>
    STA !MaxTilePriority

    JSL !Draw
endmacro

macro DrawNormalSprite(PoseID, FlipAndObjectPriority, Palette, MaxTilePriority)

    LDA !sprite_x_high,x
    XBA
    LDA !sprite_x_low,x
    REP #$20
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    LDA !sprite_y_high,x
    XBA
    LDA !sprite_y_low,x
    REP #$20
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

    %DrawWithoutOffset("<PoseID>", "<FlipAndObjectPriority>", "<Palette>", "<MaxTilePriority>")
endmacro

macro DrawNormalSpriteWithOffset(PoseID, FlipAndObjectPriority, Palette, XOffset, YOffset, MaxTilePriority)
    LDA !sprite_x_high,x
    XBA
    LDA !sprite_x_low,x
    REP #$20
    CLC
    ADC <XOffset>
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    LDA !sprite_y_high,x
    XBA
    LDA !sprite_y_low,x
    REP #$20
    CLC
    ADC <YOffset>
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

    %DrawWithoutOffset("<PoseID>", "<FlipAndObjectPriority>", "<Palette>", "<MaxTilePriority>")
endmacro

macro DrawClusterSprite(PoseID, FlipAndObjectPriority, Palette, MaxTilePriority)

    LDA !cluster_x_high,x
    XBA
    LDA !cluster_x_low,x
    REP #$20
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    LDA !cluster_y_high,x
    XBA
    LDA !cluster_y_low,x
    REP #$20
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

    %DrawWithoutOffset("<PoseID>", "<FlipAndObjectPriority>", "<Palette>", "<MaxTilePriority>")
endmacro

macro DrawClusterSpriteWithOffset(PoseID, FlipAndObjectPriority, Palette, XOffset, YOffset, MaxTilePriority)
    LDA !cluster_x_high,x
    XBA
    LDA !cluster_x_low,x
    REP #$20
    CLC
    ADC <XOffset>
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    LDA !cluster_y_high,x
    XBA
    LDA !cluster_y_low,x
    REP #$20
    CLC
    ADC <YOffset>
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

    %DrawWithoutOffset("<PoseID>", "<FlipAndObjectPriority>", "<Palette>", "<MaxTilePriority>")
endmacro

macro DrawExtendedSprite(PoseID, FlipAndObjectPriority, Palette, MaxTilePriority)

    LDA !extended_x_high,x
    XBA
    LDA !extended_x_low,x
    REP #$20
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    LDA !extended_y_high,x
    XBA
    LDA !extended_y_low,x
    REP #$20
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

    %DrawWithoutOffset("<PoseID>", "<FlipAndObjectPriority>", "<Palette>", "<MaxTilePriority>")
endmacro

macro DrawExtendedSpriteWithOffset(PoseID, FlipAndObjectPriority, Palette, XOffset, YOffset, MaxTilePriority)
    LDA !extended_x_high,x
    XBA
    LDA !extended_x_low,x
    REP #$20
    CLC
    ADC <XOffset>
    SEC
    SBC $1A
    STA !XOffSet
    SEP #$20

    LDA !extended_y_high,x
    XBA
    LDA !extended_y_low,x
    REP #$20
    CLC
    ADC <YOffset>
    SEC
    SBC $1C
    STA !YOffSet
    SEP #$20

    %DrawWithoutOffset("<PoseID>", "<FlipAndObjectPriority>", "<Palette>", "<MaxTilePriority>")
endmacro