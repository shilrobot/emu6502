using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace Emu6502
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //Application.Run(new Debugger());

            BenchmarkDlg bdlg = new BenchmarkDlg();
            bdlg.ShowDialog();
            return;

            /*Rom r = new Rom("Roms/Mega Man 2 (U).nes");
            Rom r2 = new Rom("Roms/Castlevania (E).nes");*/

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Directory.GetCurrentDirectory();// @"p:\csharp\emu6502\roms";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            Nes nes = new Nes(dlg.FileName);
            nes.Cpu.Paused = true;

#if false
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int max = 500000;
            for(int n=0; n<max; ++n)
            {
                /*w.WriteLine(String.Format("Time = {0:0.000} sec", n / (double)Nes.PpuTicksPerSecond));
                w.WriteLine(nes.Cpu.DumpRegs());
                w.Write(Disassembler.Disassemble(nes.Mem, nes.Cpu.PC, 8));
                w.Write("-");*/
                nes.Tick();
            }
            sw.Stop();
            double secs = (double)sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency;
            Console.WriteLine("PPU Speed: {0:0.00} MHz", (max / secs) / 1.0e6);
#endif

            Application.Run(new Debugger(nes));

            return;
        }
    }
}
