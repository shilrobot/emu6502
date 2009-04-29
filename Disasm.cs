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

    // TODO: Make this more table-driven (we can basically do EVERYTHING with a table)
    // e.g. INSTR[OP] = NAME/ACTION, ADDRMODE (-> implied byte count), CYCLECOUNT (later)

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
        public String Result = "";
        private StringBuilder sb = new StringBuilder();
        private byte[] data;
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
            return addr == data.Length;
        }

        private byte Next()
        {
            if (AtEnd())
                throw new DisassemblyException("Encountered end of bytestream prematurely");
            return data[addr++];
        }

        public string Immediate()
        {
            return String.Format("#${0:X2}", Next());
        }

        public string ZeroPage()
        {
            return String.Format("${0:X2}", Next());
        }

        public string ZeroPageIndexedX()
        {
            return String.Format("${0:X2},X", Next());
        }

        public string ZeroPageIndexedY()
        {
            return String.Format("${0:X2},Y", Next());
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
            // TODO: Appropriate sign shit/maybe print out full address
            return String.Format("${0:X2}", Next());
        }

        public void Disassemble()
        {
            while (DisassembleOne()) ;
        }

        // TODO: For bytes at very end, print out DB's instead of
        //       raising an exception!
        private bool DisassembleOne()
        {
            if (AtEnd())
            {
                Result = sb.ToString();
                return false;
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
                case 0xAE: name="LDX"; operand=Absolute(); break;
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
                case 0x1E: name="ASL"; operand=AbsoluteIndexedX(); break;
                case 0x06: name="ASL"; operand=ZeroPage(); break;
                case 0x16: name="ASL"; operand=ZeroPageIndexedX(); break;
                case 0x0A: name="ASL"; operand=Accumulator(); break;
                case 0x0E: name="ASL"; operand=Absolute(); break;
                case 0x5E: name="LSR"; operand=AbsoluteIndexedX(); break;
                case 0x46: name="LSR"; operand=ZeroPage(); break;
                case 0x56: name="LSR"; operand=ZeroPageIndexedX(); break;
                case 0x4A: name="LSR"; operand=Accumulator(); break;
                case 0x4E: name="LSR"; operand=Absolute(); break;
                case 0x3E: name="ROL"; operand=AbsoluteIndexedX(); break;
                case 0x26: name="ROL"; operand=ZeroPage(); break;
                case 0x36: name="ROL"; operand=ZeroPageIndexedX(); break;
                case 0x2A: name="ROL"; operand=Accumulator(); break;
                case 0x2E: name="ROL"; operand=Absolute(); break;
                case 0x7E: name="ROR"; operand=AbsoluteIndexedX(); break;
                case 0x66: name="ROR"; operand=ZeroPage(); break;
                case 0x76: name="ROR"; operand=ZeroPageIndexedX(); break;
                case 0x6A: name="ROR"; operand=Accumulator(); break;
                case 0x6E: name="ROR"; operand=Absolute(); break;
                case 0xE6: name="INC"; operand=ZeroPage(); break;
                case 0xF6: name="INC"; operand=ZeroPageIndexedX(); break;
                case 0xFE: name="INC"; operand=AbsoluteIndexedX(); break;
                case 0xEE: name="INC"; operand=Absolute(); break;
                case 0xC6: name="DEC"; operand=ZeroPage(); break;
                case 0xD6: name="DEC"; operand=ZeroPageIndexedX(); break;
                case 0xDE: name="DEC"; operand=AbsoluteIndexedX(); break;
                case 0xCE: name="DEC"; operand=Absolute(); break;
                case 0x6C: name="JMP"; operand=Indirect(); break;
                case 0x4C: name="JMP"; operand=Absolute(); break;
                case 0x20: name="JSR"; operand=Absolute(); break;
                case 0x00: name="BRK"; operand=""; break;
                case 0x60: name="RTS"; operand=""; break;
                case 0x40: name="RTI"; operand=""; break;
                case 0x08: name="PHP"; operand=""; break;
                case 0x28: name="PLP"; operand=""; break;
                case 0x48: name="PHA"; operand=""; break;
                case 0x68: name="PLA"; operand=""; break;
                case 0xE8: name="INX"; operand=""; break;
                case 0xCA: name="DEX"; operand=""; break;
                case 0xC8: name="INY"; operand=""; break;
                case 0x88: name="DEY"; operand=""; break;
                case 0xAA: name="TAX"; operand=""; break;
                case 0x8A: name="TXA"; operand=""; break;
                case 0xA8: name="TAY"; operand=""; break;
                case 0x98: name="TYA"; operand=""; break;
                case 0xBA: name="TSX"; operand=""; break;
                case 0x9A: name="TXS"; operand=""; break;
                case 0xF8: name="SED"; operand=""; break;
                case 0xD8: name="CLD"; operand=""; break;
                case 0x78: name="SEI"; operand=""; break;
                case 0x58: name="CLI"; operand=""; break;
                case 0x38: name="SEC"; operand=""; break;
                case 0x18: name="CLC"; operand=""; break;
                case 0xB8: name="CLV"; operand=""; break;
                case 0xF0: name="BEQ"; operand=Relative(); break;
                case 0xD0: name="BNE"; operand=Relative(); break;
                case 0x30: name="BMI"; operand=Relative(); break;
                case 0x10: name="BPL"; operand=Relative(); break;
                case 0x90: name="BCC"; operand=Relative(); break;
                case 0xB0: name="BCS"; operand=Relative(); break;
                case 0x50: name="BVC"; operand=Relative(); break;
                case 0x70: name="BVS"; operand=Relative(); break;
                case 0xEA: name="NOP"; operand=""; break;
            /* END GENERATED */
                default:
                    name = ".DB";
                    operand = String.Format("${0:X2}", opcode);
                    break;
            }

            int byteCount = addr - instrStartAddr;
            if (byteCount < 1 || byteCount > 3)
                throw new DisassemblyException("Something is wrong: byte count < 1 or > 3 for instr");

            sb.AppendFormat("{0:X4}   ", instrStartAddr);
            for (int i = instrStartAddr; i < instrStartAddr + byteCount; ++i)
                sb.AppendFormat("{0:X2} ", data[i]);
            for (int i = 0; i < 3 - byteCount; ++i)
                sb.Append("   ");
            sb.Append("  ");
            sb.Append(name);
            sb.Append(" ");
            sb.Append(operand);
            sb.Append("\r\n");

            return true;
        }
        
        public static void DB(StringBuilder sb, int addr, byte b)
        {
            sb.AppendFormat("{0:X4}   {0:X2}         .DB ${0:X2}", addr, b);
        }

        public static bool NewDisassemble(byte[] data, int addr, StringBuilder sb, out int bytesConsumed)
        {
            bytesConsumed = 0;

            if (data == null)
                throw new ArgumentNullException("data");

            if (sb == null)
                throw new ArgumentNullException("sb");

            if (addr < 0 ||
                addr > 0xFFFF ||
                addr >= data.Length)
                throw new ArgumentOutOfRangeException("addr");

            byte opcode = data[addr];
            if (opNames[opcode] == null)
                return false;
            AddressMode mode = addrModes[opcode];
            int byteCount = byteCounts[(int)mode];
            if (data.Length - addr < byteCount)
                return false;
            byte lo = 0, hi = 0;
            if (byteCount > 0)
                lo = data[addr + 1];
            if (byteCount > 1)
                hi = data[addr + 2];
            
            if(byteCount == 3)
                sb.AppendFormat("{0:X4}   {1:X2} {2:X2} {3:X2}   {4}", addr, opcode, lo, hi, opNames[opcode]);
            else if (byteCount == 2)
                sb.AppendFormat("{0:X4}   {1:X2} {2:X2}      {3}", addr, opcode, lo, opNames[opcode]);
            else if (byteCount == 1)
                sb.AppendFormat("{0:X4}   {1:X2}         {2}", addr, opcode, opNames[opcode]);

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
                    sb.AppendFormat(" A", lo);
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
                    sb.AppendFormat(" ${0:X2}", lo);
                    break;
            }

            sb.Append("\r\n");

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
AddressMode.Implied,
AddressMode.IndexedIndirectX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.Accumulator,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedX,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.IndexedIndirectX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.Accumulator,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.IndexedIndirectX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.Accumulator,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.IndexedIndirectX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.Accumulator,
AddressMode.Implied,
AddressMode.Indirect,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.IndexedIndirectX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.IndexedIndirectX,
AddressMode.Immediate,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.IndexedIndirectX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedX,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.IndexedIndirectX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.ZeroPage,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Immediate,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Absolute,
AddressMode.Implied,
AddressMode.Relative,
AddressMode.IndirectIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.ZeroPageIndexedX,
AddressMode.ZeroPageIndexedX,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedY,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.Implied,
AddressMode.AbsoluteIndexedX,
AddressMode.AbsoluteIndexedX,
AddressMode.Implied,
/* END MODES */
        };
    }
}
