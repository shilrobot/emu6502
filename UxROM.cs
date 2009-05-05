using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class UxROM : Mapper
    {
        byte[] PrgRomBank0;
        byte[] PrgRomBank1;

        public UxROM(Nes nes)
            : base(nes)
        {
        }

        public override void  Reset()
        {
            //nes.Ppu.PatternTables = new byte[8 * 1024];// nes.Rom.ChrRomBanks[0];
            // TODO: What default?
            PrgRomBank0 = nes.Rom.PrgRomBanks[0];
            PrgRomBank1 = nes.Rom.PrgRomBanks[nes.Rom.PrgRomBanks.Length - 1];
        }

        public override byte PrgRomRead(int addr)
        {
            if (addr >= 0x8000 && addr < 0xC000)
                return PrgRomBank0[addr & 0x3FFF];
            else
                return PrgRomBank1[addr & 0x3FFF];
        }

        public override void PrgRomWrite(int addr, byte data)
        {
            int newBank = data % nes.Rom.PrgRomBanks.Length;
            PrgRomBank0 = nes.Rom.PrgRomBanks[newBank];
        }
    }
}
