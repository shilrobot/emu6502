using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Emu6502
{
    public class Nes
    {
        //public const long PpuTicksPerSecond = 5369318;
        public static Nes ActiveNes;

        public Controller Controller1 = new Controller();
        public Controller Controller2 = new Controller();

        public Rom Rom { get; private set; }
        public Chip6502 Cpu { get; private set; }
        public Ppu Ppu { get; private set; }
        public NesMemory Mem { get; private set; }
        public Mapper Mapper { get; private set; }
        //private int cpuCycles;
        //private int ppuCycles;
        //public int frameDivider = 0;

        private int cpuDivider = 0;
        public long TotalCpuCycles = 0;
        public long TotalPpuCycles = 0;

        private Stopwatch sw = new Stopwatch();
        public float FPS;
        private int frameCount = 0;

        public Dictionary<string, int> EventCounters = new Dictionary<string, int>();

        public void RecordEvent(string name)
        {
            if (EventCounters.ContainsKey(name))
                EventCounters[name] += 1;
            else
                EventCounters[name] = 1;
        }

        public Nes(string romfile)
        {
            ActiveNes=this;
            Rom = new Rom(romfile);
            Mem = new NesMemory(this);
            Cpu = new Chip6502(Mem);
            Ppu = new Ppu(this);
            switch (Rom.MapperNumber)
            {
                case 0:
                    Mapper = new NROM(this);
                    break;
                case 1:
                    Mapper = new MMC1(this);
                    break;
                case 2:
                    Mapper = new UxROM(this);
                    break;
                default:
                    // TODO: Cleaner...
                    throw new Exception("Unknown mapper");
            }

            Reset();
        }

        public void Reset()
        {
            sw.Reset();
            sw.Start();
            //cpuCycles = 0;// ppuCycles = 0;
            cpuDivider = 0;
            TotalPpuCycles = 0;
            TotalCpuCycles = 0;
            frameCount = 0;
            Mem.Reset();
            Ppu.Reset();
            Mapper.Reset();
            Cpu.RAM = Mem.RAM;
            Cpu.Reset();
            sw.Start();
        }

        public int RunOneFrame()
        {
            /*if (Cpu.Paused)
                return ppuCyclesToRun;*/

            while (true)
            {
                if (Cpu.WaitCycles <= Ppu.WaitCycles)
                {
                    Cpu.Run(Ppu.WaitCycles);
                }
                else
                    Cpu.WaitCycles -= Ppu.WaitCycles;

                Ppu.FrameCycle += Ppu.WaitCycles;
                Ppu.WaitCycles = 0;
                Ppu.Tick();
                if (Ppu.VsyncSignalToMainLoop)
                {
                    Ppu.VsyncSignalToMainLoop = false;
                    ++frameCount;
                    if (frameCount == 10)
                    {
                        float secs = sw.ElapsedTicks / (float)Stopwatch.Frequency;
                        FPS = frameCount / secs;
                        sw.Reset();
                        sw.Start();
                        frameCount = 0;
                    }
                    //render = true;
                    return 0;
                }

#if false
                //Console.WriteLine("ToRun: {0} CPU: {1} PPU: {2}", ppuCyclesToRun, Cpu.WaitCycles, Ppu.WaitCycles);
                //int min = ppuCyclesToRun < Ppu.WaitCycles ? ppuCyclesToRun : Ppu.WaitCycles;
                int min = Ppu.WaitCycles < Cpu.WaitCycles ? Ppu.WaitCycles : Cpu.WaitCycles;
                //Console.WriteLine("Min={0}", min);

                //TotalCpuCycles += min;
                //TotalPpuCycles += min;
                
                Cpu.WaitCycles -= min;
                Ppu.WaitCycles -= min;
                //ppuCyclesToRun -= min;


                if (Cpu.WaitCycles == 0)
                    Cpu.Tick();
                /*if (Ppu.WaitCycles < 0)
                    throw new InvalidOperationException("FUCK");*/
                Ppu.FrameCycle += min;
                if (Ppu.WaitCycles == 0)
                {
                    Ppu.Tick();
                    if (Ppu.VsyncSignalToMainLoop)
                    {
                        Ppu.VsyncSignalToMainLoop = false;
                        ++frameCount;
                        if (frameCount == 10)
                        {
                            float secs = sw.ElapsedTicks / (float)Stopwatch.Frequency;
                            FPS = frameCount / secs;
                            sw.Reset();
                            sw.Start();
                            frameCount = 0;
                        }
                        //render = true;
                        return 0;
                    }
                }
#endif

            }


            //return ppuCyclesToRun;
        }
    }
}
