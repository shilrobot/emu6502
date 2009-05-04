using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Emu6502
{
    public partial class PpuOutput : Form
    {
        public static int[] Palette = new int[]
        {
            // 0x00
            0x757575,
            0x271b8f,
            0x0000ab,
            0x47009f,
            0x8f0077,
            0xab0013,
            0xa70000,
            0x7f0b00,
            0x432f00,
            0x004700,
            0x004100,
            0x003f17,
            0x1b3f5f,
            0x000000,
            0x000000,
            0x000000,

            // 0x10
            0xbcbcbc,
            0x0073ef,
            0x233bef,
            0x8300f3,
            0xbf00bf,
            0xe7005b,
            0xdb2b00,
            0xcb4f0f,
            0x8b7300,
            0x009700,
            0x00ab00,
            0x00933b,
            0x00838b,
            0x000000,
            0x000000,
            0x000000,

            // 0x20
            0xffffff,
            0x3fbfff,
            0x5f97ff,
            0xa78bfd,
            0xf77bff,
            0xff77b7,
            0xff7763,
            0xff9b3b,
            0xf3bf3f,
            0x83d313,
            0x4fdf4b,
            0x58f898,
            0x00ebdb,
            0x000000,
            0x000000,
            0x000000,

            // 0x30
            0xffffff,
            0xabe7ff,
            0xc7d7ff,
            0xd7cbff,
            0xffc7ff,
            0xffc7db,
            0xffbfb3,
            0xffdbab,
            0xffe7a3,
            0xe3ffa3,
            0xabf3bf,
            0xb3ffcf,
            0x9ffff3,
            0x000000,
            0x000000,
            0x000000,
        };

        private Nes nes;
        private Ppu ppu;
        private Bitmap bmp = new Bitmap(Ppu.ScreenWidth, Ppu.ScreenHeight);

        public PpuOutput(Nes nes)
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ClientSize = new Size(Ppu.ScreenWidth, Ppu.ScreenHeight);
            this.ppu = nes.Ppu;
            this.nes = nes;
        }

        private Bitmap GetPattern(int addr, int paletteAddr)
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
                    /*byte intensity;
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
                        bmp.SetPixel(x, y, Color.FromArgb(intensity, intensity, intensity));*/
                    if (result == 0)
                        bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    else
                    {
                        int paletteEntry = ppu.Read(paletteAddr + result)%64; // todo: bits
                        bmp.SetPixel(x, y, Color.FromArgb(0xFF << 24 | Palette[paletteEntry]));
                    }
                }

            return bmp;
        }

        private void DrawNameTable(Graphics g, int addr, int tx, int ty)
        {
            for(int y=0; y<30; ++y)
                for (int x = 0; x < 32; ++x)
                {
                    byte b = ppu.Read(addr++);;
                    Bitmap chunk = GetPattern(0x1000 + b*16, 0x3F00);
                    g.DrawImage(chunk, tx + x*8, ty + y*8);
                }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        /*
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode & Keys.KeyCode)
            {
                case Keys.Z:
                    nes.Controller1.A = true;
                    break;
                case Keys.X:
                    nes.Controller1.B = true;
                    break;
                case Keys.Return:
                    nes.Controller1.Select = true;
                    break;
                case Keys.Space:
                    nes.Controller1.Start = true;
                    break;
                case Keys.Left:
                    nes.Controller1.Left = true;
                    break;
                case Keys.Right:
                    nes.Controller1.Right = true;
                    break;
                case Keys.Up:
                    nes.Controller1.Up = true;
                    break;
                case Keys.Down:
                    nes.Controller1.Down = true;
                    break;
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode & Keys.KeyCode)
            {
                case Keys.Z:
                    nes.Controller1.A = false;
                    break;
                case Keys.X:
                    nes.Controller1.B = false;
                    break;
                case Keys.Return:
                    nes.Controller1.Select = false;
                    break;
                case Keys.Space:
                    nes.Controller1.Start = false;
                    break;
                case Keys.Left:
                    nes.Controller1.Left = false;
                    break;
                case Keys.Right:
                    nes.Controller1.Right = false;
                    break;
                case Keys.Up:
                    nes.Controller1.Up = false;
                    break;
                case Keys.Down:
                    nes.Controller1.Down = false;
                    break;
            }

            base.OnKeyUp(e);
        }
         * */

        private bool toggle = false;

        private unsafe void PpuOutput_Paint(object sender, PaintEventArgs e)
        {
            // TODO: Now we have to actually make this fancy so we can see what the shit is going on.

            /*int baseNametableAddr = 0;
            if ((ppu.PpuCtrl & 0x3) == 0)
                baseNametableAddr = 0x2000;
            else if ((ppu.PpuCtrl & 0x3) == 1)
                baseNametableAddr = 0x2400;
            else if ((ppu.PpuCtrl & 0x3) == 2)
                baseNametableAddr = 0x2800;
            else // 3
                baseNametableAddr = 0x2C00;*/

            /*int spritePatternTableAddr = 0x0000;
            if ((ppu.PpuCtrl & 0x08) != 0)
                spritePatternTableAddr = 0x1000;

            bool eightBySixteen = false;
            if ((ppu.PpuCtrl & 0x20) != 0)
                eightBySixteen = true;

            bool enableBG = (ppu.PpuMask & 0x08) != 0;
            bool enableSprites = (ppu.PpuMask & 0x10) != 0;*/

            Graphics g = e.Graphics;
            //g.SetClip(new Rectangle(0, 0, 256, 262)); // NTSC clipping
            //g.Clear(Color.Blue);

            /*Bitmap bmp = new Bitmap(Ppu.ScreenWidth, Ppu.ScreenHeight);
            for(int y = 0; y < Ppu.ScreenHeight; ++y)
                for(int x=0; x<Ppu.ScreenWidth; ++x)
                    bmp.SetPixel(x,y,Color.FromArgb(ppu.Framebuffer[x + y*Ppu.ScreenWidth]));
            g.DrawImage(bmp, 0, 0);*/

            BitmapData data = 
                bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                 System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // Assume we have zero stride (not exactly safe?)
            Marshal.Copy(ppu.Framebuffer, 0, data.Scan0, bmp.Width * bmp.Height);
            bmp.UnlockBits(data);
            g.DrawImageUnscaledAndClipped(bmp, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height));

            String hud = String.Format("FPS {0:0.0}", nes.FPS);//\n{1}", nes.FPS, ppu.Mirroring);
            g.DrawString(hud, new Font("Courier New", 8), new SolidBrush(Color.White), 0, 0);

#if false

            String hud = String.Format("Base Nametable Addr: ${0:X4}\n"+
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

            /*for (int i = 0; i < 64; ++i)
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
            }*/


            /*
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            DrawNameTable(g, 0x2000, 0, 0);

            bool EightBySixteen = (ppu.PpuCtrl & 0x20) != 0;
            bool HighBank8x8 = (ppu.PpuCtrl & 0x08) != 0;
            */
#endif

            /*DrawNameTable(g, baseNametableAddr, -ppu.ScrollX, -ppu.ScrollY);
            DrawNameTable(g, baseNametableAddr == 0x2000 ? 0x2400 : 0x2000, -ppu.ScrollX + 256, -ppu.ScrollY);*/
            /*for (int i = 0; i < 64; ++i)
            {
                byte B0 = ppu.SpriteMem[i * 4 + 0];
                byte B1 = ppu.SpriteMem[i * 4 + 1];
                byte B2 = ppu.SpriteMem[i * 4 + 2];
                byte B3 = ppu.SpriteMem[i * 4 + 3];

                int x = B3;
                int y = B0;
                if (y >= 0xEF)
                    continue;
                int palette = (B2 & 0x3);
                bool flipH = (B2 & 0x40) != 0;
                bool flipV = (B2 & 0x80) != 0;
                Bitmap chunk = GetPattern(spritePatternTableAddr + B1 * 16, 0x3F10 + palette*4);
                if (flipH)
                    chunk.RotateFlip(RotateFlipType.RotateNoneFlipX);
                if (flipV)
                    chunk.RotateFlip(RotateFlipType.RotateNoneFlipY);
                g.DrawImage(chunk, x, y);
            }*/


            /*int tx = 0;
            int ty = 0;
            for (int i = 0; i < 32; ++i)
            {
                byte palIdx = ppu.Palette[i];//;.Read(0x3f00 + i);
                Color c = Color.FromArgb((int)(0xFF000000 | Palette[palIdx]));
                Rectangle r = new Rectangle(tx * 16, ty * 16, 16, 16);
                g.FillRectangle(new SolidBrush(c), r);
                tx++;
                if (tx >= 4)
                {
                    tx = 0;
                    ty++;
                }
            }*/
            

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
                //hud += String.Format("Spr{0:00} ${1:X2} ${2:X2} ({3},{4})\n", i, B1, B2, B3, B0);
                int x = B3;
                int y = B0 + 1;
                Color color = Color.FromArgb(0,255,0);
                if(i == 0 && toggle)
                    color = Color.Black;
                g.DrawRectangle(new Pen(color, 1), new Rectangle(x, y, 8, (ppu.PpuCtrl & 0x20) != 0 ? 16 : 8));
            }

            toggle = !toggle;

            //g.DrawString(hud, new Font("Courier New", 8), new SolidBrush(Color.White), 0, 0);
//#endif
            return;
        }
    }
}
