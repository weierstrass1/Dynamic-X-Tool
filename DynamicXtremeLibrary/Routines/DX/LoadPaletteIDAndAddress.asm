;Input:
;   A = version, 8 bits
;   $0E = Palette Context ID, 16 bits
;Output:
;   $00 = Palette ID
;   $02-$04 = Palette Address
?DXLoadPaletteIDAndAddress:
    %LoadPaletteIDAndAddress()
RTL