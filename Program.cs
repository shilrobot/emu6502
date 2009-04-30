using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

            /*Rom r = new Rom("Roms/Mega Man 2 (U).nes");
            Rom r2 = new Rom("Roms/Castlevania (E).nes");*/
            Rom r3 = new Rom("Roms/Super Mario Bros. (E).nes");
            NesMemory mem = new NesMemory(r3);
            Chip6502 cpu = new Chip6502(mem);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            int max = 10000000;
            for(int n=0; n<max; ++n)
            {
                cpu.Tick();
            }
            sw.Stop();
            double secs = (double)sw.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency;
            Console.WriteLine("Speed: {0:0.00} MHz", (max / secs) / 1.0e6);
            return;
        }
    }
}
