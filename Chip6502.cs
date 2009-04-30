using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public partial class Chip6502
    {
        public const byte SignBit = 0x80;

        public byte A; // Accumulator
        public byte X; // X registr
        public byte Y; // Y register
        public ushort PC; // Current instruction address

        // Status flags
        public bool C, Z, I, V, N;

        public byte SP; // Stack pointer
        public IMemory Mem;

        public Chip6502(IMemory mem)
        {
            Mem = mem;
            Reset();
        }

        public void Reset()
        {
            A = X = Y = 0;
            // Load PC from reset vector
            PC = ReadWord(0xFFFC);
            // Z flag apparently is set after reset
            C = I = V = N = false;
            Z = true;
            SP = 0xFF;
        }

        // In practice this is connected to the vertical retrace from the PPU
        public void NMI()
        {
            PushWord(PC);
            PushStatus(false);
            I = true;
            PC = ReadWord(0xFFFA);
        }

        public bool IRQ()
        {
            if (I)
                return false;
            PushWord(PC);
            PushStatus(false);
            I = true;
            PC = ReadWord(0xFFFE);
            return true;
        }

        private byte Read(int addr)
        {
            return Mem.Read(addr);
        }

        private ushort ReadWord(int addr)
        {
            return (ushort)(Read(addr) | Read(addr + 1) << 8);
        }

        private void Write(int addr, byte val)
        {
            Mem.Write(addr, val);
        }

        private void Push(byte val)
        {
            Write(0x100 + (SP--), val);
        }

        private void PushWord(ushort val)
        {
            Push((byte)((val & 0xFF00) >> 8));
            Push((byte)(val & 0xFF));
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
        }

        private void PushStatus(bool brk)
        {
            // Bit 5 is always set, bit 3 (D) is always zero
            byte status = (byte)((N ? 0x80 : 0x00) |
                                 (V ? 0x40 : 0x00) |
                                 0x20 |
                                 (brk ? 0x10 : 0x00) |
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
            I = (status & 0x04) != 0;
            Z = (status & 0x02) != 0;
            C = (status & 0x01) != 0;
        }


        // TODO: Cycle-accurate counters, etc.

    }
}
