using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public enum AddressMode
    {
        Immediate,
        ZeroPage,
        ZeroPageIndexedX,
        ZeroPageIndexedY,
        Accumulator,
        Absolute,
        AbsoluteIndexedX,
        AbsoluteIndexedY,
        Indirect,
        IndexedIndirectX,
        IndirectIndexedY,
        Implied,
        Relative
    }

    public class Disassembler
    {
        // TODO: Allow for different "display addreses" versus actual internal addresses to 'data'
        public static string Disassemble(IMemory mem, int addr, int count)
        {
            StringBuilder sb = new StringBuilder();
            while (count > 0)
            {
                int consumed;
                if (DisassembleOne(mem, addr, count, sb, out consumed))
                {
                    addr += consumed;
                    count -= consumed;
                }
                else
                {
                    DB(sb, addr, mem.Read(addr));
                    ++addr;
                    --count;
                }
            }
            return sb.ToString();
        }

        public static void DB(StringBuilder sb, int addr, byte b)
        {
            sb.AppendFormat("{0:X4}   {1:X2}         .DB ${1:X2}\r\n", addr, b);
        }

        public static bool DisassembleOne(IMemory mem, int addr, int count, StringBuilder sb, out int bytesConsumed)
        {
            bytesConsumed = 0;

            if (mem == null)
                throw new ArgumentNullException("data");

            if (sb == null)
                throw new ArgumentNullException("sb");

            if (addr < 0 ||
                addr > 0xFFFF)
                throw new ArgumentOutOfRangeException("addr");

            byte opcode = mem.Read(addr);
            if (opNames[opcode] == null)
                return false;
            AddressMode mode = addrModes[opcode];
            int byteCount = byteCounts[(int)mode];
            if (count < byteCount)
                return false;
            byte lo = 0, hi = 0;
            if (byteCount >= 2)
                lo = mem.Read(addr + 1);
            if (byteCount == 3)
                hi = mem.Read(addr + 2);
            
            if(byteCount == 3)
                sb.AppendFormat("{0:X4}   {1:X2} {2:X2} {3:X2}   {4}", addr, opcode, lo, hi, opNames[opcode]);
            else if (byteCount == 2)
                sb.AppendFormat("{0:X4}   {1:X2} {2:X2}      {3}", addr, opcode, lo, opNames[opcode]);
            else if (byteCount == 1)
                sb.AppendFormat("{0:X4}   {1:X2}         {2}", addr, opcode, opNames[opcode]);

            // TODO: Handle don't-care byte for BRK!

            switch (mode)
            {
                case AddressMode.Immediate:
                    sb.AppendFormat(" #${0:X2}", lo);
                    break;
                case AddressMode.ZeroPage:
                    sb.AppendFormat(" ${0:X2}", lo);
                    break;
                case AddressMode.ZeroPageIndexedX:
                    sb.AppendFormat(" ${0:X2},X", lo);
                    break;
                case AddressMode.ZeroPageIndexedY:
                    sb.AppendFormat(" ${0:X2},Y", lo);
                    break;
                case AddressMode.Accumulator:
                    //sb.AppendFormat(" A", lo);
                    break;
                case AddressMode.Absolute:
                    sb.AppendFormat(" ${0:X2}{1:X2}", hi, lo);
                    break;
                case AddressMode.AbsoluteIndexedX:
                    sb.AppendFormat(" ${0:X2}{1:X2},X", hi, lo);
                    break;
                case AddressMode.AbsoluteIndexedY:
                    sb.AppendFormat(" ${0:X2}{1:X2},Y", hi, lo);
                    break;
                case AddressMode.Indirect:
                    sb.AppendFormat(" (${0:X2}{1:X2})", hi, lo);
                    break;
                case AddressMode.IndexedIndirectX:
                    sb.AppendFormat(" (${0:X2},X)", lo);
                    break;
                case AddressMode.IndirectIndexedY:
                    sb.AppendFormat(" (${0:X2}),Y", lo);
                    break;
                case AddressMode.Implied:
                    break;
                case AddressMode.Relative:
                    // TODO: Appropriate sign shit/maybe print out full address
                    {
                        int offset = (lo <= 127) ? lo : lo - 256;
                        int targetAddr = (addr + 2 + offset) & 0xFFFF;
                        sb.AppendFormat(" ${0:X4}", targetAddr);
                    }
                    //sb.AppendFormat(" ${0:X2}", lo);
                    break;
            }

            sb.Append("\r\n");

            bytesConsumed = byteCount;
            return true;
        }

        // Corresponds to numerical values in AddressMode
        private static int[] byteCounts = new int[] {
            2, // Immediate
            2, // ZeroPage
            2, // ZeroPageIndexedX
            2, // ZeroPageIndexedY
            1, // Accumulator
            3, // Absolute
            3, // AbsoluteIndexedX
            3, // AbsoluteIndexedY
            3, // Indirect
            2, // IndexedIndirectX
            2, // IndirectIndexedY
            1, // Implied
            2, // Relative
        };

        private static string[] opNames = new string[] {
            /* BEGIN OPNAMES */
"BRK", "ORA", null, null, null, "ORA", "ASL", null, 
"PHP", "ORA", "ASL", null, null, "ORA", "ASL", null, 
"BPL", "ORA", null, null, null, "ORA", "ASL", null, 
"CLC", "ORA", null, null, null, "ORA", "ASL", null, 
"JSR", "AND", null, null, "BIT", "AND", "ROL", null, 
"PLP", "AND", "ROL", null, "BIT", "AND", "ROL", null, 
"BMI", "AND", null, null, null, "AND", "ROL", null, 
"SEC", "AND", null, null, null, "AND", "ROL", null, 
"RTI", "EOR", null, null, null, "EOR", "LSR", null, 
"PHA", "EOR", "LSR", null, "JMP", "EOR", "LSR", null, 
"BVC", "EOR", null, null, null, "EOR", "LSR", null, 
"CLI", "EOR", null, null, null, "EOR", "LSR", null, 
"RTS", "ADC", null, null, null, "ADC", "ROR", null, 
"PLA", "ADC", "ROR", null, "JMP", "ADC", "ROR", null, 
"BVS", "ADC", null, null, null, "ADC", "ROR", null, 
"SEI", "ADC", null, null, null, "ADC", "ROR", null, 
null, "STA", null, null, "STY", "STA", "STX", null, 
"DEY", null, "TXA", null, "STY", "STA", "STX", null, 
"BCC", "STA", null, null, "STY", "STA", "STX", null, 
"TYA", "STA", "TXS", null, null, "STA", null, null, 
"LDY", "LDA", "LDX", null, "LDY", "LDA", "LDX", null, 
"TAY", "LDA", "TAX", null, "LDY", "LDA", "LDX", null, 
"BCS", "LDA", null, null, "LDY", "LDA", "LDX", null, 
"CLV", "LDA", "TSX", null, "LDY", "LDA", "LDX", null, 
"CPY", "CMP", null, null, "CPY", "CMP", "DEC", null, 
"INY", "CMP", "DEX", null, "CPY", "CMP", "DEC", null, 
"BNE", "CMP", null, null, null, "CMP", "DEC", null, 
"CLD", "CMP", null, null, null, "CMP", "DEC", null, 
"CPX", "SBC", null, null, "CPX", "SBC", "INC", null, 
"INX", "SBC", "NOP", null, "CPX", "SBC", "INC", null, 
"BEQ", "SBC", null, null, null, "SBC", "INC", null, 
"SED", "SBC", null, null, null, "SBC", "INC", null, 
/* END OPNAMES */
        };

        private static AddressMode[] addrModes = new AddressMode[] {
            /* BEGIN MODES */
AddressMode.Implied, AddressMode.IndexedIndirectX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Immediate, AddressMode.Accumulator, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedX, AddressMode.Implied, 
AddressMode.Absolute, AddressMode.IndexedIndirectX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Immediate, AddressMode.Accumulator, AddressMode.Implied, 
AddressMode.Absolute, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.IndexedIndirectX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Immediate, AddressMode.Accumulator, AddressMode.Implied, 
AddressMode.Absolute, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.IndexedIndirectX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Immediate, AddressMode.Accumulator, AddressMode.Implied, 
AddressMode.Indirect, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.IndexedIndirectX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Implied, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Absolute, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedY, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Immediate, AddressMode.IndexedIndirectX, AddressMode.Immediate, AddressMode.Implied, 
AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Immediate, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Absolute, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedY, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedY, AddressMode.Implied, 
AddressMode.Immediate, AddressMode.IndexedIndirectX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Immediate, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Absolute, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedX, AddressMode.Implied, 
AddressMode.Immediate, AddressMode.IndexedIndirectX, AddressMode.Implied, AddressMode.Implied, 
AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.ZeroPage, AddressMode.Implied, 
AddressMode.Implied, AddressMode.Immediate, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Absolute, AddressMode.Absolute, AddressMode.Absolute, AddressMode.Implied, 
AddressMode.Relative, AddressMode.IndirectIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.ZeroPageIndexedX, AddressMode.ZeroPageIndexedX, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedY, AddressMode.Implied, AddressMode.Implied, 
AddressMode.Implied, AddressMode.AbsoluteIndexedX, AddressMode.AbsoluteIndexedX, AddressMode.Implied, 
/* END MODES */
        };
    }
}
