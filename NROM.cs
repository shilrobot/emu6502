using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class NROM : Mapper
    {
        byte[] PrgRomBank0;
        byte[] PrgRomBank1;

        byte[] ChrRomBank0;
        byte[] ChrRomBank1;

        public NROM(Nes nes)
            : base(nes)
        {
        }

        public override void Reset()
        {
            /*
             * nes.Ppu.PatternTables = 
                (nes.Rom.ChrRomBanks.Length == 0) ? new byte[8 * 1024] : nes.Rom.ChrRomBanks[0];
            */
            if (nes.Rom.ChrRomBanks.Length != 0)
            {
                for (int i = 0; i < 0x1000; ++i)
                    nes.Ppu.PatternTables[0][i] = nes.Rom.ChrRomBanks[0][i];
                for (int i = 0; i < 0x1000; ++i)
                    nes.Ppu.PatternTables[1][i] = nes.Rom.ChrRomBanks[0][0x1000+i];
                
            }

            PrgRomBank0 = nes.Rom.PrgRomBanks[0];
            if(nes.Rom.PrgRomBanks.Length == 1)
                PrgRomBank1 = nes.Rom.PrgRomBanks[0];
            else
                PrgRomBank1 = nes.Rom.PrgRomBanks[1];
        }

        public override byte PrgRomRead(int addr)
        {
            if (addr >= 0x8000 && addr < 0xC000)
                return PrgRomBank0[addr & 0x3FFF];
            else
                return PrgRomBank1[addr & 0x3FFF];
        }
    }
}
