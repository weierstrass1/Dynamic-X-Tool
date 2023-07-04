macro AddStack(val)
	REP #$20
	TSC
	CLC
	ADC #<val>
	TCS
	SEP #$20
endmacro

macro SubStack(val)
	REP #$20
	TSC
	SEC
	SBC #<val>
	TCS
	SEP #$20
endmacro

macro PushPar(par)
	LDA <par>
	PHA
endmacro

macro RTIN(n)
	%AddStack(<n>)
RTI
endmacro

macro RTSN(n)
	%AddStack(<n>)
RTS
endmacro

macro RTLN(n)
	%AddStack(<n>)
RTL
endmacro

macro StartFunction(name, n)
<name>:
	%SubStack(<n>)
endmacro

macro CallFunction(name, m)
JSR <name>

%AddStack(<m>)
endmacro

macro CallFunctionLong(name, m)
JSL <name>

%AddStack(<m>)
endmacro