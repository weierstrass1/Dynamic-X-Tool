namespace DynamicXSNES
{
    public enum Mapper { 
		NoROM,
		LoROM,
		HiROM,
		SA1ROM,
		BigSA1ROM,
		ExLoROM,
		ExHiROM, 
		LoROMFastROM, 
		HiROMFastROM,
		SDD1ROM,
		ExLoROMFastROM,
		ExHiROMFastROM,
		SFXROM
	};
    public class SNESROMUtils
    {
		public const int HEADER_SIZE = 0x200;
		public static Mapper GetMapper(byte[] rom)
		{
			switch(rom[0x000081D5])
			{
				case 0x20:
					return Mapper.LoROM;
				case 0x30:
					return Mapper.LoROMFastROM;
				case 0x23:
                    if (rom[0x000081D7] == 0x0D)
                        return Mapper.BigSA1ROM;
                    return Mapper.SA1ROM;
				case 0x31:
                    return Mapper.HiROMFastROM;
            }
			if (rom[0x000101D5] == 0x21)
				return Mapper.HiROM;
			if (rom[0x000101D5] == 0x31)
				return Mapper.HiROMFastROM;
			if(rom.Length <= 0x004081D5)
                return Mapper.NoROM;
            return rom[0x004081D5] switch
            {
                0x24 => Mapper.ExLoROM,
                0x34 => Mapper.ExLoROMFastROM,
                0x25 => Mapper.ExHiROM,
                0x35 => Mapper.ExHiROMFastROM,
                _ => Mapper.NoROM,
            };
		}
		private static readonly int[] sa1Banks = { 0, 1 << 20, -1, -1, 2 << 20, 3 << 20, -1, -1 };
        public static int PCtoSNES(int addr, Mapper mapper)
		{
			if (addr < 0) 
				return -1;

			addr -= HEADER_SIZE;
			switch(mapper)
            {
				case Mapper.NoROM:
					return addr;
				case Mapper.LoROMFastROM:
				case Mapper.LoROM:
					if (addr >= 0x400000) return -1;
					addr = ((addr << 1) & 0x7F0000) | (addr & 0x7FFF) | 0x8000;
					return addr | 0x800000;
				case Mapper.HiROMFastROM:
				case Mapper.HiROM:
					if (addr >= 0x400000) return -1;
					return addr | 0xC00000;
				case Mapper.ExLoROMFastROM:
				case Mapper.ExLoROM:
					if (addr >= 0x800000) return -1;
					if ((addr & 0x400000) == 0x400000)
					{
						addr -= 0x400000;
						addr = ((addr << 1) & 0x7F0000) | (addr & 0x7FFF) | 0x8000;
						return addr;
					}
					addr = ((addr << 1) & 0x7F0000) | (addr & 0x7FFF) | 0x8000;
					return addr | 0x800000;
				case Mapper.ExHiROMFastROM:
				case Mapper.ExHiROM:
					if (addr >= 0x800000) return -1;
					if ((addr & 0x400000) == 0x400000) return addr;
					return addr | 0xC00000;
				case Mapper.SA1ROM:
					for (int i = 0; i < 8; i++)
					{
						if (sa1Banks[i] == (addr & 0x700000))
							return 0x008000 | (i << 21) | ((addr & 0x0F8000) << 1) | (addr & 0x7FFF); 
					}
					return -1;
				case Mapper.BigSA1ROM:
					if (addr >= 0x800000) return -1;
					if ((addr & 0x400000) == 0x400000)
					{
						return addr | 0xC00000;
					}
					if ((addr & 0x600000) == 0x000000)
					{
						return ((addr << 1) & 0x3F0000) | 0x8000 | (addr & 0x7FFF);
					}
					if ((addr & 0x600000) == 0x200000)
					{
						return 0x800000 | ((addr << 1) & 0x3F0000) | 0x8000 | (addr & 0x7FFF);
					}
					return -1;
				case Mapper.SFXROM:
					if (addr >= 0x200000) 
						return -1;
					return ((addr << 1) & 0x7F0000) | (addr & 0x7FFF) | 0x8000;
				default:
					return -1;
			}
		}
		public static int Join3(byte[] rom, int address, Mapper m)
        {
			int snes = JoinAddress(rom[address + 2], rom[address + 1], rom[address]);
			return SNEStoPC(snes, m);
		}
		public static int JoinAddress(byte[] rom, int address)
			=> JoinAddress(rom[address + 2], rom[address + 1], rom[address]);

        public static int JoinAddress(byte bnk, byte hb, byte lb)
        {
			return ((bnk << 16) + (hb << 8) + lb) & 0x00FFFFFF;
        }
		public static int RemoveAt(byte[] rom, int address)
		{
			if (address + 8 >= rom.Length)
				return 0;
			if (rom[address] != 0x53 || rom[address + 1] != 0x54 ||
				rom[address + 2] != 0x41 || rom[address + 3] != 0x52)
					return 0;
			int l = (rom[address + 5] << 8) + rom[address + 4];
			if (address + 8 + l >= rom.Length)
				return 0;
			if (rom[address + 6] != (rom[address + 4] ^ 0xFF) ||
				rom[address + 7] != (rom[address + 5] ^ 0xFF))
				return 0;
			l += 8 + address;
			for (int i = address; i < l; i++)
            {
				rom[i] = 0;
			}
			return l - address;
		}
		public static int SNEStoPC(int addr, Mapper mapper)
		{
			if (addr < 0 || addr > 0xFFFFFF) return -1;//not 24bit
			switch(mapper)
            {
				case Mapper.NoROM:
					return addr + HEADER_SIZE;
				case Mapper.LoROMFastROM:
				case Mapper.LoROM:
					// randomdude999: The low pages ($0000-$7FFF) of banks 70-7D are used
					// for SRAM, the high pages are available for ROM data though
					if ((addr & 0xFE0000) == 0x7E0000 ||//wram
						(addr & 0x408000) == 0x000000 ||//hardware regs, ram mirrors, other strange junk
						(addr & 0x708000) == 0x700000)//sram (low parts of banks 70-7D)
						return -1;
					addr = ((addr & 0x7F0000) >> 1 | (addr & 0x7FFF));
					return addr + HEADER_SIZE;
				case Mapper.HiROMFastROM:
				case Mapper.HiROM:
					if ((addr & 0xFE0000) == 0x7E0000 ||//wram
							(addr & 0x408000) == 0x000000)//hardware regs, ram mirrors, other strange junk
						return -1;
					return (addr + HEADER_SIZE) & 0x3FFFFF;
				case Mapper.ExLoROMFastROM:
				case Mapper.ExLoROM:
					if ((addr & 0xF00000) == 0x700000 ||//wram, sram
						(addr & 0x408000) == 0x000000)//area that shouldn't be used in lorom
						return -1;
					if ((addr & 0x800000) == 0x800000)
					{
						addr = ((addr & 0x7F0000) >> 1 | (addr & 0x7FFF));
					}
					else
					{
						addr = ((addr & 0x7F0000) >> 1 | (addr & 0x7FFF)) + 0x400000;
					}
					return addr + HEADER_SIZE;
				case Mapper.ExHiROMFastROM:
				case Mapper.ExHiROM:
					if ((addr & 0xFE0000) == 0x7E0000 ||//wram
						(addr & 0x408000) == 0x000000)//hardware regs, ram mirrors, other strange junk
						return -1;
					if ((addr & 0x800000) == 0x000000) return (addr & 0x3FFFFF) | 0x400000;
					return (addr & 0x3FFFFF) + HEADER_SIZE;
				case Mapper.SA1ROM:
					if ((addr & 0x408000) == 0x008000)
					{
						addr = sa1Banks[(addr & 0xE00000) >> 21] | ((addr & 0x1F0000) >> 1) | (addr & 0x007FFF);
						return addr + HEADER_SIZE;
					}
					if ((addr & 0xC00000) == 0xC00000)
					{
                        addr = sa1Banks[((addr & 0x100000) >> 20) | ((addr & 0x200000) >> 19)] | (addr & 0x0FFFFF);
                        return addr + HEADER_SIZE;
                    }
					return -1;
				case Mapper.BigSA1ROM:
					if ((addr & 0xC00000) == 0xC00000)//hirom
					{
						addr = (addr & 0x3FFFFF) | 0x400000;
						return addr + HEADER_SIZE;
					}
					if ((addr & 0xC00000) == 0x000000 || (addr & 0xC00000) == 0x800000)//lorom
					{
						if ((addr & 0x008000) == 0x000000) return -1;

						addr = (addr & 0x800000) >> 2 | (addr & 0x3F0000) >> 1 | (addr & 0x7FFF);
						return HEADER_SIZE + addr;

                    }
					return -1;
				case Mapper.SFXROM:
					// Asar emulates GSU1, because apparently emulators don't support the extra ROM data from GSU2
					if ((addr & 0x600000) == 0x600000 ||//wram, sram, open bus
						(addr & 0x408000) == 0x000000 ||//hardware regs, ram mirrors, rom mirrors, other strange junk
						(addr & 0x800000) == 0x800000)//fastrom isn't valid either in superfx
						return -1;
					addr = ((addr & 0x400000) == 0x400000) ?
								addr & 0x3FFFFF :
								(addr & 0x7F0000) >> 1 | (addr & 0x7FFF);
					return addr + HEADER_SIZE;
				default:
					return -1;
			}
		}
		public static List<(int, int)> FindFreeSpace(byte[] rom)
        {
            List < (int, int)> map = new();

            int startingSpace = 0;
            bool free = false;
            int freeCounter = 0;

            int lowerLimit = 17 * 32768;
            int delta;

            for (int i = 0; i < rom.Length;) 
            {
                if (rom.Length - 1 - i > 8 &&
                    rom[i] == 0x53 && rom[i + 1] == 0x54 &&
                    rom[i + 2] == 0x41 && rom[i + 3] == 0x52 &&
                    rom[i + 4] == (byte)(rom[i + 6] ^ 0xFF) &&
                    rom[i + 5] == (byte)(rom[i + 7] ^ 0xFF))
                {
                    if (free)
                    {
                        free = false;
                        if (startingSpace >= lowerLimit)
                            map.Add((startingSpace, freeCounter));
                    }
                    i += ((rom[i + 5] * 256) + rom[i + 4] + 9);
                }
                else
                {
                    if (!free)
                    {
                        free = true;
                        startingSpace = i;
                        freeCounter = 0;
                    }
                    freeCounter++;
                    i++;
                }
                if (free && (i - 512) / 32768 > (startingSpace - 512) / 32768) 
                {
                    delta = (((i - 512) / 32768) * 32768) + 512 - startingSpace;
                    if (delta > 0 && freeCounter > delta)
                    {
                        if (startingSpace >= lowerLimit)
                            map.Add((startingSpace, delta));
                        freeCounter -= delta;
                        startingSpace += delta;
                    }
                }
            }
            return map;
        }
		public static byte[] GetRATS(byte[] data)
        {
			byte[] rats = new byte[8];
			rats[0] = 0x53;										//S
			rats[1] = 0x54;										//T
			rats[2] = 0x41;										//A
			rats[3] = 0x52;										//R
			rats[4] = (byte)((data.Length - 1) & 0xFF);			//size-1 low byte
			rats[5] = (byte)(((data.Length - 1) / 256) & 0xFF);	//size-1 high byte
			rats[6] = (byte)(rats[4] ^ 0xFF);					//~(size-1 low byte)
			rats[7] = (byte)(rats[5] ^ 0xFF);					//~(size-1 hight byte)
			return rats;
        }
        public static void InsertDataWithRats(byte[] rom, int address , byte[] data)
        {
			GetRATS(data).CopyTo(rom, address);
			data.CopyTo(rom, address + 8);                              //data
        }
		public static (List<(int, int, int)>, List<byte[]>) MergeResources(List<byte[]> resources)
		{
			List<(int, byte[])> positions = new();

			int i = 0;
			foreach (var item in resources)
			{
				positions.Add((i, item));
				i++;
			}
			positions.Sort((x1, x2) => x1.Item2.Length < x2.Item2.Length ? 1 :
										x1.Item2.Length > x2.Item2.Length ? -1 :
										0);
			byte[] resBuff;
			List<byte[]> currentbuffer;
			List<(int, int, int)> orderAndPosition = new();
			List<byte[]> result = new();
			int size;
			bool find;
			int buffIndex = 0;
			int counter;
			while(positions.Count > 0)
			{
				size = 0;
                currentbuffer = new();
				find = true;
				while(find)
				{
					find = false;
					foreach(var buff in positions)
					{
						if(buff.Item2.Length + size <= 32760)
						{
							find = true;
							currentbuffer.Add(buff.Item2);
							orderAndPosition.Add((buff.Item1, buffIndex, size));
							size += buff.Item2.Length;
							positions.Remove(buff);
                            break;
                        }
					}	
				}
                resBuff = new byte[size];
				counter = 0;
                foreach (var buff in currentbuffer)
				{
					buff.CopyTo(resBuff, counter);
					counter += buff.Length;
                }
				result.Add(resBuff);
                buffIndex++;
            }

			return (orderAndPosition, result);
		}
    }
}
