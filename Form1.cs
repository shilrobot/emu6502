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

        private class DisasmMemory : IMemory
        {
            private byte[] data = new byte[65536];

            // TODO: Handle address wraparound?
            public byte Read(int addr) { return data[addr]; }
            public void Write(int addr, byte b) { data[addr] = b; }
        }

        private void disassemble_Click(object sender, EventArgs e)
        {
            int addr = 0;

            try
            {
                addr = Convert.ToInt32(startAddr.Text.Trim(), 16);
            }
            catch
            {
                outputBox.Text = "Malformed address";
                return;
            }

            if (addr < 0 || addr > 0xFFFF)
            {
                outputBox.Text = "Address out of range";
                return;
            }

            DisasmMemory mem = new DisasmMemory();
            int currAddr = addr;

            // lol slow
            string[] octets = hexBox.Text.Split(' ', '\t', '\n', '\r');
            int count = 0;
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

                if (currAddr > 0xFFFF)
                {
                    outputBox.Text = "Exceeded memory limits";
                    return;
                }

                mem.Write(currAddr, value);
                ++currAddr;
                ++count;
            }

            outputBox.Text = Disassembler.Disassemble(mem, addr, count);
        }

    }
}
