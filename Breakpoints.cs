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
    public partial class Breakpoints : Form
    {
        private Nes nes;

        public Breakpoints(Nes nes)
        {
            InitializeComponent();

            this.nes = nes;
            PopulateList();
        }

        private void PopulateList()
        {
            breakpointList.Items.Clear();
            List<ushort> addrs = nes.Cpu.Breakpoints.GetBreakpointAddresses();
            addrs.Sort();
            foreach (ushort addr in addrs)
                breakpointList.Items.Add(String.Format("${0:X4}", addr));
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            ushort addr=0;
            try
            {
                addr = Convert.ToUInt16(breakpointEdit.Text, 16);
            }
            catch
            {
                MessageBox.Show("Malformatted address");
                return;
            }

            nes.Cpu.Breakpoints.SetBreakpoint(addr);
            PopulateList();
        }

    }
}
