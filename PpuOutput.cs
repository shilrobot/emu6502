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
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            //DrawNameTable(g, 0x2000, 0, 0);

            bool EightBySixteen = (ppu.PpuCtrl & 0x20) != 0;
            bool HighBank8x8 = (ppu.PpuCtrl & 0x08) != 0;

            for (int i = 0; i < 64; ++i)
            {
                byte B0 = ppu.SpriteMem[i * 4 + 0];
                byte B1 = ppu.SpriteMem[i * 4 + 1];
                byte B2 = ppu.SpriteMem[i * 4 + 2];
                byte B3 = ppu.SpriteMem[i * 4 + 3];

                int x = B3;
                int y = B0;
                Bitmap chunk = GetPattern((HighBank8x8 ? 0x1000 : 0x0000) + (B1 * 16) );
                g.DrawImage(chunk, x, y);
            }

            return;
        }
    }
}
