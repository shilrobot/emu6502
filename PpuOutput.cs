using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Emu6502
{
    public partial class PpuOutput : Form
    {
        private Ppu ppu;

        public PpuOutput(Ppu ppu)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ppu = ppu;
        }

        private Bitmap GetPattern(int addr)
        {
            Bitmap bmp = new Bitmap(8, 8);

            for (int y = 0; y < 8; ++y)
                for (int x = 0; x < 8; ++x)
                {
                    byte b1 = ppu.PatternTables[addr + y];
                    byte b2 = ppu.PatternTables[addr + y + 8];
                    byte bit1 = (byte)((b1 >> (7 - x)) & 0x1);
                    byte bit2 = (byte)((b2 >> (7 - x)) & 0x1);
                    byte result = (byte)(bit2 << 1 | bit1);
                    byte intensity;
                    if (result == 3)
                        intensity = 255;
                    else if (result == 2)
                        intensity = 160;
                    else if (result == 1)
                        intensity = 80;
                    else
                        intensity = 0;

                    if (result == 0)
                        bmp.SetPixel(x, y, Color.FromArgb(0,0,0,0));
                    else
                        bmp.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));
                }

            return bmp;
        }

        private void DrawNameTable(Graphics g, int addr, int tx, int ty)
        {
            for(int y=0; y<30; ++y)
                for (int x = 0; x < 32; ++x)
                {
                    byte b = ppu.Read(addr);;
                    Bitmap chunk = GetPattern(b);
                    g.DrawImage(chunk, tx + x*8, ty + y*8);
                }
        }

        private void PpuOutput_Paint(object sender, PaintEventArgs e)
        {
            // TODO: Now we have to actually make this fancy so we can see what the shit is going on.

            int baseNametableAddr = 0;
            if ((ppu.PpuCtrl & 0x3) == 0)
                baseNametableAddr = 0x2000;
            else if ((ppu.PpuCtrl & 0x3) == 1)
                baseNametableAddr = 0x2400;
            else if ((ppu.PpuCtrl & 0x3) == 2)
                baseNametableAddr = 0x2800;
            else // 3
                baseNametableAddr = 0x2C00;

            int spritePatternTableAddr = 0x0000;
            if ((ppu.PpuCtrl & 0x08) != 0)
                spritePatternTableAddr = 0x1000;

            bool eightBySixteen = false;
            if ((ppu.PpuCtrl & 0x20) != 0)
                eightBySixteen = true;

            bool enableBG = (ppu.PpuMask & 0x08) != 0;
            bool enableSprites = (ppu.PpuMask & 0x10) != 0;

            Graphics g = e.Graphics;
            g.SetClip(new Rectangle(0, 0, 256, 224));
            g.Clear(Color.Black);

            /*String hud = String.Format("Base Nametable Addr: ${0:X4}\n"+
                                        "8x8 Sprite Pat Table Addr: ${1:X4}\n"+
                                        "Sprite Size: {2}\n"+
                                        "Enable BG: {3}\n"+
                                        "Enable Sprites: {4}\n"+
                                        "BG Scroll X: {5}\n"+
                                        "BG Scroll Y: {6}\n\n",
                                        baseNametableAddr,
                                        spritePatternTableAddr,
                                        eightBySixteen ? "8x16":"8x8",
                                        enableBG,
                                        enableSprites,
                                        ppu.ScrollX,
                                        ppu.ScrollY);

            for (int i = 0; i < 64; ++i)
            {
                byte B0 = ppu.SpriteMem[i * 4 + 0];
                byte B1 = ppu.SpriteMem[i * 4 + 1];
                byte B2 = ppu.SpriteMem[i * 4 + 2];
                byte B3 = ppu.SpriteMem[i * 4 + 3];

               // int x = B3;
                //int y = B0;
                //Bitmap chunk = GetPattern((HighBank8x8 ? 0x1000 : 0x0000) + (B1 * 16));
                //g.DrawImage(chunk, x, y);
                hud += String.Format("Spr{0:00} ${1:X2} ${2:X2} ({3},{4})\n", i, B1, B2, B3, B0);
            }

            g.DrawString(hud, new Font("Courier New", 8), new SolidBrush(Color.White), 0, 0);*/

            /*
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            DrawNameTable(g, 0x2000, 0, 0);

            bool EightBySixteen = (ppu.PpuCtrl & 0x20) != 0;
            bool HighBank8x8 = (ppu.PpuCtrl & 0x08) != 0;
            */

            //DrawNameTable(g, baseNametableAddr, 0, 0);

            
            for (int i = 0; i < 64; ++i)
            {
                byte B0 = ppu.SpriteMem[i * 4 + 0];
                byte B1 = ppu.SpriteMem[i * 4 + 1];
                byte B2 = ppu.SpriteMem[i * 4 + 2];
                byte B3 = ppu.SpriteMem[i * 4 + 3];

                int x = B3;
                int y = B0;
                if (y >= 0xEF)
                    continue;
                bool flipH = (B2 & 0x40) != 0;
                bool flipV = (B2 & 0x80) != 0;
                Bitmap chunk = GetPattern(spritePatternTableAddr + B1 * 16);
                if (flipH)
                    chunk.RotateFlip(RotateFlipType.RotateNoneFlipX);
                if (flipV)
                    chunk.RotateFlip(RotateFlipType.RotateNoneFlipY);
                g.DrawImage(chunk, x, y);
            }

            /*int x = 0, y = 0;
            for (int addr=0; addr < 0x2000; addr += 16)
            {
                Bitmap chunk = GetPattern(addr);
                g.DrawImage(chunk, x * 8, y * 8);
                x++;
                if (x > 16)
                {
                    ++y;
                    x = 0;
                }
            }*/

            return;
        }
    }
}
