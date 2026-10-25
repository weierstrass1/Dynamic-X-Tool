if !KevinLMVRAMOptimization
if (((read1($0FF0B4)-'0')*100)+((read1($0FF0B6)-'0')*10)+(read1($0FF0B7)-'0')) > 370
    error "You don't need this patch, because Lunar Magic 3.70 implemented this optimization!"
endif

org $008751
    rep #$20
    lda $03
endif
