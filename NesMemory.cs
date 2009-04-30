using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    // TODO: Separate mapper system
    public class NesMemory : IMemory
    {
        private byte[] RAM = new byte[2048];
        private Rom rom;

        public NesMemory(Rom rom)
        {
            this.rom = rom;
        }

        public byte Read(int addr)
        {
            addr = addr & 0xFFFF;

            //Console.WriteLine("Read ${0:X4}", addr);

            if (addr < 0x2000)
            {
                // RAM mirroring
                addr = addr & 0x7FF;
                //Console.WriteLine(" -> RAM ${0:X4} = ${1:X2}", addr, RAM[addr]);
                return RAM[addr];
            }
            else if (addr < 0x4000)
            {
                addr = 0x2000 | (addr & 0x7);
                //Console.WriteLine("R IO ${0:X4}", addr);
                return 0;
            }
            else if (addr < 0x4020)
            {
                //Console.WriteLine("R IO ${0:X4}", addr);
                return 0;
            }
            else if (addr < 0x6000)
            {
                //Console.WriteLine(" -> Expansion ROM ${0:X4}", addr);
                return 0;
            }
            else if (addr < 0x8000)
            {
                //Console.WriteLine(" -> Save RAM ${0:X4}", addr);
                return 0;
            }
            else
            {
                byte b;

                // Temp. NROM emulation
                if (addr < 0xC000)
                {
                    b = rom.PrgRomBanks[0][addr - 0x8000];
                }
                else
                {
                    b = rom.PrgRomBanks[1][addr - 0xC000];
                }

                //Console.WriteLine(" -> PRG-ROM ${0:X4} = ${1:X2}", addr, b);
                return b;
            }
        }


        public void Write(int addr, byte val)
        {
            addr = addr & 0xFFFF;

            //Console.WriteLine("Write ${0:X4} = ${1:X2}", addr, val);

            if (addr < 0x2000)
            {
                // RAM mirroring
                addr = addr & 0x7FF;
                //Console.WriteLine(" -> RAM ${0:X4}", addr);
                //Console.WriteLine("W RAM ${0:X4} = ${1:X2}", addr, val);
                RAM[addr] = val;
            }
            else if (addr < 0x4000)
            {
                addr = 0x2000 | (addr & 0x7);
                //Console.WriteLine("W IO ${0:X4} = ${1:X2}", addr, val);
                // TODO
            }
            else if (addr < 0x4020)
            {
                //Console.WriteLine("W IO ${0:X4} = ${1:X2}", addr, val);
                // TODO
            }
            else if (addr < 0x6000)
            {
                //Console.WriteLine(" -> Expansion ROM ${0:X4}", addr);
            }
            else if (addr < 0x8000)
            {
                //Console.WriteLine(" -> Save RAM ${0:X4}", addr);
                // TODO
            }
            else
            {
                //Console.WriteLine(" -> PRG-ROM ${0:X4}", addr);
            }
        }
    }
}
