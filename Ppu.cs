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
        //public byte[] PatternTables;
        public byte[][] PatternTables;
        public byte[][] NameAttributeTables;
        public byte[] Palette;
        public byte[] SpriteMem; // OAM stuff

        private bool paletteDirty;
        private int[] PaletteCache = new int[32];

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

        public int WaitCycles = 0;
        public int FrameCycle = 0;
        public int ScanlineIndex = 0; // Essentially "Y" counter
        //public int ScanlineCycle = 0; // Essentially "X" counter

        //private StreamWriter fs = new StreamWriter("nmi.csv");

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

        private const int Cycle_Start = 0;
        private const int Cycle_RaiseNMI = 3;
        private const int Cycle_ClearVSync = 2270 * 3;
        //private const int Cycle_SkipFrame = (19 * 341) + 328;
        private const int Cycle_FirstVisibleScanline = (20 * 341);

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
            //ScanlineCycle = 0;
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
            // TODO: Only call this from the mapper's init
            PatternTables = new byte[2][];
            PatternTables[0] = new byte[0x1000];
            PatternTables[1] = new byte[0x1000];
            SetMirroring(nes.Rom.MirrorType);

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

            if ((PpuCtrl & 0x80) != 0)
                nes.RecordEvent("Ppu.Vbl.NmiDisabled");
            else
                nes.RecordEvent("Ppu.Vbl.NmiEnabled");


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
                Console.WriteLine("PPUSTATUS read returned VSync flag PC=${0:X4} @ {1} cy", nes.Cpu.PC, nes.TotalCpuCycles);
                status |= 0x80;

                nes.RecordEvent("Ppu.Vbl.FlagRead");

                //fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                VsyncFlag = false;
                //fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
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


        public static int MirrorHigh(int addr)
        {
            return (addr & 0xC00) >> 10;
        }

        public static int MirrorLow(int addr)
        {
            return (addr & 0x3FF);
        }

        public static int PatternHigh(int addr)
        {
            return (addr & 0x1000) >> 12;
        }

        public static int PatternLow(int addr)
        {
            return (addr & 0x0FFF);
        }

        public byte Read(int addr)
        {
            addr = addr & 0x3FFF;
            //Console.WriteLine("R VRAM ${0:X4}", addr);

            if(addr >= 0 && addr < 0x2000)
            {
                return PatternTables[PatternHigh(addr)][PatternLow(addr)];
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
                //PatternTables[addr] = val;
                // TODO: Check if this is ROM or RAM... depends on mapper
                PatternTables[PatternHigh(addr)][PatternLow(addr)] = val;
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
                paletteDirty = true;
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

        private static int[] tempPalette = new int[4];

        private void UpdatePaletteCache()
        {

            paletteDirty = false;

            for(int i=0; i<32; ++i)
                PaletteCache[i] = PpuOutput.Palette[Palette[i] & 0x3f] | 0xFF << 24;
        }

        private void RenderBG(int srcRow, int ntX, int ntY, int screenX, int screenY)
        {
            int rowStart = screenY << 8; // Multiply by 256
            int bgPatternStart = (PpuCtrl & 0x10) != 0 ? 0x1000 : 0x0000;

            //int nametableStart = 0x2000;

            if (paletteDirty)
                UpdatePaletteCache();

            //tempPalette[0] = 0;

            int mirrorHigh = ntY << 1 | ntX;//MirrorHigh(nameTableOffset);

            int tileY = srcRow / 8;
            int subtileY = srcRow % 8;
            int attrByteY = tileY >> 2;/// 4;
            int attrByteSubY = tileY & 0x3;
            int nibbleY = (attrByteSubY >> 1) & 0x1;

            for (int tileX = 0; tileX < 32; ++tileX)
            {
                int attrByteX = tileX >> 2;/// 4;
                int attrByteSubX = tileX & 0x3;
                int attrAddr = 0x3c0 + attrByteX + (attrByteY <<3);
                byte attrByte = NameAttributeTables[mirrorHigh][attrAddr];
                int nibbleX = (attrByteSubX >> 1) & 0x1;
                int shiftAmt = (nibbleY << 2 | nibbleX << 1);
                int paletteAddr = ((attrByte >> shiftAmt) & 0x3) <<2;
                //int palette0 = PpuOutput.Palette[Palette[0] & 0x3f] | 0xFF << 24;
                /*int palette1 = PpuOutput.Palette[Palette[paletteAddr + 1] & 0x3f] | 0xFF << 24;
                int palette2 = PpuOutput.Palette[Palette[paletteAddr + 2] & 0x3f] | 0xFF << 24;
                int palette3 = PpuOutput.Palette[Palette[paletteAddr + 3] & 0x3f] | 0xFF << 24;*/
                /*tempPalette[1] = PaletteCache[paletteAddr+1];//PpuOutput.Palette[Palette[paletteAddr + 1] & 0x3f] | 0xFF << 24;
                tempPalette[2] = PaletteCache[paletteAddr+2];//PpuOutput.Palette[Palette[paletteAddr + 2] & 0x3f] | 0xFF << 24;
                tempPalette[3] = PaletteCache[paletteAddr + 3];//PpuOutput.Palette[Palette[paletteAddr + 3] & 0x3f] | 0xFF << 24;
                */
                int pattern = NameAttributeTables[mirrorHigh][(tileY << 5) + tileX];

                int patternAddr = bgPatternStart + (pattern << 4);

                int addr1 = patternAddr + subtileY;
                int addr2 = patternAddr + subtileY + 8;
                int hi = PatternHigh(addr1);
                int lo = PatternLow(addr1);
                byte[] pt = PatternTables[hi];
                /*byte b1 = pt[lo];
                byte b2 = pt[lo + 8];*/
                ushort b1b2 = (ushort)(pt[lo] << 8 | pt[lo + 8]);

                int targetX = screenX + 8;
                if (targetX >= ScreenWidth)
                    targetX = ScreenWidth;
                while (screenX < targetX)
                {
                    /*if (screenX >= ScreenWidth)
                        return;*/

                    if (screenX >= 0)
                    {
                        int result = (b1b2 >> 15) | ((b1b2 >> 6) & 0x2);
                        if (result != 0)
                            BGBuffer[screenX] = PaletteCache[paletteAddr + result];
                    }

                    ++screenX;
                    b1b2 <<= 1;
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

            int addr1 = patternAddr + cy;
            int addr2 = patternAddr + cy + 8;
            byte b1 = PatternTables[PatternHigh(addr1)][PatternLow(addr1)];
            byte b2 = PatternTables[PatternHigh(addr2)][PatternLow(addr2)];

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
                        {
                            nes.RecordEvent("Ppu.Sprite0Hit");
                            SpriteHitFlag = true; // TODO: Store which cycle it happened on
                        }

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

            int uberScrollY = baseNTY << 8 | ScrollY;

            //int baseNTX = PpuCtrl & 0x1;
            //int baseNTY = (PpuCtrl >> 1) & 0x1;
            bool _8x16 = (PpuCtrl & 0x20) != 0;

            //baseNTY = uberScrollY >= 30 * 8 ? 1 : 0;
            //int scrollY = uberScrollY % (30 * 8);

            /*baseNTX = 0; baseNTY = 0; ScrollX = 0; ScrollY = 0;*/

            //FillBG(row);

            // Hrm, not exactly, but getting there.
            // It looks like stuff should be a bit further down... (by 16px)
            int scrollY = ScrollY;// % 100; ;// % (30 * 8);

            int virtualRow = row + ScrollY + baseNTY*(30*8);
            int rowModulo = virtualRow % (30 * 8);

            if (enableBG)
            {
                /*if (virtualRow >= 0 && virtualRow < ScreenHeight)
                {
                    RenderBG(rowModulo, baseNTX, 0, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(rowModulo, baseNTX, 0, -ScrollX + ScreenWidth, row);
                }
                else if (virtualRow >= ScreenHeight && virtualRow < ScreenHeight*2)
                {
                    RenderBG(rowModulo, baseNTX, 1, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(rowModulo, baseNTX, 1, -ScrollX + ScreenWidth, row);
                }
                else if (virtualRow >= ScreenHeight*2 && virtualRow < ScreenHeight*3)
                {
                    RenderBG(rowModulo, baseNTX, 0, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(rowModulo, baseNTX, 0, -ScrollX + ScreenWidth, row);
                }
                else 
                {
                    RenderBG(rowModulo, baseNTX, 1, -ScrollX, row);
                    baseNTX ^= 1;
                    RenderBG(rowModulo, baseNTX, 1, -ScrollX + ScreenWidth, row);
                }*/
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

            /*if (spriteFlagBefore != SpriteHitFlag)
            {
                fbPos = row * ScreenWidth;
                for (int x = 0; x < ScreenWidth; ++x)
                    Framebuffer[fbPos++] = 0xFF << 24 | 0xFF0000;
            }*/

            /*if (enableSprites)
            {
                if (_8x16)
                    RenderSprites8x16(row, false);
                else
                    RenderSprites8x8(row, false);
            }*/

            /*Framebuffer[ScreenWidth*row + ScrollX] = 0xFF << 24 | 0xFF0000;
            Framebuffer[ScreenWidth * row + ScrollY] = 0xFF << 24 | 0x00FF00;*/
        }

        private void RaiseVsyncNmi()
        {
            if ((PpuCtrl & 0x80) != 0)
            {
                nes.RecordEvent("Ppu.Vbl.NmiRaised");
                //fs.WriteLine("{0},1,NMI Triggered", nes.TotalCpuCycles);
                nes.Cpu.NMI();
            }
            else
                nes.RecordEvent("Ppu.Vbl.NmiNotRaised");
        }

        private void Vsync()
        {
            SpriteHitFlag = false;
            VsyncFlag = true;
            VsyncSignalToMainLoop = true;
            /*if ((PpuCtrl & 0x80) != 0)
            {
                nes.RecordEvent("Ppu.Vbl.NmiRaised");
                //fs.WriteLine("{0},1,NMI Triggered", nes.TotalCpuCycles);
                nes.Cpu.NMI();
            }
            else
                nes.RecordEvent("Ppu.Vbl.NmiNotRaised");*/

            nes.RecordEvent("Ppu.Vbl.Total");
            // Console.WriteLine("VSync NMI Ignored");
        }

        private void BeginScanline()
        {
            int row = ScanlineIndex;// -VsyncScanlines;

            // If BG & sprites are enabled, V = T @ scanline 0

            // TODO: Actually do this imemdiately at *CYCLE ZERO* not at cycle zero of visible row 0
            if ((PpuMask & 0x18) != 0 && row == 0)
                loopyV = loopyT;

            int coarseX = loopyV & 0x1F;
            int coarseY = (loopyV >> 5) & 0x1f;
            int fineY = (loopyV >> 12) & 0x7;
            ScrollX = (byte)((coarseX) << 3 | fineX);
            ScrollY = (byte)((coarseY) << 3 | fineY);

           if (row >= 0 && row < ScreenHeight)
                RenderRow(row);
           ++ScanlineIndex;
        }

#if false
        private void FinishScanline()
        {

            ++ScanlineIndex;
            //ScanlineCycle = 0;

            if (ScanlineIndex >= Scanlines)
            {
                //fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                ScanlineCycle = 0;
                ScanlineIndex = 0;
                FrameCycle = 0;
                Vsync();
                //fs.WriteLine("{0},1,VBL set", nes.TotalCpuCycles);
                //fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                //Console.WriteLine("NMI @ {0} cycles", nes.TotalCpuCycles);
            }
        }
#endif

        private string shiftString = "";

        public void Tick()
        {
            FrameCycle = FrameCycle % (262 * 341);

            if (FrameCycle >= Cycle_FirstVisibleScanline && FrameCycle % 341 == 0)
            {
                //Console.WriteLine("#{0} Scanline", FrameCycle);
                BeginScanline();
                WaitCycles = 341;
            }
            else if (FrameCycle == Cycle_Start)
            {
                //Console.WriteLine("#{0} Start", FrameCycle);
                ScanlineIndex = 0;
                //VsyncFlag = true;
                Vsync();
                WaitCycles = Cycle_RaiseNMI - Cycle_Start;
            }
            else if (FrameCycle == Cycle_RaiseNMI)
            {
                //Console.WriteLine("#{0} Raise NMI", FrameCycle);
                //Console.WriteLine("VSYNC");
                //Vsync();
                RaiseVsyncNmi();
                WaitCycles = Cycle_ClearVSync - Cycle_RaiseNMI;
            }
            else if (FrameCycle == Cycle_ClearVSync)
            {
                //Console.WriteLine("#{0} Clear VSync", FrameCycle);
                VsyncFlag = false;
                cycleSkipToggle = !cycleSkipToggle;
                WaitCycles = Cycle_FirstVisibleScanline - Cycle_ClearVSync;

                // Skip one cycle per frame if BG is enabled
                if ((PpuMask & 0x08) != 0 && cycleSkipToggle)
                {
                    FrameCycle++;
                    WaitCycles--;
                }
            }
            else
                throw new InvalidOperationException("PPU ticking at non-sync point!");

            //Console.WriteLine("Wait Cycles={0} (until {1})", WaitCycles, FrameCycle + WaitCycles);
            /*if (FrameCycle == Cycle_RaiseNMI)
            {
                BeginScanline();
                WaitCycles = Cycle_ClearRais
            }
            else if (FrameCycle == 3)
                RaiseVsyncNmi();
            else if (FrameCycle == 2270 * 3 + 10)
                VsyncFlag = false;
            else if (FrameCycle == (20 * 341) + 328)
            {
                cycleSkipToggle = !cycleSkipToggle;
                // Skip one cycle per frame if BG is enabled
                if ((PpuMask & 0x08) != 0 && cycleSkipToggle)
                {
                    ++FrameCycle;
                }
            }*/

#if false
            if (ScanlineCycle == 0)
            {
                BeginScanline();
            }

            if (FrameCycle == 3)
                RaiseVsyncNmi();

            FrameCycle++;
            ScanlineCycle++;

            // To pass vbl_clear_timing.nes from blargg's ppu tests, this has to be -7 to -1
            // To pass vbl_clear_timing.nes from his vbl_nmi_timing tests, this has to be exactly 10
            // Weird huh? :/
            if (FrameCycle == 2270*3+10)//6200)
            {
                //fs.WriteLine("{0},0,Clear VBL Flag", nes.TotalCpuCycles);
                //Console.WriteLine("Clear NMI Flag @ {0} cycles", nes.TotalCpuCycles);
                //fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
                VsyncFlag = false;
                //fs.WriteLine("{0},{1}", nes.TotalCpuCycles, VsyncFlag ? 1 : 0);
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
#endif
        }

        /*private void ShowFrame()
        {
            PpuOutput output = new PpuOutput();
            output.ShowDialog();
        }*/
    }
}
