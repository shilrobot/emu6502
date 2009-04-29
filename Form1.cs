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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void disassemble_Click(object sender, EventArgs e)
        {
            List<byte> data = new List<byte>();
            // lol slow
            string[] octets = hexBox.Text.Split(' ', '\t', '\n', '\r');
            foreach (string octet in octets)
            {
                if(octet.Length == 0)
                    continue;
                if (octet.Length > 2)
                {
                    outputBox.Text = "Malformed input";
                    return;
                }

                byte value = 0;
                try
                {
                    value = Convert.ToByte(octet, 16);
                }
                catch
                {
                    outputBox.Text = "Malformed input";
                    return;
                }
                data.Add(value);
            }

            /*
            Disassembler disasm = new Disassembler(0, data.ToArray());
            try
            {
                disasm.Disassemble();
            }
            catch (DisassemblyException ex)
            {
                outputBox.Text = ex.Message;
                return;
            }
             * */

            outputBox.Text = Disassembler.Disassemble(data.ToArray(), 0);
        }

    }
}
