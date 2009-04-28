using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class DisassemblyException : Exception
    {
        public DisassemblyException(string message) : base(message) { }
    }

    public class Disassembler
    {
        public String Result = "";
        private StringBuilder sb = new StringBuilder();
        private byte[] data;
        private int pos = 0;
        private int addr;

        public Disassembler(int startAddr, byte[] data)
        {
            if (startAddr < 0 || startAddr > 0xFFFF)
                throw new ArgumentOutOfRangeException("startAddr");
            this.addr = startAddr;
            this.data = data;
        }

        private bool AtEnd()
        {
            return pos == data.Length;
        }

        private byte Next()
        {
            if (AtEnd())
                throw new DisassemblyException("Encountered end of bytestream prematurely");
            return data[pos++];
        }

        public string Immediate()
        {
            return String.Format("#${0:X2}", Next());
        }

        public string ZeroPage()
        {
            return String.Format("#${0:X2}", Next());
        }

        public string ZeroPageIndexedX()
        {
            return String.Format("#${0:X2},X", Next());
        }

        public string ZeroPageIndexedY()
        {
            return String.Format("#${0:X2},Y", Next());
        }

        public string Accumulator()
        {
            return "A";
        }

        public string Absolute()
        {
            byte low = Next();
            byte hi = Next();
            return String.Format("${0:X2}{1:X2}", hi, low);
        }

        public string AbsoluteIndexedX()
        {
            byte low = Next();
            byte hi = Next();
            return String.Format("${0:X2}{1:X2},X", hi, low);
        }

        public string AbsoluteIndexedY()
        {
            byte low = Next();
            byte hi = Next();
            return String.Format("${0:X2}{1:X2},Y", hi, low);
        }

        public string Indirect()
        {
            byte low = Next();
            byte hi = Next();
            return String.Format("(${0:X2}{1:X2})", hi, low);
        }

        public string IndexedIndirectX()
        {
            return String.Format("(${0:X2},X)", Next());
        }

        public string IndirectIndexedY()
        {
            return String.Format("(${0:X2}),Y", Next());
        }

        public string Implied()
        {
            return "";
        }

        public string Relative()
        {
            return String.Format("#${0:X2}", Next());
        }

        public void Disassemble()
        {
            if (AtEnd())
            {
                Result = sb.ToString();
                return;
            }

            if (addr > 0xFFFF)
                throw new DisassemblyException("Exceeded address $FFFF");

            int instrStartAddr = addr;

            byte opcode = Next();

            //sb.AppendFormat("{0:X4} ", addr, opcode);
            string name;
            string operand;

            switch (opcode)
            {
            /* BEGIN GENERATED */
                case 0xA5: name="LDA"; operand=ZeroPage(); break;
                case 0xA9: name="LDA"; operand=Immediate(); break;
                case 0xB5: name="LDA"; operand=ZeroPageIndexedX(); break;
                case 0xBD: name="LDA"; operand=AbsoluteIndexedX(); break;
                case 0xB9: name="LDA"; operand=AbsoluteIndexedY(); break;
                case 0xA1: name="LDA"; operand=IndexedIndirectX(); break;
                case 0xB1: name="LDA"; operand=IndirectIndexedY(); break;
                case 0xAD: name="LDA"; operand=Absolute(); break;
                case 0x85: name="STA"; operand=ZeroPage(); break;
                case 0x91: name="STA"; operand=IndirectIndexedY(); break;
                case 0x95: name="STA"; operand=ZeroPageIndexedX(); break;
                case 0x9D: name="STA"; operand=AbsoluteIndexedX(); break;
                case 0x81: name="STA"; operand=IndexedIndirectX(); break;
                case 0x99: name="STA"; operand=AbsoluteIndexedY(); break;
                case 0x8D: name="STA"; operand=Absolute(); break;
                case 0xB6: name="LDX"; operand=ZeroPageIndexedY(); break;
                case 0xA6: name="LDX"; operand=ZeroPage(); break;
                case 0xA2: name="LDX"; operand=Immediate(); break;
                case 0xBE: name="LDX"; operand=AbsoluteIndexedY(); break;
                case 0x86: name="STX"; operand=ZeroPage(); break;
                case 0x8E: name="STX"; operand=Absolute(); break;
                case 0x96: name="STX"; operand=ZeroPageIndexedY(); break;
                case 0xBC: name="LDY"; operand=AbsoluteIndexedX(); break;
                case 0xA4: name="LDY"; operand=ZeroPage(); break;
                case 0xB4: name="LDY"; operand=ZeroPageIndexedX(); break;
                case 0xA0: name="LDY"; operand=Immediate(); break;
                case 0xAC: name="LDY"; operand=Absolute(); break;
                case 0x84: name="STY"; operand=ZeroPage(); break;
                case 0x94: name="STY"; operand=ZeroPageIndexedX(); break;
                case 0x8C: name="STY"; operand=Absolute(); break;
                case 0x25: name="AND"; operand=ZeroPage(); break;
                case 0x29: name="AND"; operand=Immediate(); break;
                case 0x35: name="AND"; operand=ZeroPageIndexedX(); break;
                case 0x3D: name="AND"; operand=AbsoluteIndexedX(); break;
                case 0x39: name="AND"; operand=AbsoluteIndexedY(); break;
                case 0x21: name="AND"; operand=IndexedIndirectX(); break;
                case 0x31: name="AND"; operand=IndirectIndexedY(); break;
                case 0x2D: name="AND"; operand=Absolute(); break;
                case 0x05: name="ORA"; operand=ZeroPage(); break;
                case 0x09: name="ORA"; operand=Immediate(); break;
                case 0x15: name="ORA"; operand=ZeroPageIndexedX(); break;
                case 0x1D: name="ORA"; operand=AbsoluteIndexedX(); break;
                case 0x19: name="ORA"; operand=AbsoluteIndexedY(); break;
                case 0x01: name="ORA"; operand=IndexedIndirectX(); break;
                case 0x11: name="ORA"; operand=IndirectIndexedY(); break;
                case 0x0D: name="ORA"; operand=Absolute(); break;
                case 0x45: name="EOR"; operand=ZeroPage(); break;
                case 0x49: name="EOR"; operand=Immediate(); break;
                case 0x55: name="EOR"; operand=ZeroPageIndexedX(); break;
                case 0x5D: name="EOR"; operand=AbsoluteIndexedX(); break;
                case 0x59: name="EOR"; operand=AbsoluteIndexedY(); break;
                case 0x41: name="EOR"; operand=IndexedIndirectX(); break;
                case 0x51: name="EOR"; operand=IndirectIndexedY(); break;
                case 0x4D: name="EOR"; operand=Absolute(); break;
                case 0x24: name="BIT"; operand=ZeroPage(); break;
                case 0x2C: name="BIT"; operand=Absolute(); break;
                case 0xC5: name="CMP"; operand=ZeroPage(); break;
                case 0xC9: name="CMP"; operand=Immediate(); break;
                case 0xD5: name="CMP"; operand=ZeroPageIndexedX(); break;
                case 0xDD: name="CMP"; operand=AbsoluteIndexedX(); break;
                case 0xD9: name="CMP"; operand=AbsoluteIndexedY(); break;
                case 0xC1: name="CMP"; operand=IndexedIndirectX(); break;
                case 0xD1: name="CMP"; operand=IndirectIndexedY(); break;
                case 0xCD: name="CMP"; operand=Absolute(); break;
                case 0xE4: name="CPX"; operand=ZeroPage(); break;
                case 0xEC: name="CPX"; operand=Absolute(); break;
                case 0xE0: name="CPX"; operand=Immediate(); break;
                case 0xC4: name="CPY"; operand=ZeroPage(); break;
                case 0xCC: name="CPY"; operand=Absolute(); break;
                case 0xC0: name="CPY"; operand=Immediate(); break;
                case 0x65: name="ADC"; operand=ZeroPage(); break;
                case 0x69: name="ADC"; operand=Immediate(); break;
                case 0x75: name="ADC"; operand=ZeroPageIndexedX(); break;
                case 0x7D: name="ADC"; operand=AbsoluteIndexedX(); break;
                case 0x79: name="ADC"; operand=AbsoluteIndexedY(); break;
                case 0x61: name="ADC"; operand=IndexedIndirectX(); break;
                case 0x71: name="ADC"; operand=IndirectIndexedY(); break;
                case 0x6D: name="ADC"; operand=Absolute(); break;
                case 0xE5: name="SBC"; operand=ZeroPage(); break;
                case 0xE9: name="SBC"; operand=Immediate(); break;
                case 0xF5: name="SBC"; operand=ZeroPageIndexedX(); break;
                case 0xFD: name="SBC"; operand=AbsoluteIndexedX(); break;
                case 0xF9: name="SBC"; operand=AbsoluteIndexedY(); break;
                case 0xE1: name="SBC"; operand=IndexedIndirectX(); break;
                case 0xF1: name="SBC"; operand=IndirectIndexedY(); break;
                case 0xED: name="SBC"; operand=Absolute(); break;
                case 0x06: name="ASL"; operand=ZeroPage(); break;
                case 0x16: name="ASL"; operand=ZeroPageIndexedX(); break;
                case 0x1E: name="ASL"; operand=AbsoluteIndexedX(); break;
                case 0x03: name="ASL"; operand=Absolute(); break;
                case 0x46: name="LSR"; operand=ZeroPage(); break;
                case 0x56: name="LSR"; operand=ZeroPageIndexedX(); break;
                case 0x5E: name="LSR"; operand=AbsoluteIndexedX(); break;
                case 0x4E: name="LSR"; operand=Absolute(); break;
                case 0x26: name="ROL"; operand=ZeroPage(); break;
                case 0x26: name="ROL"; operand=ZeroPageIndexedX(); break;
                case 0x3E: name="ROL"; operand=AbsoluteIndexedX(); break;
                case 0x2E: name="ROL"; operand=Absolute(); break;
                case 0x66: name="ROR"; operand=ZeroPage(); break;
                case 0x76: name="ROR"; operand=ZeroPageIndexedX(); break;
                case 0x7E: name="ROR"; operand=AbsoluteIndexedX(); break;
                case 0x6E: name="ROR"; operand=Absolute(); break;
                case 0xE6: name="INC"; operand=ZeroPage(); break;
                case 0xF6: name="INC"; operand=ZeroPageIndexedX(); break;
                case 0xFE: name="INC"; operand=AbsoluteIndexedX(); break;
                case 0xEE: name="INC"; operand=Absolute(); break;
                case 0xC6: name="DEC"; operand=ZeroPage(); break;
                case 0xD6: name="DEC"; operand=ZeroPageIndexedX(); break;
                case 0xDE: name="DEC"; operand=AbsoluteIndexedX(); break;
                case 0xCE: name="DEC"; operand=Absolute(); break;
                case 0xFF: name="JMP"; operand=Indirect(); break;
                case 0x4C: name="JMP"; operand=Absolute(); break;
                case 0x20: name="JSR"; operand=Absolute(); break;
                case 0x00: name="BRK"; operand=Implied(); break;
                case 0x60: name="RTS"; operand=Implied(); break;
                case 0x40: name="RTI"; operand=Implied(); break;
                case 0x08: name="PHP"; operand=Implied(); break;
                case 0x28: name="PLP"; operand=Implied(); break;
                case 0x48: name="PHA"; operand=Implied(); break;
                case 0x68: name="PLA"; operand=Implied(); break;
                case 0xE8: name="INX"; operand=Implied(); break;
                case 0xCA: name="DEX"; operand=Implied(); break;
                case 0xC8: name="INY"; operand=Implied(); break;
                case 0x88: name="DEY"; operand=Implied(); break;
                case 0xAA: name="TAX"; operand=Implied(); break;
                case 0x8A: name="TXA"; operand=Implied(); break;
                case 0xA8: name="TAY"; operand=Implied(); break;
                case 0x98: name="TYA"; operand=Implied(); break;
                case 0xBA: name="TSX"; operand=Implied(); break;
                case 0x9A: name="TXS"; operand=Implied(); break;
                case 0xF8: name="SED"; operand=Implied(); break;
                case 0xD8: name="CLD"; operand=Implied(); break;
                case 0x78: name="SEI"; operand=Implied(); break;
                case 0x58: name="CLI"; operand=Implied(); break;
                case 0x38: name="SEC"; operand=Implied(); break;
                case 0x18: name="CLC"; operand=Implied(); break;
                case 0xB8: name="CLV"; operand=Implied(); break;
                case 0xF0: name="BEQ"; operand=Relative(); break;
                case 0xD0: name="BNE"; operand=Relative(); break;
                case 0x30: name="BMI"; operand=Relative(); break;
                case 0x10: name="BPL"; operand=Relative(); break;
                case 0x90: name="BCC"; operand=Relative(); break;
                case 0xB0: name="BCS"; operand=Relative(); break;
                case 0x50: name="BVC"; operand=Relative(); break;
                case 0x70: name="BVS"; operand=Relative(); break;
                case 0xEA: name="NOP"; operand=Implied(); break;
            /* END GENERATED */
                default:
                    name = "DB";
                    operand = String.Format("${0:X2}", opcode);
                    break;
            }

            int byteCount = addr - instrStartAddr;
            if (byteCount < 1 || byteCount > 3)
                throw new DisassemblyException("Something is wrong: byte count < 1 or > 3 for instr");

            sb.AppendFormat("{0:X4} ", addr);
            for (int i = instrStartAddr; i < instrStartAddr + byteCount; ++i)
                sb.AppendFormat("{0:X2} ", data[i]);
            for (int i = 0; i < 3 - byteCount; ++i)
                sb.Append("   ");
            sb.Append(name);
            sb.Append(" ");
            sb.Append(operand);
            sb.Append("\n");
        }
    }
}
