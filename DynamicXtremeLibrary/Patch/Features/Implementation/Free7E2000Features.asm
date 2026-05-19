
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
			LDY.b #Data_GFX32>>16
			STY $4324
			JML $00A36D|!rom
		.not_done
			CPX #$06
			BCC .return
				LDY.b #Data_GFX33>>16
				STY $4324
			.return
				JML $00A355|!rom
				
	fix_yoshi2:
		INX
		CPX $0D84|!addr
		BCC .not_done
			LDY.b #Data_GFX32>>16
			STY $4324
			JML $00A38D|!rom
		.not_done
			CPX #$06
			BCC .return
				LDY.b #Data_GFX33>>16
				STY $4324
			.return
				JML $00A375|!rom
				
	fix_berries:
		LDY.b #Data_GFX32>>16
		STY $4324
		LDA $0D76|!addr
		STA $4322
		RTL

; animated tile data	
AnimatedTileData:
	dw Data_GFX33+$1800,Data_GFX33+$1A00,Data_GFX33+$1C00,Data_GFX33+$1E00	; entry 00 - animated ? block
	dw Data_GFX33+$1880,Data_GFX33+$1A80,Data_GFX33+$1C80,Data_GFX33+$1E80	; entry 01 - animated note block
	dw Data_GFX33+$1900,Data_GFX33+$1900,Data_GFX33+$1900,Data_GFX33+$1900	; entry 02 - turn block
	dw Data_GFX33+$2080,Data_GFX33+$2280,Data_GFX33+$2480,Data_GFX33+$2680	; entry 03 - midway point
	dw Data_GFX33+$1900,Data_GFX33+$1B00,Data_GFX33+$1D00,Data_GFX33+$1F00	; entry 04 - animated turn block
	dw Data_GFX33+$3000,Data_GFX33+$3080,Data_GFX33+$3100,Data_GFX33+$3180	; entry 05 - berry
	dw Data_GFX33+$2F20,Data_GFX33+$2F20,Data_GFX33+$2F20,Data_GFX33+$2F20	; entry 06 - blank
	dw Data_GFX33+$2F20,Data_GFX33+$2F20,Data_GFX33+$2F20,Data_GFX33+$2F20	; entry 07 - blank
	dw Data_GFX33+$1680,Data_GFX33+$1680,Data_GFX33+$1680,Data_GFX33+$1680	; entry 08 - used block
	dw Data_GFX33+$2700,Data_GFX33+$2780,Data_GFX33+$2700,Data_GFX33+$2780	; entry 09 - muncher
	dw Data_GFX33+$2F20,Data_GFX33+$2F20,Data_GFX33+$2F20,Data_GFX33+$2F20	; entry 0A - blank
	dw Data_GFX33+$2F00,Data_GFX33+$2F00,Data_GFX33+$2F00,Data_GFX33+$2F00	; entry 0B - on/off line guide?
	dw Data_GFX33+$1400,Data_GFX33+$1400,Data_GFX33+$1400,Data_GFX33+$1400	; entry 0C - on switch
	dw Data_GFX33+$1980,Data_GFX33+$1B80,Data_GFX33+$1D80,Data_GFX33+$1F80	; entry 0D - animated coin
	dw Data_GFX33+$2000,Data_GFX33+$2200,Data_GFX33+$2400,Data_GFX33+$2600	; entry 0E - animated water (tileset index 0)
	dw Data_GFX33+$1180,Data_GFX33+$1380,Data_GFX33+$1580,Data_GFX33+$1780	; entry 0F - animated castle lava (tileset index 0)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 10 - ? block (tileset index 0)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 11 - ? block (tileset index 0)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 12 - ? block (tileset index 0)
	dw Data_GFX33+$2000,Data_GFX33+$2200,Data_GFX33+$2400,Data_GFX33+$2600	; entry 13 - animated water (tileset index 1)
	dw Data_GFX33+$1180,Data_GFX33+$1380,Data_GFX33+$1580,Data_GFX33+$1780	; entry 14 - animated castle lava (tileset index 1)
	dw Data_GFX33+$0000,Data_GFX33+$0200,Data_GFX33+$0400,Data_GFX33+$0600	; entry 15 - animated castle conveyor/escalator (tileset index 1)
	dw Data_GFX33+$0600,Data_GFX33+$0400,Data_GFX33+$0200,Data_GFX33+$0000	; entry 16 - animated castle conveyor/escalator, reverse (tileset index 1)
	dw Data_GFX33+$2100,Data_GFX33+$2300,Data_GFX33+$2500,Data_GFX33+$2300	; entry 17 - animated castle BG candle light (tileset index 1)
	dw Data_GFX33+$2000,Data_GFX33+$2200,Data_GFX33+$2400,Data_GFX33+$2600	; entry 18 - animated water (tileset index 2)
	dw Data_GFX33+$2800,Data_GFX33+$2A00,Data_GFX33+$2C00,Data_GFX33+$2E00	; entry 19 - animated rope and rope end (tileset index 2)
	dw Data_GFX33+$2880,Data_GFX33+$2A80,Data_GFX33+$2C80,Data_GFX33+$2E80	; entry 1A - animated sloped rope (tileset index 2)
	dw Data_GFX33+$2E80,Data_GFX33+$2C80,Data_GFX33+$2A80,Data_GFX33+$2880	; entry 1B - reverse animated sloped rope (tileset index 2)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 1C - ? block (tileset index 2)
	dw Data_GFX33+$2180,Data_GFX33+$2380,Data_GFX33+$2580,Data_GFX33+$2380	; entry 1D - animated cave BG sparkles (tileset index 3)
	dw Data_GFX33+$0080,Data_GFX33+$0280,Data_GFX33+$0480,Data_GFX33+$0680	; entry 1E - animated sloped cave lava (tileset index 3)
	dw Data_GFX33+$0100,Data_GFX33+$0300,Data_GFX33+$0500,Data_GFX33+$0700	; entry 1F - more animated sloped cave lava (tileset index 3)
	dw Data_GFX33+$0180,Data_GFX33+$0380,Data_GFX33+$0580,Data_GFX33+$0780	; entry 20 - animated cave lava (tileset index 3)
	dw Data_GFX33+$0680,Data_GFX33+$0480,Data_GFX33+$0280,Data_GFX33+$0080	; entry 21 - animated sloped cave lava, reverse (tileset index 3)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 22 - ? block (tileset index 4)
	dw Data_GFX33+$2980,Data_GFX33+$2B80,Data_GFX33+$2D80,Data_GFX33+$2B80	; entry 23 - animated ghost house light (tileset index 4)
	dw Data_GFX33+$1100,Data_GFX33+$1300,Data_GFX33+$1500,Data_GFX33+$1700	; entry 24 - animated seaweed (tileset index 4)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 25 - ? block (tileset index 4)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 26 - ? block (tileset index 4)
	dw Data_GFX33+$2180,Data_GFX33+$2380,Data_GFX33+$2580,Data_GFX33+$2380	; entry 27 - animated cave BG sparkles (tileset index 5)
	dw Data_GFX33+$2900,Data_GFX33+$2B00,Data_GFX33+$2D00,Data_GFX33+$2B00	; entry 28 - animated stars (tileset index 5)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 29 - ? block (tileset index 5)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 2A - ? block (tileset index 5)
	dw Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800,Data_GFX33+$1800	; entry 2B - ? block (tileset index 5)
	dw Data_GFX33+$1480,Data_GFX33+$1480,Data_GFX33+$1480,Data_GFX33+$1480	; entry 2C - P-switch door
	dw Data_GFX33+$1980,Data_GFX33+$1B80,Data_GFX33+$1D80,Data_GFX33+$1F80	; entry 2D - animated coin
	dw Data_GFX33+$1980,Data_GFX33+$1B80,Data_GFX33+$1D80,Data_GFX33+$1F80	; entry 2E - animated coin
	dw Data_GFX33+$1980,Data_GFX33+$1B80,Data_GFX33+$1D80,Data_GFX33+$1F80	; entry 2F - animated coin
	dw Data_GFX33+$1800,Data_GFX33+$1A00,Data_GFX33+$1C00,Data_GFX33+$1E00	; entry 30 - animated ? block
	dw Data_GFX33+$2F80,Data_GFX33+$2F80,Data_GFX33+$2F80,Data_GFX33+$2F80	; entry 31 - more on/off line guide?
	dw Data_GFX33+$1600,Data_GFX33+$1600,Data_GFX33+$1600,Data_GFX33+$1600	; entry 32 - off switch
	dw Data_GFX33+$1680,Data_GFX33+$1680,Data_GFX33+$1680,Data_GFX33+$1680	; entry 33 - used block
	
;	dw Data_GFX32+$4D80,Data_GFX32+$4F80,Data_GFX32+$5C00,Data_GFX32+$5C80	; entry 05 - berry
