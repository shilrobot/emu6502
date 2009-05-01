using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Emu6502
{
    public struct DecodedInstr
    {
        public int Address;
        public byte[] Bytes;
        public string String;
    }

    public partial class DisassemblyWindow : Control
    {
        public Nes Nes { get { return nes; } set { nes = value; Disassemble();  } }
        private Nes nes = null;
        private Font fnt;
        public int InstrNum { get; set; }
        public int StartAddress { get; set; }
        private List<DecodedInstr> decodedInstrs = new List<DecodedInstr>();
        private Image bpImage;
        private Image arrowImage;
        public int SelectedIndex; // TODO: Property
        public int Scroll = 0; // TODO: Property
        private bool mouseDown = false;
        private Dictionary<ushort, int> decodeMap = new Dictionary<ushort, int>();

        public DisassemblyWindow()
        {
            InitializeComponent();
            Init();
        }

        public DisassemblyWindow(Nes nes)
        {
            InitializeComponent();
            Init();

            Nes = nes;
        }

        private void Init()
        {
            this.DoubleBuffered = true;
            this.TabStop = true;
            this.Enabled = true;
            try
            {
                bpImage = new Bitmap("breakpoint.png");
                arrowImage = new Bitmap("arrow.png");
            }
            catch
            {
            }
            
            fnt = new Font("Courier New", 10.0f);
        }

        public void SelectAddress(ushort address)
        {
            if (decodeMap.ContainsKey(address))
            {
                SelectedIndex = decodeMap[address];
                MakeSelectionVisible();
                Invalidate();
            }
        }

        private void Disassemble()
        {
            if (nes == null)
                return;

            decodedInstrs.Clear();
            decodeMap.Clear();
            int addr = 0x8000;
            while (addr <= 0xFFFF)
            {
                int count = 0x10000 - addr;
                StringBuilder sb = new StringBuilder();
                int bytes;

                if (Disassembler.DisassembleOne(nes.Mem, addr, count, sb, out bytes))
                {
                    DecodedInstr instr = new DecodedInstr();
                    instr.Address = addr;
                    instr.Bytes = new byte[bytes];
                    for (int i = 0; i < bytes; ++i)
                        instr.Bytes[i] = nes.Mem.Read(addr + i);
                    instr.String = sb.ToString();
                    decodedInstrs.Add(instr);
                    decodeMap[(ushort)addr] = decodedInstrs.Count - 1;

                    addr += bytes;
                }
                else
                {
                    byte value = nes.Mem.Read(addr);
                    Disassembler.DB(sb, addr, value);

                    DecodedInstr instr = new DecodedInstr();
                    instr.Address = addr;
                    instr.Bytes = new byte[1];
                    instr.Bytes[0] = value;
                    instr.String = sb.ToString();
                    decodedInstrs.Add(instr);
                    decodeMap[(ushort)addr] = decodedInstrs.Count - 1;
                    
                    addr += 1;
                }
            }

            Console.WriteLine("Decoded {0} instrs for disassembler", decodedInstrs.Count);
        }

        private int GetLineHeight(Graphics g)
        {
            SizeF size = g.MeasureString("X", fnt);
            return (int)Math.Ceiling(size.Height);
        }

        public void MakeSelectionVisible()
        {
            int lineHeight = GetLineHeight(CreateGraphics());
            int lineTop = lineHeight * SelectedIndex;
            int lineBottom = lineHeight * (SelectedIndex + 1);
            if (lineTop - Scroll <= 0)
            {
                Scroll = lineTop;
                Invalidate();
            }
            else if (lineBottom - Scroll >= ClientSize.Height)
            {
                Scroll = lineTop - ClientSize.Height + lineHeight;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (nes == null)
                return;

            Graphics g = pe.Graphics;
            g.Clear(Color.White);
            Size clientSize = this.ClientSize;
            //Console.WriteLine(clientRect.Height);

            // TODO: Start depending on where scrollbound is? I dunno.
            int y = -Scroll;
            int lineHeight = GetLineHeight(g);
            for(int i=StartAddress; i < decodedInstrs.Count; ++i)
            {
                if (y < -lineHeight)
                {
                    y += lineHeight;
                    continue;
                }

                DecodedInstr inst = decodedInstrs[i];

                bool isBP = nes.Cpu.Breakpoints.GetBreakpoint((ushort)inst.Address) != null;
                bool isPC = nes.Cpu.PC == inst.Address;
                bool isSelected = (i == SelectedIndex);

                Color bg = Color.White;
                Color fg = Color.Black;

                if (isPC)
                {
                    bg = Color.FromArgb(0xFF, 0xEE, 0x00);
                }
                else if (isBP)
                {
                    bg = Color.DarkRed;
                    fg = Color.White;
                }

                if(bg != Color.White)
                    g.FillRectangle(new SolidBrush(bg), new Rectangle(0, y, clientSize.Width, lineHeight));
                
                g.DrawString(inst.String, fnt, new SolidBrush(fg), 20, y);

                Pen p = new Pen(Color.Black);
                p.DashStyle = DashStyle.Dot;
                if (isSelected)
                    g.DrawRectangle(p, new Rectangle(0, y, clientSize.Width-1, lineHeight));

                if (isBP || isPC)
                {
                    int bpX = (int)Math.Round((20 - bpImage.Width) / 2.0);
                    int bpY = (int)Math.Round(y + (lineHeight - bpImage.Height) / 2.0);

                    RectangleF destRect = new RectangleF(bpX, bpY, bpImage.Width, bpImage.Height);

                    if(isBP)
                        g.DrawImage(bpImage, destRect);
                    if (isPC)
                        g.DrawImage(arrowImage, destRect);
                }

                y += lineHeight;

                if (y > clientSize.Height)
                    break;
            }
        }


        private void SelectPosition(int x, int y)
        {
            if (y < 0)
                y = 0;
            if (y >= ClientSize.Height)
                y = ClientSize.Height - 1;
            Graphics g = CreateGraphics();
            int lineHeight = GetLineHeight(g);

            int index = (y + Scroll)/ lineHeight;
            if (index != SelectedIndex && index >= 0 && index < decodedInstrs.Count)
            {
                SelectedIndex = index;
                Invalidate();
            }
            MakeSelectionVisible();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseDown = true;
            SelectPosition(e.X, e.Y);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if(mouseDown)
                SelectPosition(e.X, e.Y);
            base.OnMouseMove(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Up ||
                (keyData & Keys.KeyCode) == Keys.Down)
                return true;

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch(e.KeyCode & Keys.KeyCode)
            {
                case Keys.F9:
                    if (SelectedIndex >= 0 & SelectedIndex < decodedInstrs.Count)
                    {
                        ushort addr = (ushort)decodedInstrs[SelectedIndex].Address;
                        if (nes.Cpu.Breakpoints.GetBreakpoint(addr) != null)
                            nes.Cpu.Breakpoints.ClearBreakpoint(addr);
                        else
                            nes.Cpu.Breakpoints.SetBreakpoint(addr);
                        Invalidate();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.PageUp:
                    {
                        int lineHeight = GetLineHeight(CreateGraphics());
                        int page = (int)Math.Ceiling((double)ClientSize.Height / lineHeight);
                        SelectedIndex = Math.Max(0, SelectedIndex - page);
                        MakeSelectionVisible();
                        Invalidate();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.PageDown:
                    {
                        int lineHeight = GetLineHeight(CreateGraphics());
                        int page = (int)Math.Ceiling((double)ClientSize.Height / lineHeight);
                        SelectedIndex = Math.Min(decodedInstrs.Count-1, SelectedIndex + page);
                        MakeSelectionVisible();
                        Invalidate();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    break;

                case Keys.Down:
                    if (SelectedIndex < decodedInstrs.Count)
                    {
                        ++SelectedIndex;
                        MakeSelectionVisible();
                        Invalidate();
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;

                case Keys.Up:
                    if (SelectedIndex > 0)
                    {
                        --SelectedIndex;
                        MakeSelectionVisible();
                        Invalidate();
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
            }

            base.OnKeyDown(e);
        }
    }
}
