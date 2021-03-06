﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Emu6502
{
    public partial class Chip6502
    {
        // Locations of interrupt vectors in memory
        public const ushort NMIAddr     = 0xFFFA;
        public const ushort ResetAddr   = 0xFFFC;
        public const ushort IRQAddr     = 0xFFFE;

        public bool Paused = false;
        public bool SingleStep = false;

        public byte A; // Accumulator
        public byte X; // X register
        public byte Y; // Y register
        public ushort PC; // Current instruction address

        public BreakpointManager Breakpoints { get; private set; }

        // Status flags
        // Note: D (BCD arithmetic flag) exists but doesn't actually do anything
        // Likewise, B is not a real flag, but rather only shows up as 1 in the saved
        // status register on the stack after BRK causes an interrupt, or after PHP.
        public bool C, Z, I, V, N, D;

        /*public bool N { get { return (sbyte)DeferN < 0; } set { DeferN = value ? (byte)0xFF : (byte)0; } }
        public bool Z { get { return DeferZ == 0; } set { DeferZ = value ? (byte)0 : (byte)0xFF; } }

        public byte DeferZ = 0;
        public byte DeferN = 0;*/

        public byte SP; // Stack pointer
        public IMemory Mem;

        //private int ignoreOpcodes = 0;
        public int WaitCycles = 0;

        private List<ushort> PCHistory = new List<ushort>();

        public byte[] RAM;

        


        //private StreamWriter fs = new StreamWriter("my_cpu.txt");

        public Chip6502(IMemory mem)
        {
            Mem = mem;
            Breakpoints = new BreakpointManager();
            //Reset();
        }

        public string DumpRegs()
        {
            return String.Format("PC={0:X4} SP={1:X2} A={2:X2} X={3:X2} Y={4:x2} C={5} Z={6} I={7} V={8} N={9}",
                PC, SP, A, X, Y, C ? 1 : 0, Z ? 1 : 0, I ? 1 : 0, V ? 1 : 0, N ? 1 : 0);
        }

        public void Reset()
        {
            A = X = Y = 0;
            // Load PC from reset vector
            // Z flag apparently is set after reset
            C = I = V = N = D = false;
            Z = true;
            SP = 0xFF;
            //ignoreOpcodes = 0;
            SetPC(ReadWord(ResetAddr));
            //Console.WriteLine("6502 Reset -> Jump to ${0:X4}", PC);
            Nes.ActiveNes.RecordEvent("Cpu.Int.Reset");
        }

        // In practice this is connected to the vertical retrace from the PPU
        public void NMI()
        {
            Console.WriteLine("NMI return value: ${0:X4} @ {1} cy", PC, Nes.ActiveNes.TotalCpuCycles);
            PushWord(PC);
            PushStatus(false);
            I = true;
            SetPC(ReadWord(NMIAddr));
            //Console.WriteLine("6502 NMI -> Jump to ${0:X4}", PC);
            WaitCycles += 7*3; // Interrupt latency
            Nes.ActiveNes.RecordEvent("Cpu.Int.NMI");
        }

        public void IRQ()
        {
            if (!I)
            {
                PushWord(PC);
                PushStatus(false);
                I = true;
                SetPC(ReadWord(IRQAddr));
                Nes.ActiveNes.RecordEvent("Cpu.Int.IRQ");
                //Console.WriteLine("6502 IRQ -> Jump to ${0:X4}", PC);
            }
            else
                Nes.ActiveNes.RecordEvent("Cpu.Int.IRQ.Ignored");// Console.WriteLine("6502 IRQ (Ignored)");
        }

        public byte Read(int addr)
        {
            return Mem.Read(addr);
        }

        public ushort ReadWord(int addr)
        {
            return (ushort)(Read(addr) | Read(addr + 1) << 8);
        }

        private ushort ReadWordZP(int addr)
        {
            return (ushort)(Read(addr&0xFF) | Read((addr + 1)&0xFF) << 8);
        }

        private void Write(int addr, byte val)
        {
            Mem.Write(addr, val);
        }

        private void Push(byte val)
        {
            Write(0x100 + (SP--), val);
            /*if (SP == 0xFF)
                 Console.WriteLine("Stack Wraparound on Push");*/
        }

        private void PushWord(ushort val)
        {
            Push((byte)(val >> 8));
            Push((byte)val);
        }

        private ushort PullWord()
        {
            byte lo = Pull();
            byte hi = Pull();
            return (ushort)(lo | (hi << 8));
        }

        private byte Pull()
        {
            return Read(0x100 + (++SP));
            /*if (SP == 0x00)
                Console.WriteLine("Stack Wraparound on Pull");*/
        }

        private void PushStatus(bool brk)
        {
            /*Console.WriteLine("Pushed Status: N={0} V={1} I={2} Z={3} C={4} B={5}",
                N ? 1 : 0,
                V ? 1 : 0,
                I ? 1 : 0,
                Z ? 1 : 0,
                C ? 1 : 0,
                brk ? 1 : 0);*/

            // Bit 5 is always set, bit 3 (D) is always zero
            byte status = (byte)((N ? 0x80 : 0x00) |
                                 (V ? 0x40 : 0x00) |
                                 0x20 |
                                 (brk ? 0x10 : 0x00) |
                                 (D ? 0x08 : 0x00) |
                                 (I ? 0x04 : 0x00) |
                                 (Z ? 0x02 : 0x00) |
                                 (C ? 0x01 : 0x00));
            Push(status);
        }

        private void PullStatus()
        {
            byte status = Pull();
            N = (status & 0x80) != 0;
            V = (status & 0x40) != 0;
            D = (status & 0x08) != 0;
            I = (status & 0x04) != 0;
            Z = (status & 0x02) != 0;
            C = (status & 0x01) != 0;
            /*Console.WriteLine("Pulled Status: N={0} V={1} I={2} Z={3} C={4}",
                N ? 1 : 0,
                V ? 1 : 0,
                I ? 1 : 0,
                Z ? 1 : 0,
                C ? 1 : 0);*/
        }

        private void DumpPCHistory()
        {
            Console.WriteLine("PC history, oldest first:");
            foreach (ushort pc in PCHistory)
                Console.WriteLine("PC=${0:X4}", pc);
        }

        public void SetPC(ushort newPC)
        {
            /*if ((PC & 0xFF00) != (newPC & 0xFF00))
            {
                PCHistory.Add(PC);
                PCHistory.Add(newPC);
                if (PCHistory.Count > 64)
                    PCHistory.RemoveAt(0);
            }*/
            PC = newPC;
            if (Breakpoints.GetBreakpoint(PC) != null || SingleStep)
            {
                Paused = true;
                SingleStep = false;
            }
        }
    }
}
