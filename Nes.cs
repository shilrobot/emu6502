using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class Nes
    {
        public Rom Rom;
        public Chip6502 Cpu;
        public NesMemory Mem;

        public Nes(string romfile)
        {
            Rom = new Rom(romfile);
            Mem = new NesMemory(Rom);
            Cpu = new Chip6502(Mem);
        }
    }
}
