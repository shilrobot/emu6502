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

        private Bitmap GetPattern(int num)
        {

        }

        private void PpuOutput_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);
            
            Bitmap bmp = new Bitmap(8,8);
            int tileX = 0;
            int tileY = 0;
            int addr = 0x0000;
            while (addr < 0x2000)
            {
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
                g.DrawImage(bmp, tileX * 8, tileY * 8);
                addr += 16;
                tileX++;
                if (tileX == 8)
                {
                    tileX = 0;
                    tileY++;
                }
            }
        }
    }
}
