
	load_animated_data:
		asl
		ora $00
		tay
		phb : phk : plb
		lda.w AnimatedTileData,y
		plb
		jml $05BB96|!rom

	fix_yoshi:
		INX
		CPX $0D84|!addr
		BCC .not_done
			LDY.b #GFX32>>16
			STY $4324
			JML $00A36D|!rom
		.not_done
			CPX #$06
			BCC .return
				LDY.b #GFX33>>16
				STY $4324
			.return
				JML $00A355|!rom
				
	fix_yoshi2:
		INX
		CPX $0D84|!addr
		BCC .not_done
			LDY.b #GFX32>>16
			STY $4324
			JML $00A38D|!rom
		.not_done
			CPX #$06
			BCC .return
				LDY.b #GFX33>>16
				STY $4324
			.return
				JML $00A375|!rom
				
	fix_berries:
		LDY.b #GFX32>>16
		STY $4324
		LDA $0D76|!addr
		STA $4322
		RTL

; animated tile data	
AnimatedTileData:
	dw GFX33+$1800,GFX33+$1A00,GFX33+$1C00,GFX33+$1E00	; entry 00 - animated ? block
	dw GFX33+$1880,GFX33+$1A80,GFX33+$1C80,GFX33+$1E80	; entry 01 - animated note block
	dw GFX33+$1900,GFX33+$1900,GFX33+$1900,GFX33+$1900	; entry 02 - turn block
	dw GFX33+$2080,GFX33+$2280,GFX33+$2480,GFX33+$2680	; entry 03 - midway point
	dw GFX33+$1900,GFX33+$1B00,GFX33+$1D00,GFX33+$1F00	; entry 04 - animated turn block
	dw GFX33+$3000,GFX33+$3080,GFX33+$3100,GFX33+$3180	; entry 05 - berry
	dw GFX33+$2F20,GFX33+$2F20,GFX33+$2F20,GFX33+$2F20	; entry 06 - blank
	dw GFX33+$2F20,GFX33+$2F20,GFX33+$2F20,GFX33+$2F20	; entry 07 - blank
	dw GFX33+$1680,GFX33+$1680,GFX33+$1680,GFX33+$1680	; entry 08 - used block
	dw GFX33+$2700,GFX33+$2780,GFX33+$2700,GFX33+$2780	; entry 09 - muncher
	dw GFX33+$2F20,GFX33+$2F20,GFX33+$2F20,GFX33+$2F20	; entry 0A - blank
	dw GFX33+$2F00,GFX33+$2F00,GFX33+$2F00,GFX33+$2F00	; entry 0B - on/off line guide?
	dw GFX33+$1400,GFX33+$1400,GFX33+$1400,GFX33+$1400	; entry 0C - on switch
	dw GFX33+$1980,GFX33+$1B80,GFX33+$1D80,GFX33+$1F80	; entry 0D - animated coin
	dw GFX33+$2000,GFX33+$2200,GFX33+$2400,GFX33+$2600	; entry 0E - animated water (tileset index 0)
	dw GFX33+$1180,GFX33+$1380,GFX33+$1580,GFX33+$1780	; entry 0F - animated castle lava (tileset index 0)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 10 - ? block (tileset index 0)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 11 - ? block (tileset index 0)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 12 - ? block (tileset index 0)
	dw GFX33+$2000,GFX33+$2200,GFX33+$2400,GFX33+$2600	; entry 13 - animated water (tileset index 1)
	dw GFX33+$1180,GFX33+$1380,GFX33+$1580,GFX33+$1780	; entry 14 - animated castle lava (tileset index 1)
	dw GFX33+$0000,GFX33+$0200,GFX33+$0400,GFX33+$0600	; entry 15 - animated castle conveyor/escalator (tileset index 1)
	dw GFX33+$0600,GFX33+$0400,GFX33+$0200,GFX33+$0000	; entry 16 - animated castle conveyor/escalator, reverse (tileset index 1)
	dw GFX33+$2100,GFX33+$2300,GFX33+$2500,GFX33+$2300	; entry 17 - animated castle BG candle light (tileset index 1)
	dw GFX33+$2000,GFX33+$2200,GFX33+$2400,GFX33+$2600	; entry 18 - animated water (tileset index 2)
	dw GFX33+$2800,GFX33+$2A00,GFX33+$2C00,GFX33+$2E00	; entry 19 - animated rope and rope end (tileset index 2)
	dw GFX33+$2880,GFX33+$2A80,GFX33+$2C80,GFX33+$2E80	; entry 1A - animated sloped rope (tileset index 2)
	dw GFX33+$2E80,GFX33+$2C80,GFX33+$2A80,GFX33+$2880	; entry 1B - reverse animated sloped rope (tileset index 2)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 1C - ? block (tileset index 2)
	dw GFX33+$2180,GFX33+$2380,GFX33+$2580,GFX33+$2380	; entry 1D - animated cave BG sparkles (tileset index 3)
	dw GFX33+$0080,GFX33+$0280,GFX33+$0480,GFX33+$0680	; entry 1E - animated sloped cave lava (tileset index 3)
	dw GFX33+$0100,GFX33+$0300,GFX33+$0500,GFX33+$0700	; entry 1F - more animated sloped cave lava (tileset index 3)
	dw GFX33+$0180,GFX33+$0380,GFX33+$0580,GFX33+$0780	; entry 20 - animated cave lava (tileset index 3)
	dw GFX33+$0680,GFX33+$0480,GFX33+$0280,GFX33+$0080	; entry 21 - animated sloped cave lava, reverse (tileset index 3)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 22 - ? block (tileset index 4)
	dw GFX33+$2980,GFX33+$2B80,GFX33+$2D80,GFX33+$2B80	; entry 23 - animated ghost house light (tileset index 4)
	dw GFX33+$1100,GFX33+$1300,GFX33+$1500,GFX33+$1700	; entry 24 - animated seaweed (tileset index 4)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 25 - ? block (tileset index 4)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 26 - ? block (tileset index 4)
	dw GFX33+$2180,GFX33+$2380,GFX33+$2580,GFX33+$2380	; entry 27 - animated cave BG sparkles (tileset index 5)
	dw GFX33+$2900,GFX33+$2B00,GFX33+$2D00,GFX33+$2B00	; entry 28 - animated stars (tileset index 5)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 29 - ? block (tileset index 5)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 2A - ? block (tileset index 5)
	dw GFX33+$1800,GFX33+$1800,GFX33+$1800,GFX33+$1800	; entry 2B - ? block (tileset index 5)
	dw GFX33+$1480,GFX33+$1480,GFX33+$1480,GFX33+$1480	; entry 2C - P-switch door
	dw GFX33+$1980,GFX33+$1B80,GFX33+$1D80,GFX33+$1F80	; entry 2D - animated coin
	dw GFX33+$1980,GFX33+$1B80,GFX33+$1D80,GFX33+$1F80	; entry 2E - animated coin
	dw GFX33+$1980,GFX33+$1B80,GFX33+$1D80,GFX33+$1F80	; entry 2F - animated coin
	dw GFX33+$1800,GFX33+$1A00,GFX33+$1C00,GFX33+$1E00	; entry 30 - animated ? block
	dw GFX33+$2F80,GFX33+$2F80,GFX33+$2F80,GFX33+$2F80	; entry 31 - more on/off line guide?
	dw GFX33+$1600,GFX33+$1600,GFX33+$1600,GFX33+$1600	; entry 32 - off switch
	dw GFX33+$1680,GFX33+$1680,GFX33+$1680,GFX33+$1680	; entry 33 - used block
	
;	dw GFX32+$4D80,GFX32+$4F80,GFX32+$5C00,GFX32+$5C80	; entry 05 - berry
