;--------------------------------------------------
;------------------Unsigned Mul--------------------
;--------------------------------------------------
macro MulAfterMul(b)
if !sa1
    STZ $2252
    LDA<b>
    STA $2253

    LDA !MultiplicationResult
    STA $2251

    STZ $2254
else
    NOP : NOP : NOP : NOP
    LDA !MultiplicationResult
    STA $4202

    LDA<b>
    STA $4203
endif
endmacro

macro Mul(a, b)
if !sa1
    STZ $2250
    STZ $2252

    LDA<b>
    STA $2253

    LDA<a>
    STA $2251

    STZ $2254
else
    LDA<a>
    STA $4202

    LDA<b>
    STA $4203
endif
endmacro

macro MulAfterDiv(b)
if !sa1 == 0
    NOP : NOP : NOP : NOP
    NOP : NOP : NOP : NOP
endif
    %Mul(" !DivisionResult", " <b>")
endmacro

macro MulW(a, b)
    %Mul(" <a>"," <b>")
if !sa1
    NOP
    JMP ?+
?+
else
    NOP : NOP : NOP : NOP
endif
endmacro

macro MulWAfterMul(b)
    %MulAfterMul(" <b>")
if !sa1
    NOP
    JMP ?+
?+
else
    NOP : NOP : NOP : NOP
endif
endmacro

macro MulWAfterDiv(b)
    %MulAfterDiv(" <b>")
if !sa1
    NOP
    JMP ?+
?+
else
    NOP : NOP : NOP : NOP
endif
endmacro

;--------------------------------------------------
;------------------Unsigned Div--------------------
;--------------------------------------------------
macro Div(ah, al, b)
if !sa1
    LDA #$01
    STA $2250
    LDA<b>
    STA $2253
    
    LDA<al>
    STA $2251
    LDA<ah>
    STA $2252

    STZ $2254
else
    LDA<al>
    STA $4204
    LDA<ah>
    STA $4205

    LDA<b>
    STA $4206
endif
endmacro

macro DivAfterMul(b)
if !sa1 == 0
    NOP : NOP : NOP : NOP
endif
    %Div(" !MultiplicationResult+1", " !MultiplicationResult", " <b>")
endmacro

macro DivAfterDiv(b)
if !sa1

    LDA<b>
    STA $2253
    
    LDA !DivisionResult
    STA $2251
    LDA !DivisionResult+1
    STA $2252

    STZ $2254
else
    NOP : NOP : NOP : NOP
    NOP : NOP : NOP : NOP

    LDA !DivisionResult
    STA $4204
    LDA !DivisionResult+1
    STA $4205

    LDA<b>
    STA $4206
endif
endmacro

macro DivW(ah, al, b)
    %Div(" <ah>"," <al>"," <b>")
if !sa1
    NOP
    JMP ?+
?+
else
    NOP : NOP : NOP : NOP
    NOP : NOP : NOP : NOP
endif
endmacro

macro DivWAfterMul(b)
    %DivAfterMul(" <b>")
if !sa1
    NOP
    JMP ?+
?+
else
    NOP : NOP : NOP : NOP
    NOP : NOP : NOP : NOP
endif
endmacro

macro DivWAfterDiv(b)
    %DivAfterDiv(" <b>")
if !sa1
    NOP
    JMP ?+
?+
else
    NOP : NOP : NOP : NOP
    NOP : NOP : NOP : NOP
endif
endmacro