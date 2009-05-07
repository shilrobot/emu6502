using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Emu6502
{
    public partial class BenchmarkDlg : Form
    {

        public BenchmarkDlg()
        {
            InitializeComponent();
        }

        private double Benchmark(string name, string romPath)
        {
            Nes nes = new Nes(romPath);
            bool render;
            int cycles = 341*262*60*10;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for(int i=0; i<10*60; ++i)
                nes.RunOneFrame();//cycles, out render);
            sw.Stop();
            double time = sw.ElapsedTicks / (double)Stopwatch.Frequency;
            textBox.AppendText(String.Format("{0}: {1:0.000} s\r\n", name, time));
            Refresh();
            return time;
        }

        private double StdDev(ICollection<double> vals)
        {
            double sum = vals.Sum();
            double avg = sum / vals.Count;
            double sumSquaredError = 0;
            foreach (double x in vals)
                sumSquaredError += (x - avg) * (x - avg);
            return Math.Sqrt(sumSquaredError / vals.Count);
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            textBox.Clear();

            List<double> averages = new List<double>();

            for (int i = 0; i < 5; ++i)
            {
                double sum = 0;
                sum += Benchmark("Super Mario Brothers", @"p:\csharp\emu6502\roms\Super Mario Bros. (JU) (PRG0) [!].nes");
                sum += Benchmark("Mega Man 2", @"p:\csharp\emu6502\roms\Mega Man 2 (U).nes");
                sum += Benchmark("Castlevania", @"p:\csharp\emu6502\roms\Castlevania (U) (PRG0) [!].nes");
                sum += Benchmark("Blargg CPU Test", @"p:\csharp\emu6502\roms\blargg_nes_cpu_test5\blargg_nes_cpu_test5\official.nes");
                double avg = sum / 4.0;
                textBox.AppendText(String.Format("Avg: {0:0.000} s\r\n", avg));
                textBox.AppendText(String.Format("Speed: {0:0.000}X\r\n", 10.0 / avg));
                Refresh();

                averages.Add(avg);
            }
            
            
            textBox.AppendText("--------------------");
            textBox.AppendText(String.Format("Total Avg: {0:0.000} s\r\n", averages.Sum() / averages.Count));
            textBox.AppendText(String.Format("Std Dev: {0:0.000} s\r\n", StdDev(averages)));

                Chip6502.SaveTimings();
        }
    }
}
