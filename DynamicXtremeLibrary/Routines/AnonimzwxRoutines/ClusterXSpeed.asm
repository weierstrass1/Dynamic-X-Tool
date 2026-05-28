?ClusterXSpeed:
    LDA $00    ; Load current sprite's Y speed 
    BEQ ?++                   ; If speed is 0, branch to $AC09 
    ASL                       ; \  
    ASL                       ;  |Multiply speed by 16 
    ASL                       ;  | 
    ASL                       ; /  
    CLC                       ; \  
    ADC $01                   ;  |Increase (unknown sprite table) by that value 
    STA $01                   ; /  
    PHP                       
    PHP                       
    LDY.B #$00                
    LDA $00                   ; Load current sprite's Y speed 
    LSR                       ; \  
    LSR                       ;  |Divide speed by 16 
    LSR                       ;  | 
    LSR                       ; /  
    CMP.B #$08                
    BCC ?+           
    ORA.B #$F0                
    DEY                       
?+
    PLP                       
    PHA                       
    ADC !ClusterXLow,x        ; \ Add value to current sprite's Y position 
    STA !ClusterXLow,x        ; /  
    TYA                       
    ADC !ClusterXHigh,X     
    STA !ClusterXHigh,X     
    PLA                       
    PLP                       
    ADC.B #$00                
?++ 
    STA.W $1491|!addr               
RTL                       ; Return 