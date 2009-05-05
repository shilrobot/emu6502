using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    // TODO: Separate mapper system
    public class NesMemory : IMemory
    {
        public byte[] RAM;
        public byte[] SaveRAM;
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
            SaveRAM = new byte[0x2000];
        }

        public byte Read(int addr)
        {
            //addr = addr & 0xFFFF;

            //Console.WriteLine("Read ${0:X4}", addr);

            switch(addr & 0xF000)
            {
                case 0x0000:
                case 0x1000:
                    //Console.WriteLine(" -> RAM ${0:X4} = ${1:X2}", addr, RAM[addr]);
                    return RAM[addr & 0x7FF];
                case 0x2000:
                case 0x3000:
                    {
                        Nes.ActiveNes.RecordEvent(String.Format("Mem.IO.${0:X4}.Read", addr));

                        addr = 0x2000 | (addr & 0x7);

                        byte b;
                        switch (addr)
                        {
                            case 0x2002:
                                b = nes.Ppu.ReadPpuStatus();
                                break;
                            case 0x2004:
                                b = nes.Ppu.ReadOAMData();
                                break;
                            case 0x2007:
                                b = nes.Ppu.ReadPpuData();
                                break;
                            default:
                                b = 0x0;
                                break;
                        }

                        /*if (addr != 0x2002)
                            Console.WriteLine("R IO ${0:X4} = ${1:X2}", addr, b);*/

                        return b;
                    }
                // APU registers & expansion ROM (what the heck is that?)
                case 0x4000:
                case 0x5000:
                    {
                        Nes.ActiveNes.RecordEvent(String.Format("Mem.IO.${0:X4}.Read", addr));

                        byte b = 0;
                        if (addr == 0x4016)
                        {
                            //Console.WriteLine("Controller1 = {0:X}", nes.Controller1.Captured);
                            b = (byte)(nes.Controller1.Captured & 0x1);
                            nes.Controller1.Captured >>= 1;
                        }
                        else if (addr == 0x4017)
                        {
                            b = (byte)(nes.Controller2.Captured & 0x1);
                            nes.Controller2.Captured >>= 1;
                        }
                        else
                            if(addr <= 0x4020)
                                Console.WriteLine("R IO ${0:X4}", addr);

                        //Console.WriteLine("R IO ${0:X4} = ${1:X2}", addr, b);
                        /*if (addr < 0x4020)
                        {
                            //Console.WriteLine("R IO ${0:X4}", addr);

                        }*/
                        return b;
                    }

                case 0x6000:
                case 0x7000:
                    // TODO: Mirroring? Mapper specific?
                    Nes.ActiveNes.RecordEvent("Mem.SaveRam.Read");
                    return SaveRAM[addr & 0x1FFF];

                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                case 0xC000:
                case 0xD000:
                case 0xE000:
                case 0xF000:
                    return nes.Mapper.PrgRomRead(addr & 0xFFFF);
                    /*return rom.PrgRomBanks[0][addr & 0x3FFF];

                    return rom.PrgRomBanks[1][addr & 0x3FFF];*/

                // Shut up, C# compiler
                default:
                    return 0;
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
                Nes.ActiveNes.RecordEvent(String.Format("Mem.IO.${0:X4}.Write", addr));
                /*if (addr != 0x2003)
                    Console.WriteLine("W IO ${0:X4} = ${1:X2}", addr, val);*/

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
                    case 0x2004:
                        nes.Ppu.WriteOAMData(val);
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
                //Console.WriteLine("W IO ${0:X4} = ${1:X2}", addr, val);

                Nes.ActiveNes.RecordEvent(String.Format("Mem.IO.${0:X4}.Write", addr));

                // Fake DMA
                if (addr == 0x4014)
                {
                    ushort srcAddr = (ushort)(val * 0x100);
                    //Console.WriteLine("W IO $4014: DMAing ${0:X4} to OAM memory", srcAddr);
                    // TODO: LOL, potential explosion
                    for (int i = 0; i < 256; ++i)
                    {
                        byte b = Read(srcAddr + i);
                        nes.Ppu.SpriteMem[nes.Ppu.OAMAddr++] = b;
                    }
                }
                else if (addr == 0x4016)
                {
                    if ((val & 0x1) != 0)
                    {
                        nes.Controller1.Capture();
                        nes.Controller2.Capture();
                    }
                }
                /*else
                    Console.WriteLine("W IO ${0:X4} = ${1:X2}", addr, val);*/

                // TODO
            }
            else if (addr < 0x6000)
            {
                Nes.ActiveNes.RecordEvent("Mem.ExpansionROM.Write");
                //Console.WriteLine(" -> Expansion ROM ${0:X4} = ${0:X2}", addr, val);
            }
            else if (addr < 0x8000)
            {
                Nes.ActiveNes.RecordEvent("Mem.SaveRam.Write");
                SaveRAM[addr & 0x1FFF] = val;
                //Console.WriteLine(" -> Save RAM ${0:X4} = ${0:X2}", addr, val);
                // TODO
            }
            else
            {
                Nes.ActiveNes.RecordEvent("Mem.PrgRom.Write");
                nes.Mapper.PrgRomWrite(addr, val);
                //Console.WriteLine(" -> PRG-ROM ${0:X4} = ${0:X2}", addr, val);
            }
        }
    }
}
