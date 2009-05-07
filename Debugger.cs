using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using System.IO;

namespace Emu6502
{
    public partial class Debugger : Form
    {
        private Nes nes;
        private PpuOutput outputWindow;
        private Stopwatch sw = new Stopwatch();

        public Debugger()
        {
            InitializeComponent();


            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Directory.GetCurrentDirectory();// @"p:\csharp\emu6502\roms";
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                Application.Exit();
                return;
            }


            nes = new Nes(dlg.FileName);

            UpdateScreen();
            UpdateTitle();
            Application.Idle += new EventHandler(Application_Idle);
            outputWindow = new PpuOutput(nes);
            outputWindow.Show(this);
            //outputWindow.Hide();
            disassembly2.Nes = nes;
            disassembly2.Update();
            disassembly2.SelectAddress(nes.Cpu.PC);
            sw.Start();
        }

        private void UpdateTitle()
        {
            if (nes.Cpu.Paused)
                this.Text = "Debugger [Paused]";
            else
                this.Text = "Debugger [Running]";
        }

        
        private static bool AppStillIdle() {
            pkMessage msg;
            return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct pkMessage {
            public IntPtr hWnd;
            public Message msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(out pkMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

        int counter = 0;
        
        void Application_Idle(object sender, EventArgs e)
        {
            while(AppStillIdle())
            {
                if(nes.Cpu.Paused)
                    return;

                const float deadZone = 0.3f;
                GamePadState state = GamePad.GetState(PlayerIndex.One);
                nes.Controller1.A = state.Buttons.A == (Microsoft.Xna.Framework.Input.ButtonState.Pressed);
                nes.Controller1.B = state.Buttons.X == (Microsoft.Xna.Framework.Input.ButtonState.Pressed);
                nes.Controller1.Start = state.Buttons.Start == (Microsoft.Xna.Framework.Input.ButtonState.Pressed);
                nes.Controller1.Select = state.Buttons.Back == (Microsoft.Xna.Framework.Input.ButtonState.Pressed);
                nes.Controller1.Left = state.ThumbSticks.Left.X < -deadZone;
                nes.Controller1.Right = state.ThumbSticks.Left.X > deadZone;
                nes.Controller1.Up = state.ThumbSticks.Left.Y > deadZone;
                nes.Controller1.Down = state.ThumbSticks.Left.Y < -deadZone;

                double deltaT = (double)sw.ElapsedTicks / (double)Stopwatch.Frequency;
                deltaT *= 5;
                sw.Reset();
                sw.Start();
                if(deltaT > 0.1f)
                    deltaT = 0.1f;
                double clockRate = 341 * 262 * 60;

                int cycles = (int)Math.Round(clockRate * deltaT);
                //Console.WriteLine("{0:0} ms = {1} cycles", deltaT * 1000.0, cycles);

                //bool render=false;
                /*while (!nes.Cpu.Paused && cycles > 0)
                {
                    bool tmpRender = false;
                    cycles = nes.Run(cycles, out tmpRender);

                    if(tmpRender)
                        render = true;
                }*/
                nes.RunOneFrame();

                if (nes.Cpu.Paused)
                {
                    UpdateTitle();
                    UpdateScreen();
                    disassembly2.SelectAddress(nes.Cpu.PC);
                    return;
                }

                //if(render)
                    outputWindow.Invalidate();

                counter++;
                if (counter > 10)
                {
                    outputWindow.Text = String.Format("NES Emulator - {0:0.0} FPS", nes.FPS);
                    counter = 0;
                }
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
            pcLabel.Text = String.Format("PC ${0:X4} Cy={1}", nes.Cpu.PC, nes.TotalCpuCycles);
            spLabel.Text = String.Format("SP ${0:X2}", nes.Cpu.SP);
            aLabel.Text = DecodeReg("A", nes.Cpu.A);
            xLabel.Text = DecodeReg("X", nes.Cpu.X);
            yLabel.Text = DecodeReg("Y", nes.Cpu.Y);


            cFlag.Checked = nes.Cpu.C;
            zFlag.Checked = nes.Cpu.Z;
            iFlag.Checked = nes.Cpu.I;
            vFlag.Checked = nes.Cpu.V;
            nFlag.Checked = nes.Cpu.N;

            ivectorLabel.Text = String.Format("Reset: ${0:X4} = ${1:X4}\r\n" +
                "  NMI: ${2:X4} = ${3:X4}\r\n" +
                "  IRQ: ${4:X4} = ${5:X4}\r\n",
                Chip6502.ResetAddr, nes.Cpu.ReadWord(Chip6502.ResetAddr),
                Chip6502.NMIAddr, nes.Cpu.ReadWord(Chip6502.NMIAddr),
                Chip6502.IRQAddr, nes.Cpu.ReadWord(Chip6502.IRQAddr));

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
            disassembly2.SelectAddress(nes.Cpu.PC);
        }

        private void interruptNMI_Click(object sender, EventArgs e)
        {
            nes.Cpu.NMI();
            nes.Cpu.Paused = true;
            UpdateScreen();
            UpdateTitle();
            disassembly2.SelectAddress(nes.Cpu.PC);
        }

        private void interruptIRQ_Click(object sender, EventArgs e)
        {
            nes.Cpu.IRQ();
            nes.Cpu.Paused = true;
            UpdateScreen();
            UpdateTitle();
            disassembly2.SelectAddress(nes.Cpu.PC);
        }

        private void debugGo_Click(object sender, EventArgs e)
        {
            if (nes.Cpu.Paused)
                nes.Cpu.Paused = false;
            else
            {
                nes.Cpu.Paused = true;
                UpdateScreen();
                disassembly2.SelectAddress(nes.Cpu.PC);
            }

            UpdateTitle();
        }

        private void debugBreakpoints_Click(object sender, EventArgs e)
        {
            Breakpoints bp = new Breakpoints(nes);
            bp.ShowDialog();

            // TODO: Blah, cleaner stuff for this
            UpdateScreen();
        }

        private void Debugger_FormClosed(object sender, FormClosedEventArgs e)
        {
            //nes.Cpu.SaveEncounteredInstructions();
        }

        private void redisasm_Click(object sender, EventArgs e)
        {
            disassembly2.Disassemble();
            disassembly2.SelectAddress(nes.Cpu.PC);
        }

        private void gotoBtn_Click(object sender, EventArgs e)
        {
            AddressEntryDlg dlg = new AddressEntryDlg();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                disassembly2.SelectAddress((ushort)dlg.Address);
            }
        }

        private void showPCBtn_Click(object sender, EventArgs e)
        {
            disassembly2.SelectAddress(nes.Cpu.PC);
        }

        private void ppuDebug_Click(object sender, EventArgs e)
        {
            PpuDebug dbg = new PpuDebug(this.nes.Ppu);
            dbg.Show();
        }

        private void findBtn_Click(object sender, EventArgs e)
        {
            FindDlg dlg = new FindDlg();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                disassembly2.Find(dlg.Result);
            }
        }

        private void eventsBtn_Click(object sender, EventArgs e)
        {
            List<string> eventNames = new List<string>(nes.EventCounters.Keys);
            eventNames.Sort();
            string result = "";
            foreach (string name in eventNames)
                result += String.Format("{0}={1}\r\n", name, nes.EventCounters[name]);
            MessageBox.Show(result);
        }

        private void clearEvents_Click(object sender, EventArgs e)
        {
            nes.EventCounters.Clear();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private void Debugger_Load(object sender, EventArgs e)
        {
            //outputWindow.Focus();
            SetForegroundWindow(outputWindow.Handle);
        }

    }
}
