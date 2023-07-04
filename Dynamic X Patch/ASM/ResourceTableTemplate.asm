if read1($00FFD5) == $23
    sa1rom
endif

org (read1($0082DE)<<16+read2($008241))+$<Offset>
    dl Table

freedata cleaned
Table:
<Values>    dl $FFFFFF
print dec(snestopc(Table))