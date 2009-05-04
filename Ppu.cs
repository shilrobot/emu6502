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

        private byte[] Nametable1;
        private byte[] Nametable2;
        private byte[] Nametable3;
        private byte[] Nametable4;
        public MirrorType Mirroring { get; private set; }

        // NEW STUFF
        private bool latch;
        private int loopyT;
        private int loopyV;
        private int fineX;
        private bool cycleSkipToggle;
        private bool justReadVBL;

        private bool[] SpritePriorityBuffer = new bool[ScreenWidth];
        private int[] SpriteBuffer = new int[ScreenWidth];
        private int[] BGBuffer = new int[ScreenWidth];

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
            cycleSkipToggle = false;

            latch = false;
            loopyT = loopyV = 0;
            fineX = 0;
            justReadVBL = false;

            // TODO: This is mapper's job
            //PatternTables = (nes.Rom.ChrRomBanks.Length == 0) ? new byte[8*1024] : nes.Rom.ChrRomBanks[0];
            //NameAttributeTables = new byte[4096];
            Nametable1 = new byte[1024];
            Nametable2 = new byte[1024];
            Nametable3 = new byte[1024];
            Nametable4 = new byte[1024];
            NameAttributeTables = new byte[4][];
            SetMirroring(MirrorType.Vertical);//nes.Rom.MirrorType);

            // Matches blargg's startup palette
            Palette = new byte[32] {
                0x09, 0x01, 0x00, 0x01, 0x00, 0x02, 0x02, 0x0d, 0x08, 0x10, 0x08, 0x24, 0x00, 0x00, 0x04, 0x2c,
                0x09, 0x01, 0x34, 0x03, 0x00, 0x04, 0x00, 0x14, 0x08, 0x3a, 0x00, 0x02, 0x00, 0x20, 0x2c, 0x08,
            };
            SpriteMem = new byte[256];
        }

        public void SetMirroring(MirrorType type)
        {
            Mirroring = type;

            if (type == MirrorType.Horizontal)
            {
                NameAttributeTables[0] = NameAttributeTables[1] = Nametable1;
                NameAttributeTables[2] = NameAttributeTables[3] = Nametable2;
            }
            else if (type == MirrorType.Vertical)
            {
                NameAttributeTables[0] = NameAttributeTables[2] = Nametable1;
                NameAttributeTables[1] = NameAttributeTables[3] = Nametable2;
            }
            else if (type == MirrorType.FourScreen)
            {
                NameAttributeTables[0] = Nametable1;
                NameAttributeTables[1] = Nametable2;
                NameAttributeTables[2] = Nametable3;
                NameAttributeTables[3] = Nametable4;
            }
            else if (type == MirrorType.SingleScreenLower)
            {
                NameAttributeTables[0] = 
                NameAttributeTables[1] =
                NameAttributeTables[2] =
                NameAttributeTables[3] = Nametable1;
            }
            else if (type == MirrorType.SingleScreenUpper)
            {
                NameAttributeTables[0] =
                NameAttributeTables[1] =
                NameAttributeTables[2] =
                NameAttributeTables[3] = Nametable2;
            }

            Console.WriteLine("Mirroring={0} ({1})", type, IdentifyBanks());
        }

        // $2000 PPUCTRL (W)
        public void WritePpuCtrl(byte val)
        {
            //Console.WriteLine("PPUCTRL = ${0:X2} PC = ${1:X4}", val, nes.Cpu.PC);
            PpuCtrl = val;

            /*if ((PpuCtrl & 0x80) != 0)
                fs.WriteLine("{0},{1},VBL NMI Enabled", nes.TotalCpuCycles, VsyncFlag?1:0);
            else
                fs.WriteLine("{0},{1},VBL NMI Enabled", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);*/

            // Set V&H in T
            loopyT &= ~0xC00;
            loopyT |= (val & 0x3) << 10;
        }
        
        // $2001 PPUMASK (W)
        public void WritePpuMask(byte val)
        {
            //Console.WriteLine("PPUMASK = ${0:X2} PC = ${1:X4}", val, nes.Cpu.PC);
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
                //fs.WriteLine("{0},{1},VBL flag read", nes.TotalCpuCycles, VsyncFlag?1:0);
                //Console.WriteLine("PPUSTATUS read returned VSync flag PC=${0:X4}", nes.Cpu.PC);
                status |= 0x80;


                fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                VsyncFlag = false;
                fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
            }
            /*else
                fs.WriteLine("{0},{1},VBL flag read", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);*/

            if (SpriteHitFlag)
                status |= 0x40;


            /*if (ScrollLatch != 0)
                ;// Console.WriteLine("Resetting SCROLL latch");
            ScrollLatch = 0;
            if (VramAddrLatch != 0)
                ;// Console.WriteLine("Resetting VRAM latch");
            VramAddrLatch = 0;*/
            PpuLatch = false;
            latch = false;
            /*ScrollX = 0;
            VramAddr = 0;*/

            return status;
        }

        // $2003 OAMADDR (W)
        public void WriteOAMAddr(byte val)
        {
            //Console.WriteLine("OAMADDR = ${0:X2}", val);
            OAMAddr = val;
        }

        // $2004 OAMDATA (R/W)
        public byte ReadOAMData() { return SpriteMem[OAMAddr]; }
        public void WriteOAMData(byte val) { SpriteMem[OAMAddr++] = val; }

        private void DumpPpuReg(int reg)
        {
            int coarseX = reg & 0x1f;
            int coarseY = (reg >> 5) & 0x1f;
            int fineY = (reg >> 12) & 0x7;
            int H = (reg >> 10) & 0x1;
            int V = (reg >> 11) & 0x1;
            int scrlX = (byte)((coarseX) << 3 | fineX);
            int scrlY = (byte)((coarseY) << 3 | fineY);
            /*Console.WriteLine("REG=${7:X4}: fineY={0} H={1} V={2} coarseY={3} coarseX={4} ScrollX={5} ScrollY={6}",
                                fineY,
                                H,
                                V,
                                coarseY,
                                coarseX,
                                scrlX,
                                scrlY,
                                reg);*/
        }

        // $2005 PPUSCROLL (W)
        public void WritePpuScroll(byte val)
        {
            // 1st write: SCROLLX
            if (!latch)
            {
                //Console.WriteLine("$2005/1 = ${0:X2}", val);

                loopyT &= ~0x1F;
                loopyT |= (val >> 3);

                fineX = val & 0x7;

                /*Console.WriteLine("T:");
                DumpPpuReg(loopyT);*/
            }
            // 2nd write: SCROLLY
            else
            {
                //Console.WriteLine("$2005/2 = ${0:X2}", val);

                loopyT &= ~0x73E0;
                loopyT |= (val >> 3) << 5; // todo: simplify?
                loopyT |= (val & 0x7) << 12;

                /*Console.WriteLine("T:");
                DumpPpuReg(loopyT);*/
            }

            latch = !latch;

            /*
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
             * */
        }

        // $2006 PPUADDR (W)
        public void WritePpuAddr(byte val)
        {
            if (!latch)
            {
                //Console.WriteLine("$2006/1 = ${0:X2}", val);

                /*loopyT &= ~0xFF00;
                loopyT |= (val & 0x3F) << 8;*/
                loopyT &= ~0xFF00;
                loopyT |= (val & 0xFF) << 8;
                
                DumpPpuReg(loopyT);
            }
            else
            {
                //Console.WriteLine("$2006/2 = ${0:X2}", val);

                loopyT &= ~0xFF;
                loopyT |= val;

                loopyV = loopyT;

                DumpPpuReg(loopyT);
            }

            latch = !latch;

#if false
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
#endif
        }

        // $2007 PPUDATA (R/W)
        public byte ReadPpuData()
        {
            int vramAddr = loopyV & 0x3FFF;

            byte result;

            // Palette read
            if (vramAddr >= 0x3f00)
            {
                DelayedVramRead = Read(0x2f00 | (vramAddr & 0xFF));
                result = Read(vramAddr);
                //Console.WriteLine("R VRAM (pal) ${0:X4}=${1:X2}", VramAddr, result);
            }
            else
            {
                result = DelayedVramRead;
                DelayedVramRead = Read(vramAddr);
                //Console.WriteLine("R VRAM ${0:X4}=${1:X2}", VramAddr, result);
            }


            if ((PpuCtrl & 0x04) != 0)
                vramAddr += 0x20;
            else
                vramAddr++;

            loopyV &= ~0x3FFF;
            loopyV |= (vramAddr & 0x3FFF);

            return result;
        }

        public void WritePpuData(byte data)
        {
            int vramAddr = loopyV & 0x3FFF;

            //nes.Cpu.Paused = true;
            Write(vramAddr, data);

            /*if(data != 0x00 && data != 0x24)
                Console.WriteLine("W VRAM ${0:X4}=${1:X2}", VramAddr, data);*/

            //Console.WriteLine("W VRAM ${0:X4}=${1:X2}", VramAddr, data);

            if ((PpuCtrl & 0x04) != 0)
                vramAddr += 0x20;
            else
                vramAddr++;

            loopyV &= ~0x3FFF;
            loopyV |= (vramAddr & 0x3FFF);
        }

        public string IdentifyBank(int addr)
        {
            int high = MirrorHigh(addr);
            byte[] bank = NameAttributeTables[high];
            if (bank == Nametable1)
                return "A";
            else if (bank == Nametable2)
                return "B";
            else if (bank == Nametable3)
                return "C";
            else
                return "D";
        }

        public string IdentifyBanks()
        {
            return IdentifyBank(0x2000) +
                IdentifyBank(0x2400) +
                IdentifyBank(0x2800) +
                IdentifyBank(0x2C00);
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
            //Console.WriteLine("R VRAM ${0:X4}", addr);

            if(addr >= 0 && addr < 0x2000)
            {
                return PatternTables[addr];
            }
            else if(addr >= 0x2000 && addr < 0x3F00)
            {
                //int nameAttrAddr = addr & 0x0FFF;
                byte data= NameAttributeTables[MirrorHigh(addr)][MirrorLow(addr)];
                if ((addr & 0x3FF) <= 1) 
                    Console.WriteLine("R VRAM ${0:X4}=${1:X2} Mirroring={2} Bank={3} of {4}", addr, data, Mirroring, IdentifyBank(addr), IdentifyBanks());
                return data;
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
            //Console.WriteLine("W VRAM ${0:X4}=${1:X2}", addr, val);

            if(addr >= 0 && addr < 0x2000)
            {
                // Assume this is ROM for now
                //Console.WriteLine("Writing to ROM! O_O");
                // Temp. -- allow writing to ROM, overtest.nes does this -- should be mapper specific
                PatternTables[addr] = val;
            }
            else if(addr >= 0x2000 && addr < 0x3F00)
            {
                // TODO: Proper mirroring here
                /*int nameAttrAddr = addr & 0xFFF;
                NameAttributeTables[nameAttrAddr] = val;*/
                //if ((addr & 0xFF) == 0)
                if ((addr & 0x3FF) <= 1)
                    Console.WriteLine("R VRAM ${0:X4}=${1:X2} Mirroring={2} Bank={3} of {4}", addr, val, Mirroring, IdentifyBank(addr), IdentifyBanks());
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
                            if (result == 3)
                                BGBuffer[screenX] = palette3;
                            else if (result == 2)
                                BGBuffer[screenX] = palette2;
                            else if (result == 1)
                                BGBuffer[screenX] = palette1;
                            /*else
                                BGBuffer[screenX] = 0;*/
                        }

                        ++screenX;
                        //fbPos++;
                    }
                }
            }
        }

        private void RenderSpriteRow(int row, int spriteX, int spriteY, int patternAddr, int palette, bool flipH, bool flipV, bool priority, bool spriteZero)
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
                // TODO: What do do if sprite 0 is already blocked by something?
                if (spriteX + n >= 0 && spriteX + n < ScreenWidth && SpriteBuffer[spriteX+n] == 0)
                {
                    byte bit1 = (byte)((b1 >> cx) & 0x1);
                    byte bit2 = (byte)((b2 >> cx) & 0x1);
                    byte result = (byte)(bit2 << 1 | bit1);

                    if (result != 0)
                    {
                        int color;
                        if (result == 1)
                            color = palette1;
                        else if (result == 2)
                            color = palette2;
                        else// if (result == 3)
                            color = palette3;

                        if (spriteZero && BGBuffer[spriteX + n] != 0)
                            SpriteHitFlag = true; // TODO: Store which cycle it happened on

                        SpriteBuffer[spriteX + n] = color;
                        SpritePriorityBuffer[spriteX + n] = priority;
                        /*OpaqueBuffer[spriteX + n] = true;
                        SpritePriorityBuffer[spriteX + n] = priority;
                        ColorBuffer[spriteX + n] = color;*/
                    }

                    /*if (result == 1)
                        Framebuffer[rowStart + spriteX + n] = palette1;
                    else if (result == 2)
                        Framebuffer[rowStart + spriteX + n] = palette2;
                    else if (result == 3)
                        Framebuffer[rowStart + spriteX + n] = palette3;*/

                }

                cx += dir;
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

                /*if (spritePriority != priority)
                    continue;*/

                int palette = (B2 & 0x3);
                int patternAddr = spritePatternTableAddr + B1 * 16;
                bool flipH = (B2 & 0x40) != 0;
                bool flipV = (B2 & 0x80) != 0;

                RenderSpriteRow(row, x, y, patternAddr, palette, flipH, flipV, spritePriority, i == 0);
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

                /*if (spritePriority != priority)
                    continue;*/

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
                        RenderSpriteRow(row, x, y, patternAddr2, palette, flipH, flipV, spritePriority, i == 0);
                    else
                        RenderSpriteRow(row, x, y + 8, patternAddr1, palette, flipH, flipV, spritePriority, i == 0);
                }
                else
                {
                    if (cy < 8)
                        RenderSpriteRow(row, x, y, patternAddr1, palette, flipH, flipV, spritePriority, i == 0);
                    else
                        RenderSpriteRow(row, x, y + 8, patternAddr2, palette, flipH, flipV, spritePriority, i == 0);
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
            if ((PpuMask & 0x18) != 0)
            {
                loopyV &= ~0x41F;
                loopyV |= (loopyT & 0x41F);
            }

            for (int x = 0; x < ScreenWidth; ++x)
            {
                SpriteBuffer[x] = 0;
                BGBuffer[x] = 0;
                //PriorityBuffer[x] = false;
                //ColorBuffer[x] = 0;
            }

            bool enableBG = (PpuMask & 0x08) !=0;
            bool enableSprites = (PpuMask & 0x10)!=0;

            int baseNTX = (loopyV & (1 << 10)) != 0 ? 1 : 0;
            int baseNTY = (loopyV & (1 << 11)) != 0 ? 1 : 0;
            //int baseNTX = PpuCtrl & 0x1;
            //int baseNTY = (PpuCtrl >> 1) & 0x1;
            bool _8x16 = (PpuCtrl & 0x20) != 0;

            /*baseNTX = 0; baseNTY = 0; ScrollX = 0; ScrollY = 0;*/

            //FillBG(row);

            int scrollY = ScrollY;// % 100; ;// % (30 * 8);


            if (enableBG)
            {
                if (row >= ScreenHeight - scrollY)
                {
                    baseNTY ^= 1;

                    RenderBG(row + scrollY - ScreenHeight, baseNTX, baseNTY, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(row + scrollY - ScreenHeight, baseNTX, baseNTY, -ScrollX + ScreenWidth, row);
                }
                else
                {
                    RenderBG(row + scrollY, baseNTX, baseNTY, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(row + scrollY, baseNTX, baseNTY, -ScrollX + ScreenWidth, row);                    
                }
            }

            bool spriteFlagBefore = SpriteHitFlag;

            if (enableSprites)
            {
                if (_8x16)
                    RenderSprites8x16(row, true);
                else
                    RenderSprites8x8(row, true);
            }

            int defaultBgColor = PpuOutput.Palette[Palette[0] & 0x3f] | 0xFF << 24;


            int fbPos = row*ScreenWidth;
            for (int x = 0; x < ScreenWidth; ++x)
            {
                int color;// = PpuOutput.Palette[Palette[0] & 0x3f] | 0xFF << 24;
                int spriteColor = SpriteBuffer[x];
                int bgColor = BGBuffer[x];

                if(bgColor == 0 && spriteColor == 0)
                    color = defaultBgColor;
                else if (bgColor != 0 && spriteColor == 0)
                    color = bgColor;
                else if (bgColor == 0 && spriteColor != 0)
                    color = spriteColor;
                else // both BG & sprite set
                    color = SpritePriorityBuffer[x] ? bgColor : spriteColor;

                Framebuffer[fbPos++] = color;
            }

            if (spriteFlagBefore != SpriteHitFlag)
            {
                fbPos = row * ScreenWidth;
                for (int x = 0; x < ScreenWidth; ++x)
                    Framebuffer[fbPos++] = 0xFF << 24 | 0xFF0000;
            }

            /*if (enableSprites)
            {
                if (_8x16)
                    RenderSprites8x16(row, false);
                else
                    RenderSprites8x8(row, false);
            }*/

            //Framebuffer[ScreenWidth*row + ScrollX] = 0xFF << 24 | 0xFF0000;
            //Framebuffer[ScreenWidth * row + ScrollY] = 0xFF << 24 | 0x00FF00;
        }

        private void Vsync()
        {
            SpriteHitFlag = false;
            VsyncFlag = true;
            VsyncSignalToMainLoop = true;
            if ((PpuCtrl & 0x80) != 0)
            {
                //fs.WriteLine("{0},1,NMI Triggered", nes.TotalCpuCycles);
                nes.Cpu.NMI();
            }
            else
                ;// Console.WriteLine("VSync NMI Ignored");
        }

        private void BeginScanline()
        {
            int row = ScanlineIndex - VsyncScanlines;

            // If BG & sprites are enabled, V = T @ scanline 0

            if ((PpuMask & 0x18) != 0 && row == 0)
                loopyV = loopyT;

            int coarseX = loopyV & 0x1F;
            int coarseY = (loopyV >> 5) & 0x1f;
            int fineY = (loopyV >> 12) & 0x7;
            ScrollX = (byte)((coarseX) << 3 | fineX);
            ScrollY = (byte)((coarseY) << 3 | fineY);

            if (row >= 0 && row < ScreenHeight)
                RenderRow(row);
            // hack hack
            /*if (row == 31)
                SpriteHitFlag = true;*/
        }

        private void FinishScanline()
        {

            ++ScanlineIndex;
            //ScanlineCycle = 0;

            if (ScanlineIndex >= Scanlines)
            {
                fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                ScanlineCycle = 0;
                ScanlineIndex = 0;
                FrameCycle = 0;
                Vsync();
                //fs.WriteLine("{0},1,VBL set", nes.TotalCpuCycles);
                fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                //Console.WriteLine("NMI @ {0} cycles", nes.TotalCpuCycles);
            }
        }

        private string shiftString = "";

        public void Tick()
        {
            if (ScanlineCycle == 0)
            {
                BeginScanline();
            }

            FrameCycle++;
            ScanlineCycle++;

            // To pass vbl_clear_timing.nes from blargg's ppu tests, this has to be -7 to -1
            // To pass vbl_clear_timing.nes from his vbl_nmi_timing tests, this has to be exactly 10
            // Weird huh? :/
            if (FrameCycle == 2270*3+10)//6200)
            {
                //fs.WriteLine("{0},0,Clear VBL Flag", nes.TotalCpuCycles);
                //Console.WriteLine("Clear NMI Flag @ {0} cycles", nes.TotalCpuCycles);
                fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                VsyncFlag = false;
                fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
            }

            if (FrameCycle == (20 * 341) + 328)
            {
                //Console.WriteLine("{0}", (PpuMask & 0x08) != 0 ? "B" : "-");
                /*shiftString += (PpuMask & 0x08) != 0 ? (cycleSkipToggle?"B":"b") : "-";
                if(shiftString.Length > 10)
                    shiftString = shiftString.Substring(1);
                Console.WriteLine(shiftString);*/
                cycleSkipToggle = !cycleSkipToggle;
                // Skip one cycle per frame if BG is enabled
                if ((PpuMask & 0x08) != 0 && cycleSkipToggle)
                {
                    ++ScanlineCycle;
                }
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
