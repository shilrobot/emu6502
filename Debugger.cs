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
    public partial class Debugger : Form
    {
        private Nes nes;
        private PpuOutput outputWindow;

        public Debugger(Nes nes)
        {
            InitializeComponent();
            this.nes = nes;

            UpdateScreen();
            UpdateTitle();
            Application.Idle += new EventHandler(Application_Idle);
            outputWindow = new PpuOutput(nes.Ppu);
            outputWindow.Show(this);
            outputWindow.Hide();
            disassembly2.Nes = nes;
            disassembly2.Update();
        }

        private void UpdateTitle()
        {
            if (nes.Cpu.Paused)
                this.Text = "Debugger [Paused]";
            else
                this.Text = "Debugger [Running]";
        }

        void Application_Idle(object sender, EventArgs e)
        {
            if (!nes.Cpu.Paused)
            {
                nes.Run();

                if (nes.Cpu.Paused)
                {
                    UpdateTitle();
                    UpdateScreen();
                }

                outputWindow.Invalidate();
            }
        }

        private string DecodeReg(string name, byte value)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name);
            sb.AppendFormat("  ${0:X2} ", value);
            for (int i = 7; i >= 4; --i)
                sb.Append((value & (1 << i)) != 0 ? "1" : "0");
            sb.Append(" ");
            for (int i = 3; i >= 0; --i)
                sb.Append((value & (1 << i)) != 0 ? "1" : "0");
            if (value > 127)
                sb.AppendFormat(" {0}/{1}", (int)value, (int)(sbyte)value);
            else
                sb.AppendFormat(" {0}", (int)value);
            return sb.ToString();
        }

        public void UpdateScreen()
        {
            pcLabel.Text = String.Format("PC ${0:X4}", nes.Cpu.PC);
            spLabel.Text = String.Format("SP ${0:X2}", nes.Cpu.SP);
            aLabel.Text = DecodeReg("A", nes.Cpu.A);
            xLabel.Text = DecodeReg("X", nes.Cpu.X);
            yLabel.Text = DecodeReg("Y", nes.Cpu.Y);

            cFlag.Checked = nes.Cpu.C;
            zFlag.Checked = nes.Cpu.Z;
            iFlag.Checked = nes.Cpu.I;
            vFlag.Checked = nes.Cpu.V;
            nFlag.Checked = nes.Cpu.N;

            //string dis = Disassembler.Disassemble(nes.Mem, nes.Cpu.PC - 16, 128);
            //disassembly.Text = dis;
            disassembly2.Invalidate();
        }

        private void fileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void debugStepOver_Click(object sender, EventArgs e)
        {
            nes.Cpu.SingleStep = true;
            nes.Cpu.Paused = false;
            UpdateTitle();
        }

        private void nFlag_CheckedChanged(object sender, EventArgs e)
        {
            nes.Cpu.N = nFlag.Checked;
        }

        private void vFlag_CheckedChanged(object sender, EventArgs e)
        {
            nes.Cpu.V = vFlag.Checked;
        }

        private void iFlag_CheckedChanged(object sender, EventArgs e)
        {
            nes.Cpu.I = iFlag.Checked;
        }

        private void zFlag_CheckedChanged(object sender, EventArgs e)
        {
            nes.Cpu.Z = zFlag.Checked;
        }

        private void cFlag_CheckedChanged(object sender, EventArgs e)
        {
            nes.Cpu.C = cFlag.Checked;
        }

        private void interruptReset_Click(object sender, EventArgs e)
        {
            nes.Reset();
            nes.Cpu.Paused = true;
            UpdateScreen();
            UpdateTitle();
        }

        private void interruptNMI_Click(object sender, EventArgs e)
        {
            nes.Cpu.NMI();
            nes.Cpu.Paused = true;
            UpdateScreen();
            UpdateTitle();
        }

        private void interruptIRQ_Click(object sender, EventArgs e)
        {
            nes.Cpu.IRQ();
            nes.Cpu.Paused = true;
            UpdateScreen();
            UpdateTitle();
        }

        private void debugGo_Click(object sender, EventArgs e)
        {
            if (nes.Cpu.Paused)
                nes.Cpu.Paused = false;
            else
            {
                nes.Cpu.Paused = true;
                UpdateScreen();
            }

            UpdateTitle();
        }

        private void debugBreakpoints_Click(object sender, EventArgs e)
        {
            Breakpoints bp = new Breakpoints(nes);
            bp.ShowDialog();
        }
    }
}
