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
            Mapper.Reset();
            Cpu.Reset();
            Ppu.Reset();
            sw.Start();
        }

        public int Run(int ppuCyclesToRun, out bool render)
        {
            render = false;

            if (Cpu.Paused)
                return ppuCyclesToRun;

            while (ppuCyclesToRun > 0)
            {
                --ppuCyclesToRun;

                // CPU clock is 1/3 the PPU clock
                /*++cpuCycles;
                if (cpuCycles == 3)
                {
                    cpuCycles = 0;*/

                ++cpuDivider;
                if(cpuDivider == 3)
                {
                    cpuDivider = 0;
                    ++TotalCpuCycles;

                    if (Cpu.WaitCycles > 0)
                        Cpu.WaitCycles--;

                    if (Cpu.WaitCycles == 0)
                    {
                        Cpu.Tick();
                        if (Cpu.Paused)
                            break;
                    }
                }
                //}

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
                    render = true;
                    break;
                }
            }

            return ppuCyclesToRun;
        }
    }
}
