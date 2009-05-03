using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Emu6502
{
    // TODO: "Faster" palette management
    // TODO: "Faster" sprite management

    public class Ppu
    {
        public const int ScreenWidth = 256;
        public const int ScreenHeight = 240;

        public const int Scanlines = 262;
        public const int VsyncScanlines = 20;
        public const int ClocksPerScanline = 341;

        public int[] Framebuffer = new int[ScreenWidth * ScreenHeight];

        private Nes nes;
        public byte[] PatternTables;
        public byte[][] NameAttributeTables;
        public byte[] Palette;
        public byte[] SpriteMem; // OAM stuff

        public bool VsyncFlag;
        public byte PpuCtrl;
        public byte PpuMask;
        public byte OAMAddr; // TODO: Do DMA from here...
        public byte ScrollX;
        public byte ScrollY;
        /*public int ScrollLatch;
        public int VramAddrLatch;*/
        public bool PpuLatch;
        public ushort VramAddr;
        public byte DelayedVramRead;
        public bool VsyncSignalToMainLoop = false;
        public bool SpriteHitFlag;

        public int FrameCycle = 0;
        public int ScanlineIndex = 0; // Essentially "Y" counter
        public int ScanlineCycle = 0; // Essentially "X" counter

        private StreamWriter fs = new StreamWriter("nmi.csv");

        public Ppu(Nes nes)
        {
            this.nes = nes;
        }

        public void Reset()
        {
            PpuLatch = false;
            Framebuffer = new int[ScreenWidth * ScreenHeight];
            VsyncFlag = false;
            PpuCtrl = 0x0;
            PpuMask = 0x0;
            OAMAddr = 0x0;
            //ScrollLatch = 0;
            ScrollX = 0;
            ScrollY = 0;
            //VramAddrLatch = 0;
            VramAddr = 0;
            DelayedVramRead = 0;
            VsyncSignalToMainLoop = false;
            FrameCycle = 0;
            ScanlineIndex = 0;
            ScanlineCycle = 0;
            SpriteHitFlag = false;

            // TODO: This is mapper's job
            //PatternTables = (nes.Rom.ChrRomBanks.Length == 0) ? new byte[8*1024] : nes.Rom.ChrRomBanks[0];
            //NameAttributeTables = new byte[4096];
            NameAttributeTables = new byte[4][];
            if (nes.Rom.MirrorType == MirrorType.Horizontal)
            {
                NameAttributeTables[0] = NameAttributeTables[1] = new Byte[1024];
                NameAttributeTables[2] = NameAttributeTables[3] = new Byte[1024];
            }
            else if (nes.Rom.MirrorType == MirrorType.Vertical)
            {
                NameAttributeTables[0] = NameAttributeTables[2] = new Byte[1024];
                NameAttributeTables[1] = NameAttributeTables[3] = new Byte[1024];
            }
            else if (nes.Rom.MirrorType == MirrorType.FourScreen)
            {
                NameAttributeTables[0] = new Byte[1024];
                NameAttributeTables[1] = new Byte[1024];
                NameAttributeTables[2] = new Byte[1024];
                NameAttributeTables[3] = new Byte[1024];
            }

            Palette = new byte[32];
            SpriteMem = new byte[256];
        }

        // $2000 PPUCTRL (W)
        public void WritePpuCtrl(byte val)
        {
            Console.WriteLine("PPUCTRL = ${0:X2} PC = ${1:X4}", val, nes.Cpu.PC);
            PpuCtrl = val;

            if ((PpuCtrl & 0x80) != 0)
                fs.WriteLine("{0},{1},VBL NMI Enabled", nes.TotalCpuCycles, VsyncFlag?1:0);
            else
                fs.WriteLine("{0},{1},VBL NMI Enabled", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
        }
        
        // $2001 PPUMASK (W)
        public void WritePpuMask(byte val)
        {
            Console.WriteLine("PPUMASK = ${0:X2} PC = ${1:X4}", val, nes.Cpu.PC);
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
                fs.WriteLine("{0},{1},VBL flag read", nes.TotalCpuCycles, VsyncFlag?1:0);
                Console.WriteLine("PPUSTATUS read returned VSync flag PC=${0:X4}", nes.Cpu.PC);
                status |= 0x80;
            }
            /*else
                fs.WriteLine("{0},{1},VBL flag read", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);*/

            if (SpriteHitFlag)
                status |= 0x40;

            VsyncFlag = false;

            /*if (ScrollLatch != 0)
                ;// Console.WriteLine("Resetting SCROLL latch");
            ScrollLatch = 0;
            if (VramAddrLatch != 0)
                ;// Console.WriteLine("Resetting VRAM latch");
            VramAddrLatch = 0;*/
            PpuLatch = false;
            /*ScrollX = 0;
            VramAddr = 0;*/

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
            PpuCtrl &= 0xFC;
            if (!PpuLatch)
            {
                ScrollX = val;
                //Console.WriteLine("SCROLLX = ${0:X2}", val);
            }
            else// if (ScrollLatch == 1)
            {
                ScrollY = val;
                //Console.WriteLine("SCROLLY = ${0:X2}", val);
            }

            //ScrollLatch++;
            PpuLatch = !PpuLatch;
            //ScrollLatch = (ScrollLatch + 1) % 2;
        }

        // $2006 PPUADDR (W)
        public void WritePpuAddr(byte val)
        {
            /*VramAddr <<= 8;
            VramAddr |= val;*/
            if (!PpuLatch)
            {
                VramAddr = val;// (ushort)((VramAddr & 0xFF00) | val);
            }
            else
            {
                //VramAddr = (ushort)((VramAddr & 0x00FF) | (val<<8));
                VramAddr <<= 8;
                VramAddr |= val;
            }

            //Console.WriteLine("Latched PPU address: ${0:X4}", VramAddr);
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

            PpuLatch = !PpuLatch;
        }

        // $2007 PPUDATA (R/W)
        public byte ReadPpuData()
        {
            byte result;

            // Palette read
            if (VramAddr >= 0x3f00 && VramAddr < 0x4000)
            {
                DelayedVramRead = Read(0x2F00 | (VramAddr & 0xFF));
                result = Read(VramAddr);
                Console.WriteLine("R VRAM (pal) ${0:X4}=${1:X2}", VramAddr, result);
            }
            else
            {
                result = DelayedVramRead;
                DelayedVramRead = Read(VramAddr);
                Console.WriteLine("R VRAM ${0:X4}=${1:X2}", VramAddr, result);
            }

            //Console.WriteLine("R VRAM ${0:X4}=${1:X2}", VramAddr, DelayedVramRead);

            if ((PpuCtrl & 0x04) != 0)
                VramAddr += 0x20;
            else
                VramAddr++;

            VramAddr &= 0x3FFF;

            return result;
        }

        public void WritePpuData(byte data)
        {
            //nes.Cpu.Paused = true;
            Write(VramAddr, data);

            /*if(data != 0x00 && data != 0x24)
                Console.WriteLine("W VRAM ${0:X4}=${1:X2}", VramAddr, data);*/

            //Console.WriteLine("W VRAM ${0:X4}=${1:X2}", VramAddr, data);

            if ((PpuCtrl & 0x04) != 0)
                VramAddr += 0x20;
            else
                VramAddr++;
            VramAddr &= 0x3FFF;
        }

        public int MirrorHigh(int addr)
        {
            return (addr & 0xC00) >> 10;
        }

        public int MirrorLow(int addr)
        {
            return (addr & 0x3FF);
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
                //int nameAttrAddr = addr & 0x0FFF;
                return NameAttributeTables[MirrorHigh(addr)][MirrorLow(addr)];
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
                //Console.WriteLine("Writing to ROM! O_O");
                // Temp. -- allow writing to ROM, overtest.nes does this
                PatternTables[addr] = val;
            }
            else if(addr >= 0x2000 && addr < 0x3F00)
            {
                // TODO: Proper mirroring here
                /*int nameAttrAddr = addr & 0xFFF;
                NameAttributeTables[nameAttrAddr] = val;*/

                NameAttributeTables[MirrorHigh(addr)][MirrorLow(addr)] = val;
            }
            else// if(addr >= 0x3F00 && addr < 0x4000)
            {
                int paletteAddr = addr & 0x1F;

                // Zeroth element of every palette entry is mirrored
                if ((paletteAddr & 0xF) == 0)
                {
                    //Console.WriteLine("Palette (Mirror) ${0:X2} = ${1:X2}", paletteAddr, val);
                    Palette[0x00] =
                    /*Palette[0x04] =
                    Palette[0x08] =
                    Palette[0x0c] =*/
                    Palette[0x10] =
                    /*Palette[0x14] =
                    Palette[0x18] =
                    Palette[0x1c] =*/ val;
                }
                else
                {
                    //Console.WriteLine("Palette ${0:X2} = ${1:X2}", paletteAddr, val);
                    Palette[paletteAddr] = val;
                }
            }
        }

        private void FillBG(int row)
        {
            int bgcolor = PpuOutput.Palette[Palette[0] & 0x3f] | 0xFF << 24;
            int pos = ScreenWidth * row;
            for (int x = 0; x < ScreenWidth; x++)
                Framebuffer[pos++] = bgcolor;
        }

        private void RenderBG(int srcRow, int ntX, int ntY, int screenX, int screenY)
        {
            int rowStart = ScreenWidth * screenY;
            int bgPatternStart = (PpuCtrl & 0x10) != 0 ? 0x1000 : 0x0000;

            //int nametableStart = 0x2000;

            int mirrorHigh = ntY << 1 | ntX;//MirrorHigh(nameTableOffset);

            int tileY = srcRow / 8;
            int subtileY = srcRow % 8;
            //int fbPos = rowStart;
            if (tileY >= 0 && tileY < 0x30)
            {
                for (int tileX = 0; tileX < 32; ++tileX)
                {
                    int attrByteX = tileX / 4;
                    int attrByteY = tileY / 4;
                    int attrByteSubX = tileX % 4;
                    int attrByteSubY = tileY % 4;
                    int attrAddr = 0x3c0 + attrByteX + attrByteY * 8;
                    byte attrByte = NameAttributeTables[mirrorHigh][attrAddr];
                    int nibbleX = (attrByteSubX >> 1) & 0x1;
                    int nibbleY = (attrByteSubY >> 1) & 0x1;
                    int shiftAmt = (nibbleY << 2 | nibbleX << 1);
                    int paletteAddr = ((attrByte >> shiftAmt) & 0x3) * 4;
                    //int palette0 = PpuOutput.Palette[Palette[0] & 0x3f] | 0xFF << 24;
                    int palette1 = PpuOutput.Palette[Palette[paletteAddr + 1] & 0x3f] | 0xFF << 24;
                    int palette2 = PpuOutput.Palette[Palette[paletteAddr + 2] & 0x3f] | 0xFF << 24;
                    int palette3 = PpuOutput.Palette[Palette[paletteAddr + 3] & 0x3f] | 0xFF << 24;


                    int pattern = NameAttributeTables[mirrorHigh][tileY * 0x20 + tileX];

                    int patternAddr = bgPatternStart + pattern * 16;
                    byte b1 = PatternTables[patternAddr + subtileY];
                    byte b2 = PatternTables[patternAddr + subtileY + 8];

                    for (int subtileX = 0; subtileX < 8; ++subtileX)
                    {
                        byte bit1 = (byte)((b1 >> (7 - subtileX)) & 0x1);
                        byte bit2 = (byte)((b2 >> (7 - subtileX)) & 0x1);
                        byte result = (byte)(bit2 << 1 | bit1);

                        if (screenX >= 0 && screenX < ScreenWidth)
                        {
                            int fbPos = rowStart + screenX;

                            if (result == 3)
                                Framebuffer[fbPos] = palette3;
                            else if (result == 2)
                                Framebuffer[fbPos] = palette2;
                            else if (result == 1)
                                Framebuffer[fbPos] = palette1;
                            /*else
                                Framebuffer[fbPos] = palette0;*/
                        }

                        ++screenX;
                        //fbPos++;
                    }
                }
            }
        }

        private void RenderSpriteRow(int row, int spriteX, int spriteY, int patternAddr, int palette, bool flipH, bool flipV)
        {
            int paletteAddr = 0x10 + palette * 4;
            int palette1 = PpuOutput.Palette[Palette[paletteAddr + 1] & 0x3f] | 0xFF << 24;
            int palette2 = PpuOutput.Palette[Palette[paletteAddr + 2] & 0x3f] | 0xFF << 24;
            int palette3 = PpuOutput.Palette[Palette[paletteAddr + 3] & 0x3f] | 0xFF << 24;

            int rowStart = ScreenWidth * row;

            int cx = flipH ? 0 : 7;
            int dir = flipH ? 1 : -1;

            int cy = row - spriteY;
            if (flipV)
                cy = 7 - cy;

            byte b1 = PatternTables[patternAddr + cy];
            byte b2 = PatternTables[patternAddr + cy + 8];

            for (int n = 0; n < 8; ++n)
            {
                if (spriteX + cx >= 0 && spriteX + cx < ScreenWidth)
                {
                    byte bit1 = (byte)((b1 >> cx) & 0x1);
                    byte bit2 = (byte)((b2 >> cx) & 0x1);
                    byte result = (byte)(bit2 << 1 | bit1);

                    if (result == 1)
                        Framebuffer[rowStart + spriteX + n] = palette1;
                    else if (result == 2)
                        Framebuffer[rowStart + spriteX + n] = palette2;
                    else if (result == 3)
                        Framebuffer[rowStart + spriteX + n] = palette3;

                    cx += dir;
                }
            }
        }

        private void RenderSprites8x8(int row, bool priority)
        {
            // TODO: Sprite-BG tests...

            int spritePatternTableAddr = (PpuCtrl & 0x08) != 0 ? 0x1000 : 0x0000;

            // Render sprites
            int offset = 0;
            for (int i = 0; i < 64; ++i)
            {
                int y = SpriteMem[offset++];
                byte B1 = SpriteMem[offset++];
                byte B2 = SpriteMem[offset++];
                int x = SpriteMem[offset++];

                if (y >= 0xEE)
                    continue;

                y += 1;
                int cy = row - y;

                if (cy < 0 || cy >= 8)
                    continue;

                bool spritePriority = (B2 & 0x20) != 0;

                if (spritePriority != priority)
                    continue;

                int palette = (B2 & 0x3);
                int patternAddr = spritePatternTableAddr + B1 * 16;
                bool flipH = (B2 & 0x40) != 0;
                bool flipV = (B2 & 0x80) != 0;

                RenderSpriteRow(row, x, y, patternAddr, palette, flipH, flipV);
            }
        }

        private void RenderSprites8x16(int row, bool priority)
        {
            // TODO: Sprite-BG tests...

            // Render sprites
            int offset = 0;
            for (int i = 0; i < 64; ++i)
            {
                int y = SpriteMem[offset++];
                byte B1 = SpriteMem[offset++];
                byte B2 = SpriteMem[offset++];
                int x = SpriteMem[offset++];

                if (y >= 0xEE)
                    continue;

                y += 1;
                int cy = row - y;

                if (cy < 0 || cy >= 16)
                    continue;

                bool spritePriority = (B2 & 0x20) != 0;

                if (spritePriority != priority)
                    continue;

                int pattern1 = B1 & 0xFE;
                int bank = (B1 & 0x1) != 0 ? 0x1000 : 0x0000;
                int palette = (B2 & 0x3);
                int patternAddr1 = bank + pattern1 * 16;
                int patternAddr2 = bank + (pattern1 + 1) * 16;
                bool flipH = (B2 & 0x40) != 0;
                bool flipV = (B2 & 0x80) != 0;

                if (flipV)
                {
                    if (cy < 8)
                        RenderSpriteRow(row, x, y, patternAddr2, palette, flipH, flipV);
                    else
                        RenderSpriteRow(row, x, y+8, patternAddr1, palette, flipH, flipV);
                }
                else
                {
                    if (cy < 8)
                        RenderSpriteRow(row, x, y, patternAddr1, palette, flipH, flipV);
                    else
                        RenderSpriteRow(row, x, y + 8, patternAddr2, palette, flipH, flipV);
                }
            }
        }

#if false
        private void RenderSprites(int row, bool priority)
        {
            int rowStart = ScreenWidth * row;

            // TODO: Sprite-BG tests...

            int spritePatternTableAddr = (PpuCtrl & 0x08) != 0 ? 0x1000 : 0x0000;

            /*
            bool _8x16 = (PpuCtrl & 0x20) != 0;
            int spriteHeight = _8x16 ? 16 : 8;
             * */

            // Render sprites
            int offset = 0;
            for (int i = 0; i < 64; ++i)
            {
                int y = SpriteMem[offset++];
                byte B1 = SpriteMem[offset++];
                byte B2 = SpriteMem[offset++];
                int x = SpriteMem[offset++];

                if (y >= 0xEE)
                    continue;

                y += 1;
                int cy = row - y;

                if (cy < 0 || cy >= 8)
                    continue;

                bool spritePriority = (B2 & 0x20) != 0;

                if (spritePriority != priority)
                    continue;

                int palette = (B2 & 0x3);
                int paletteAddr = 0x10 + palette * 4;
                int palette1 = PpuOutput.Palette[Palette[paletteAddr + 1] & 0x3f] | 0xFF << 24;
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

                for (int n = 0; n < 8; ++n)
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
#endif

        private void RenderRow(int row)
        {
            bool enableBG = (PpuMask & 0x08) !=0;
            bool enableSprites = (PpuMask & 0x10)!=0;

            int baseNTX = PpuCtrl & 0x1;
            int baseNTY = (PpuCtrl >> 1) & 0x1;
            bool _8x16 = (PpuCtrl & 0x20) != 0;

            FillBG(row);

            if (enableSprites)
            {
                if (_8x16)
                    RenderSprites8x16(row, true);
                else
                    RenderSprites8x8(row, true);
            }

            if (enableBG)
            {
                if (row >= ScreenHeight - ScrollY)
                {
                    baseNTY ^= 1;

                    RenderBG(row + ScrollY - ScreenHeight, baseNTX, baseNTY, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(row + ScrollY - ScreenHeight, baseNTX, baseNTY, -ScrollX + ScreenWidth, row);
                }
                else
                {
                    RenderBG(row + ScrollY, baseNTX, baseNTY, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(row + ScrollY, baseNTX, baseNTY, -ScrollX + ScreenWidth, row);                    
                }
            }

            if (enableSprites)
            {
                if (_8x16)
                    RenderSprites8x16(row, false);
                else
                    RenderSprites8x8(row, false);
            }
        }

        private void Vsync()
        {
            SpriteHitFlag = false;
            VsyncFlag = true;
            VsyncSignalToMainLoop = true;
            if ((PpuCtrl & 0x80) != 0)
            {
                fs.WriteLine("{0},1,NMI Triggered", nes.TotalCpuCycles);
                nes.Cpu.NMI();
            }
            else
                Console.WriteLine("VSync NMI Ignored");
        }

        public void FinishScanline()
        {
            /*if (ScanlineIndex == 20)
            {
                Console.WriteLine("Clear NMI Flag @ {0} cycles", nes.TotalCpuCycles);
                VsyncFlag = false;
            }*/

            /*ScanlineCycle++;
            if (ScanlineCycle >= ClocksPerScanline)
            {*/
                int row = ScanlineIndex - VsyncScanlines;
                if (row >= 0 && row < ScreenHeight)
                    RenderRow(row);
                // hack hack
                if (row == 31)
                    SpriteHitFlag = true;
                ++ScanlineIndex;
                //ScanlineCycle = 0;

                if (ScanlineIndex >= Scanlines)
                {
                    ScanlineCycle = 0;
                    ScanlineIndex = 0;
                    FrameCycle = 0;
                    Vsync();
                    fs.WriteLine("{0},1,VBL set", nes.TotalCpuCycles);
                    Console.WriteLine("NMI @ {0} cycles", nes.TotalCpuCycles);
                }
            //}

        }

        public void Tick()
        {
            FrameCycle++;
            ScanlineCycle++;

            // Temp. -- Figure out why this works! :( Should be 2270*3 = 6810
            if (FrameCycle == 2270*3)//6200)
            {
                fs.WriteLine("{0},0,Clear VBL Flag", nes.TotalCpuCycles);
                Console.WriteLine("Clear NMI Flag @ {0} cycles", nes.TotalCpuCycles);
                VsyncFlag = false;
            }

            if (ScanlineCycle == ClocksPerScanline)
            {
                ScanlineCycle = 0;
                FinishScanline();
            }
        }

        /*private void ShowFrame()
        {
            PpuOutput output = new PpuOutput();
            output.ShowDialog();
        }*/
    }
}
