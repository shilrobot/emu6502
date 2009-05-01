using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            cpuCycles = ppuCycles = 0;
            Mem.Reset();
            Cpu.Reset();
            Ppu.Reset();
        }

        public bool Tick()
        {
            if (Cpu.Paused)
                return false;

            // TODO: Cycle-accurate CPU simulator
            ++cpuCycles;
            if (cpuCycles == 3)
            {
                cpuCycles = 0;
                Cpu.Tick();
            }

            if (Cpu.Paused)
                return false;

            Ppu.Tick();

            if (Ppu.VsyncSignalToMainLoop)
            {
                Ppu.VsyncSignalToMainLoop = false;
                /*++frameDivider;
                if (frameDivider == 5)
                {
                    frameDivider = 0;
                    return false;
                }*/
                return false;
            }

            return true;
        }

        public void Run()
        {
            while (Tick()) ;
        }
    }
}
