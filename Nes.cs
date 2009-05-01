using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Emu6502
{
    public class Nes
    {
        public const long PpuTicksPerSecond = 5369318;

        public Rom Rom;
        public Chip6502 Cpu;
        public Ppu Ppu;
        public NesMemory Mem;
        public int cpuCycles;
        public int ppuCycles;
        public int frameDivider = 0;

        private Stopwatch sw = new Stopwatch();
        public float FPS;
        private int frameCount = 0;

        public Nes(string romfile)
        {
            Rom = new Rom(romfile);
            Mem = new NesMemory(this);
            Cpu = new Chip6502(Mem);
            Ppu = new Ppu(this);

            Reset();
        }

        public void Reset()
        {
            sw.Reset();
            sw.Start();
            cpuCycles = ppuCycles = 0;
            frameCount = 0;
            Mem.Reset();
            Cpu.Reset();
            Ppu.Reset();
            sw.Start();
        }

        public void Run()
        {
            while (!Cpu.Paused)
            {
                // CPU clock is 1/3 the PPU clock
                ++cpuCycles;
                if (cpuCycles == 3)
                {
                    cpuCycles = 0;

                    if (Cpu.WaitCycles > 0)
                        Cpu.WaitCycles--;

                    if (Cpu.WaitCycles == 0)
                    {
                        Cpu.Tick();
                        if (Cpu.Paused)
                            break;
                    }
                }

                ++Ppu.ScanlineCycle;
                if (Ppu.ScanlineCycle >= Ppu.ClocksPerScanline)
                    Ppu.FinishScanline();

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
                    break;
                }
            }
        }
    }
}
