using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Emu6502
{
    public class Ppu
    {
        public const int ScreenWidth = 256;
        public const int ScreenHeight = 240;

        public const int Scanlines = 262;
        public const int VsyncScanlines = 20;
        public const int ClocksPerScanline = 341;

        public int[] Framebuffer = new int[ScreenWidth * ScreenHeight];

        public Nes ParentNes { get { return nes; } }
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
        public bool SpriteHitFlag;

        public int ScanlineIndex = 0; // Essentially "Y" counter
        public int ScanlineCycle = 0; // Essentially "X" counter

        public Ppu(Nes nes)
        {
            this.nes = nes;
        }

        public void Reset()
        {
            Framebuffer = new int[ScreenWidth * ScreenHeight];
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
            ScanlineIndex = 0;
            ScanlineCycle = 0;
            SpriteHitFlag = false;

            // TODO: This is mapper's job
            PatternTables = nes.Rom.ChrRomBanks[0];
            NameAttributeTables = new byte[4096];
            Palette = new byte[32];
            SpriteMem = new byte[256];
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

            if (SpriteHitFlag)
                status |= 0x40;

            VsyncFlag = false;

            if (ScrollLatch != 0)
                Console.WriteLine("Resetting SCROLL latch");
            ScrollLatch = 0;
            if (VramAddrLatch != 0)
                Console.WriteLine("Resetting VRAM latch");
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
            else
                Console.WriteLine("Excess scroll x/y latch ${0:X2}", val);

            ++ScrollLatch;
        }

        // $2006 PPUADDR (W)
        public void WritePpuAddr(byte val)
        {
            VramAddr <<= 8;
            VramAddr |= val;

            Console.WriteLine("Latched PPU address: ${0:X4}", VramAddr);
            /*if (VramAddrLatch == 0)
            {
                VramAddr = val;
                Console.WriteLine("Latched PPU address low: ${0:X4}", VramAddr);
            }
            else if (VramAddrLatch == 1)
            {
                VramAddr <<= 8;
                VramAddr |= 8;
                Console.WriteLine("Latched PPU address hi: ${0:X4}", VramAddr);
            }
            else
                Console.WriteLine("Excess PPU address latch: ${0:X2}", val);*/
            
            ++VramAddrLatch;
        }

        // $2006 PPUDATA (R/W)
        // TODO: Check that this is really correct
        public byte ReadPpuData()
        {
            DelayedVramRead = Read(VramAddr);

            //Console.WriteLine("R VRAM ${0:X4}=${1:X2}", VramAddr, DelayedVramRead);

            if ((PpuCtrl & 0x04) != 0)
                VramAddr += 0x20;
            else
                VramAddr++;

            return DelayedVramRead;
        }

        public void WritePpuData(byte data)
        {
            //nes.Cpu.Paused = true;
            Write(VramAddr, data);

            if(data != 0x00 && data != 0x24)
                Console.WriteLine("W VRAM ${0:X4}=${1:X2}", VramAddr, data);

            if ((PpuCtrl & 0x04) != 0)
                VramAddr += 0x20;
            else
                VramAddr++;
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

        private void Vsync()
        {
            SpriteHitFlag = false;
            VsyncFlag = true;
            VsyncSignalToMainLoop = true;
            if ((PpuCtrl & 0x80) != 0)
                nes.Cpu.NMI();
            else
                Console.WriteLine("VSync NMI Ignored");
        }

        private void RenderRow(int row)
        {
            /*if(row == 32)
                SpriteHitFlag = true;*/

            int rowStart = ScreenWidth*row;

            // Clear BG
            const int bgcolor = (0xFF << 24) | 0x0000FF;
            int pos = rowStart;
            for (int n = ScreenWidth - 1; n >= 0; --n)
                Framebuffer[pos++] = bgcolor;

            int spritePatternTableAddr = (PpuCtrl & 0x08)!=0 ? 0x1000 : 0x0000;

            // TODO: Sprite-BG tests...

            int offset = 0;
            for (int i = 0; i < 64; ++i)
            {
                int y = SpriteMem[offset++];
                int cy = row - y;

                if (cy < 0 || cy >= 8)
                {
                    offset += 3;
                    continue;
                }

                byte B1 = SpriteMem[offset++];
                byte B2 = SpriteMem[offset++];
                int x = SpriteMem[offset++];

                int palette = (B2 & 0x3);
                int paletteAddr = 0x10 + palette * 4;
                int palette1 = PpuOutput.Palette[Palette[paletteAddr + 1] & 0x3f] | 0xFF<<24;
                int palette2 = PpuOutput.Palette[Palette[paletteAddr + 2] & 0x3f] | 0xFF << 24;
                int palette3 = PpuOutput.Palette[Palette[paletteAddr + 3] & 0x3f] | 0xFF << 24;
                int patternAddr = spritePatternTableAddr + B1 * 16;
                bool flipH = (B2 & 0x40) != 0;
                bool flipV = (B2 & 0x80) != 0;

                if (flipV)
                    cy = 7 - cy;

                int cx = flipH ? 0 : 7;
                int dir = flipH ? 1 : -1;

                byte b1 = PatternTables[patternAddr + cy];
                byte b2 = PatternTables[patternAddr + cy + 8];

                for (int n=0; n<8; ++n)
                {
                    if (x + cx >= 0 && x + cx < ScreenWidth)
                    {
                        byte bit1 = (byte)((b1 >> cx) & 0x1);
                        byte bit2 = (byte)((b2 >> cx) & 0x1);
                        byte result = (byte)(bit2 << 1 | bit1);

                        if (result == 1)
                            Framebuffer[rowStart + x + n] = palette1;
                        else if (result == 2)
                            Framebuffer[rowStart + x + n] = palette2;
                        else if (result == 3)
                            Framebuffer[rowStart + x + n] = palette3;

                        cx += dir;
                    }
                }
            }

        }

        public void FinishScanline()
        {
            /*ScanlineCycle++;
            if (ScanlineCycle >= ClocksPerScanline)
            {*/
                int row = ScanlineIndex - VsyncScanlines;
                if (row >= 0 && row < ScreenHeight)
                    RenderRow(row);
                // hack hack
                if (row == 32)
                    SpriteHitFlag = true;
                ++ScanlineIndex;
                ScanlineCycle = 0;

                if (ScanlineIndex >= Scanlines)
                {
                    ScanlineCycle = 0;
                    ScanlineIndex = 0;
                    Vsync();
                }
            //}

        }

        private void ShowFrame()
        {
            PpuOutput output = new PpuOutput(this);
            output.ShowDialog();
        }
    }
}
