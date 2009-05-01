using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    // TODO: Separate mapper system
    public class NesMemory : IMemory
    {
        private byte[] RAM;
        private Nes nes;
        private Rom rom;

        public NesMemory(Nes nes)
        {
            this.nes = nes;
            this.rom = nes.Rom;
            
        }

        public void Reset()
        {
            RAM = new byte[2048];
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

                byte b;
                switch (addr)
                {
                    case 0x2002:
                        b = nes.Ppu.ReadPpuStatus();
                        break;
                    case 0x2003:
                        b = nes.Ppu.ReadOAMData();
                        break;
                    case 0x2007:
                        b = nes.Ppu.ReadPpuData();
                        break;
                    default:
                        b = 0x0;
                        break;
                }

                if (addr != 0x2002)
                    Console.WriteLine("R IO ${0:X4} = ${1:X2}", addr, b);
                return b;
            }
            else if (addr < 0x4020)
            {
                Console.WriteLine("R IO ${0:X4}", addr);
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
                switch (addr)
                {
                    case 0x2000:
                        nes.Ppu.WritePpuCtrl(val);
                        break;
                    case 0x2001:
                        nes.Ppu.WritePpuMask(val);
                        break;
                    case 0x2003:
                        nes.Ppu.WriteOAMAddr(val);
                        break;
                    case 0x2005:
                        nes.Ppu.WritePpuScroll(val);
                        break;
                    case 0x2006:
                        nes.Ppu.WritePpuAddr(val);
                        break;
                    case 0x2007:
                        nes.Ppu.WritePpuData(val);
                        break;
                    default:
                        break;
                }
            }
            else if (addr < 0x4020)
            {
                Console.WriteLine("W IO ${0:X4} = ${1:X2}", addr, val);

                // Fake DMA
                if (addr == 0x4014)
                {
                    ushort srcAddr = (ushort)(val * 0x100);
                    Console.WriteLine("DMAing ${0:X4} to OAM memory", srcAddr);
                    // TODO: LOL, potential explosion
                    for (int i = 0; i < 256; ++i)
                    {
                        byte b = Read(srcAddr + i);
                        nes.Ppu.SpriteMem[i] = b;
                    }
                }

                // TODO
            }
            else if (addr < 0x6000)
            {
                Console.WriteLine(" -> Expansion ROM ${0:X4} = ${0:X2}", addr, val);
            }
            else if (addr < 0x8000)
            {
                Console.WriteLine(" -> Save RAM ${0:X4} = ${0:X2}", addr, val);
                // TODO
            }
            else
            {
                Console.WriteLine(" -> PRG-ROM ${0:X4} = ${0:X2}", addr, val);
            }
        }
    }
}
