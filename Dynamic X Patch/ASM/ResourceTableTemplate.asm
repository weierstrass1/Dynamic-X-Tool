if read1($00FFD5) == $23
    fullsa1rom
else
    lorom
endif

org (read3($00821F))+$<Offset>
    dl Table

freedata cleaned
Table:
<Values>    dl $FFFFFF
print dec(snestopc(Table))