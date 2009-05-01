using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class Ppu
    {
        private Nes nes;
        public byte[] PatternTables;
        public byte[] NameAttributeTables;
        public byte[] Palette;
        public byte[] SpriteMem; // OAM stuff

        public bool VsyncFlag;
        public byte PpuCtrl;
        public byte PpuMask;
        public byte OAMAddr;
        public byte ScrollX;
        public byte ScrollY;
        public int ScrollLatch;
        public int VramAddrLatch;
        public ushort VramAddr;
        public byte DelayedVramRead;
        public bool VsyncSignalToMainLoop = false;

        public int VsyncTimer;

        public Ppu(Nes nes)
        {
            this.nes = nes;
        }

        // $2000 PPUCTRL (W)
        public void WritePpuCtrl(byte val)
        {
            Console.WriteLine("PPUCTRL = ${0:X2}", val);
            PpuCtrl = val;
        }
        
        // $2001 PPUMASK (W)
        public void WritePpuMask(byte val)
        {
            Console.WriteLine("PPUMASK = ${0:X2}", val);
            PpuMask = val;
        }

        // $2002 PPUSTATUS (R)
        public byte ReadPpuStatus()
        {
            // TODO: Sprite overflow
            // TODO: Sprite 0 hit
            byte status = 0;
            if (VsyncFlag)
            {
                Console.WriteLine("PPUSTATUS read returned VSync flag");
                status |= 0x80;
            }

            // temp.
            status |= 0x40;

            VsyncFlag = false;
            ScrollLatch = 0;
            VramAddrLatch = 0;

            return status;
        }

        // $2003 OAMADDR (W)
        public void WriteOAMAddr(byte val)
        {
            Console.WriteLine("OAMADDR = ${0:X2}", val);
            OAMAddr = val;
        }

        // $2004 OAMDATA (R/W)
        public byte ReadOAMData() { return SpriteMem[OAMAddr]; }
        public void WriteOAMData(byte val) { SpriteMem[OAMAddr++] = val; }

        // $2005 PPUSCROLL (W)
        public void WritePpuScroll(byte val)
        {
            if (ScrollLatch == 0)
            {
                ScrollX = val;
                Console.WriteLine("SCROLLX = ${0:X2}", val);
            }
            else if (ScrollLatch == 1)
            {
                ScrollY = val;
                Console.WriteLine("SCROLLY = ${0:X2}", val);
            }
            ++ScrollLatch;
        }

        // $2006 PPUADDR (W)
        public void WritePpuAddr(byte val)
        {
            if (VramAddrLatch == 0)
                VramAddr = val;
            else if (VramAddrLatch == 1)
            {
                VramAddr <<= 8;
                VramAddr |= val;
            }
            
            ++VramAddrLatch;
        }

        // $2006 PPUDATA (R/W)
        // TODO: Check that this is really correct
        public byte ReadPpuData()
        {
            DelayedVramRead = Read(VramAddr);

            if ((PpuCtrl & 0x04) != 0)
                VramAddr += 0x20;
            else
                VramAddr++;

            return DelayedVramRead;
        }

        public void WritePpuData(byte data)
        {
            Write(VramAddr, data);

            if ((PpuCtrl & 0x04) != 0)
                VramAddr += 0x20;
            else
                VramAddr++;
        }

        public void Reset()
        {
            VsyncFlag = false;
            PpuCtrl = 0x0;
            PpuMask = 0x0;
            OAMAddr = 0x0;
            ScrollLatch = 0;
            ScrollX = 0;
            ScrollY = 0;
            VramAddrLatch = 0;
            VramAddr = 0;
            DelayedVramRead = 0;
            VsyncSignalToMainLoop = false;

            // TODO: This is mapper's job
            PatternTables = nes.Rom.ChrRomBanks[0];
            NameAttributeTables = new byte[4096];
            Palette = new byte[32];
            SpriteMem = new byte[256];
        }

        public byte Read(int addr)
        {
            addr = addr & 0x3FFF;

            if(addr >= 0 && addr < 0x2000)
            {
                return PatternTables[addr];
            }
            else if(addr >= 0x2000 && addr < 0x3F00)
            {
                int nameAttrAddr = addr & 0x1FFF;
                return NameAttributeTables[nameAttrAddr];
            }
            else// if(addr >= 0x3F00 && addr < 0x4000)
            {
                int paletteAddr = addr & 0x1F;
                return Palette[paletteAddr];
            }
        }

        public void Write(int addr, byte val)
        {
            addr = addr & 0x3FFF;

            if(addr >= 0 && addr < 0x2000)
            {
                // Assume this is ROM for now
            }
            else if(addr >= 0x2000 && addr < 0x3F00)
            {
                int nameAttrAddr = addr & 0x1FFF;
                NameAttributeTables[nameAttrAddr] = val;
            }
            else// if(addr >= 0x3F00 && addr < 0x4000)
            {
                int paletteAddr = addr & 0x1F;
                Palette[paletteAddr] = val;
            }
        }

        public void Tick()
        {
            VsyncTimer++;
            if (VsyncTimer > Nes.PpuTicksPerSecond / 60)
            {
                VsyncTimer = 0;
                VsyncFlag = true;
                VsyncSignalToMainLoop = true;
                if ((PpuCtrl & 0x80) != 0)
                    nes.Cpu.NMI();
                else
                    Console.WriteLine("VSync NMI Ignored");
                //ShowFrame();
            }
        }

        private void ShowFrame()
        {
            PpuOutput output = new PpuOutput(this);
            output.ShowDialog();
        }
    }
}
