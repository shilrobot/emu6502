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
    public partial class PpuDebug : Form
    {
        private Ppu ppu;

        public PpuDebug(Ppu ppu)
        {
            InitializeComponent();
            this.ppu = ppu;
            this.DoubleBuffered = true;
            //this.ClientSize = new Size(16 * 8, 32 * 8);
        }

        private Bitmap GetPattern(int addr)
        {
            Bitmap bmp = new Bitmap(8, 8);

            for (int y = 0; y < 8; ++y)
                for (int x = 0; x < 8; ++x)
                {
                    int addr1 = addr + y;
                    int addr2 = addr + y + 8;
                    // Note: patternhigh should be same for both here
                    byte b1 = ppu.PatternTables[Ppu.PatternHigh(addr1)][Ppu.PatternLow(addr1)];
                    byte b2 = ppu.PatternTables[Ppu.PatternHigh(addr2)][Ppu.PatternLow(addr2)];
                    byte bit1 = (byte)((b1 >> (7 - x)) & 0x1);
                    byte bit2 = (byte)((b2 >> (7 - x)) & 0x1);
                    byte result = (byte)(bit2 << 1 | bit1);
                    int color;
                    if (result == 3)
                        color = 0xFF << 24 | 0xFF0000;
                    else if (result == 2)
                        color = 0xFF << 24 | 0x00FF00;
                    else if (result == 1)
                        color = 0xFF << 24 | 0x0000FF;
                    else
                        color = 0;

                    if (result == 0)
                        bmp.SetPixel(x, y, Color.FromArgb(0,0,0,0));
                    else
                        bmp.SetPixel(x, y, Color.FromArgb(color));
                    /*if (result == 0)
                        bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    else
                    {
                        int paletteEntry = ppu.Read(paletteAddr + result) % 64; // todo: bits
                        bmp.SetPixel(x, y, Color.FromArgb(0xFF << 24 | Palette[paletteEntry]));
                    }*/
                }

            return bmp;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            const int BlockSize = 13;
            SolidBrush fntBrush = new SolidBrush(Color.White);

            Font fnt = new Font("Courier New", 10);
            int x = 0, y = 0;
            for (int addr = 0x2000; addr < 0x3000; addr += 0x0400)
            {
                g.DrawString(String.Format("${0:X4}", addr), fnt, fntBrush, 16*4 + 8*16, y * BlockSize);
                y++;
                for (int i = 0; i < 32 * 30; ++i)
                {
                    g.DrawString(String.Format("{0:X2}", ppu.Read(addr+i)), fnt, fntBrush, 16*4 + 8*16 + x * BlockSize*2, y * BlockSize);
                    
                    x++;
                    if (x == 32)
                    {
                        x = 0;
                        y++;
                    }
                }
            }

            int tx = 0;
            int ty = 0;
            for (int i = 0; i < 32; ++i)
            {
                byte palIdx = ppu.Palette[i];//;.Read(0x3f00 + i);
                Color c = Color.FromArgb((int)(0xFF000000 | PpuOutput.Palette[palIdx]));
                Rectangle r = new Rectangle(tx * 16, ty * 16, 16, 16);
                g.FillRectangle(new SolidBrush(c), r);
                tx++;
                if (tx >= 4)
                {
                    tx = 0;
                    ty++;
                }
            }

            //int x=0,y=0;
            x = 0; y = 0;
            for (int addr = 0; addr < 0x2000; addr += 16)
            {
                Bitmap bmp = GetPattern(addr);
                g.DrawImage(bmp, 16*4 + x * 8, y * 8);
                x++;
                if (x == 16)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }
}
