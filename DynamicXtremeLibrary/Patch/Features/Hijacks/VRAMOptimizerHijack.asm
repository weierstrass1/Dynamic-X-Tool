; Remove Lunar Magic's hijack
; This is what usually clobbers NMI
org $008751
    rep #$20
    lda $03
