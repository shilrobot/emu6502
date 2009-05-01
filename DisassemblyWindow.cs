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
        public List<DecodedInstr> decodedInstrs = new List<DecodedInstr>();

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
            fnt = new Font("Courier New", 10.0f);
        }

        private void Disassemble()
        {
            if (nes == null)
                return;

            decodedInstrs.Clear();
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
                    
                    addr += 1;
                }
            }

            Console.WriteLine("Decoded {0} instrs for disassembler", decodedInstrs.Count);
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
            float y = 0.0f;
            for(int i=StartAddress; i < decodedInstrs.Count; ++i)
            {
                DecodedInstr inst = decodedInstrs[i];

                SizeF size = g.MeasureString(inst.String, fnt);
                if (inst.Address == nes.Cpu.PC)
                {
                    g.FillRectangle(new SolidBrush(Color.Yellow), new RectangleF(0, y, clientSize.Width, size.Height));
                }
                g.DrawString(inst.String, fnt, new SolidBrush(Color.Black), 0, y);
                y += size.Height;

                if (y > clientSize.Height)
                    break;
            }
        }
    }
}
