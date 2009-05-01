using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    partial class Chip6502
    {
        private Dictionary<string, int> encountered = new Dictionary<string, int>();

        private void Encountered(string opname)
        {
            if (encountered.ContainsKey(opname))
                encountered[opname] += 1;
            else
                encountered[opname] = 1;
        }

        public void Tick()
        {
            if (Paused)
                return;

            ushort NPC;
            ushort addr;
            byte data;

            byte opcode = Read(PC);
            switch (opcode)
            {
                /* BEGIN SWITCH */
case 0x00:
{
Encountered("BRK");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: BRK", PC);
NPC++;
if(!I) {
 PushWord(NPC);
 PushStatus(true);
 I = true;
 NPC = ReadWord(IRQAddr);
}
}
break;
case 0x01:
{
Encountered("ORA");
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x05:
{
Encountered("ORA");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x06:
{
Encountered("ASL");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
C = (data & 0x80)!=0;
data <<= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x08:
{
Encountered("PHP");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: PHP", PC);
PushStatus(false);
}
break;
case 0x09:
{
Encountered("ORA");
NPC = (ushort)(PC+2);
data = Read(PC+1);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x0A:
{
Encountered("ASL");
NPC = (ushort)(PC+1);
data = A;
C = (data & 0x80)!=0;
data <<= 1;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x0D:
{
Encountered("ORA");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x0E:
{
Encountered("ASL");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
C = (data & 0x80)!=0;
data <<= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x10:
{
Encountered("BPL");
NPC = (ushort)(PC+2);
if(!N) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x11:
{
Encountered("ORA");
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x15:
{
Encountered("ORA");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x16:
{
Encountered("ASL");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
C = (data & 0x80)!=0;
data <<= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x18:
{
Encountered("CLC");
NPC = (ushort)(PC+1);
C = false;
}
break;
case 0x19:
{
Encountered("ORA");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x1D:
{
Encountered("ORA");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x1E:
{
Encountered("ASL");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
C = (data & 0x80)!=0;
data <<= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x20:
{
Encountered("JSR");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
PushWord((ushort)(NPC-1));
NPC = addr;
}
break;
case 0x21:
{
Encountered("AND");
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x24:
{
Encountered("BIT");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
Console.WriteLine("${0:X4}: BIT", PC);
data = Read(addr);
N = (data & 0x80)!=0;
V = (data & 0x40)!=0;
Z = (A & data) == 0;
}
break;
case 0x25:
{
Encountered("AND");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x26:
{
Encountered("ROL");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
bool oldC = C;
C = (data & 0x80)!=0;
data <<= 1;
if(oldC) data |= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x28:
{
Encountered("PLP");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: PLP", PC);
PullStatus();
}
break;
case 0x29:
{
Encountered("AND");
NPC = (ushort)(PC+2);
data = Read(PC+1);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x2A:
{
Encountered("ROL");
NPC = (ushort)(PC+1);
data = A;
bool oldC = C;
C = (data & 0x80)!=0;
data <<= 1;
if(oldC) data |= 1;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x2C:
{
Encountered("BIT");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
Console.WriteLine("${0:X4}: BIT", PC);
data = Read(addr);
N = (data & 0x80)!=0;
V = (data & 0x40)!=0;
Z = (A & data) == 0;
}
break;
case 0x2D:
{
Encountered("AND");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x2E:
{
Encountered("ROL");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
bool oldC = C;
C = (data & 0x80)!=0;
data <<= 1;
if(oldC) data |= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x30:
{
Encountered("BMI");
NPC = (ushort)(PC+2);
if(N) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x31:
{
Encountered("AND");
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x35:
{
Encountered("AND");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x36:
{
Encountered("ROL");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
bool oldC = C;
C = (data & 0x80)!=0;
data <<= 1;
if(oldC) data |= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x38:
{
Encountered("SEC");
NPC = (ushort)(PC+1);
C = true;
}
break;
case 0x39:
{
Encountered("AND");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x3D:
{
Encountered("AND");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x3E:
{
Encountered("ROL");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
bool oldC = C;
C = (data & 0x80)!=0;
data <<= 1;
if(oldC) data |= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x40:
{
Encountered("RTI");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: RTI", PC);
PullStatus();
NPC = PullWord();
}
break;
case 0x41:
{
Encountered("EOR");
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x45:
{
Encountered("EOR");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x46:
{
Encountered("LSR");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
C = (data & 0x01)!=0;
data >>= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x48:
{
Encountered("PHA");
NPC = (ushort)(PC+1);
Push(A);
}
break;
case 0x49:
{
Encountered("EOR");
NPC = (ushort)(PC+2);
data = Read(PC+1);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x4A:
{
Encountered("LSR");
NPC = (ushort)(PC+1);
data = A;
C = (data & 0x01)!=0;
data >>= 1;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x4C:
{
Encountered("JMP");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
NPC = addr;
}
break;
case 0x4D:
{
Encountered("EOR");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x4E:
{
Encountered("LSR");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
C = (data & 0x01)!=0;
data >>= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x50:
{
Encountered("BVC");
NPC = (ushort)(PC+2);
if(!V) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x51:
{
Encountered("EOR");
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x55:
{
Encountered("EOR");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x56:
{
Encountered("LSR");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
C = (data & 0x01)!=0;
data >>= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x58:
{
Encountered("CLI");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: CLI", PC);
I = false;
}
break;
case 0x59:
{
Encountered("EOR");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x5D:
{
Encountered("EOR");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x5E:
{
Encountered("LSR");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
C = (data & 0x01)!=0;
data >>= 1;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x60:
{
Encountered("RTS");
NPC = (ushort)(PC+1);
NPC = (ushort)(PullWord()+1);
}
break;
case 0x66:
{
Encountered("ROR");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
bool oldC = C;
C = (data & 0x01)!=0;
data >>= 1;
if(oldC) data |= 0x80;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x68:
{
Encountered("PLA");
NPC = (ushort)(PC+1);
data = Pull();
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x6A:
{
Encountered("ROR");
NPC = (ushort)(PC+1);
data = A;
bool oldC = C;
C = (data & 0x01)!=0;
data >>= 1;
if(oldC) data |= 0x80;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x6C:
{
Encountered("JMP");
NPC = (ushort)(PC+3);
{ ushort addr1 = ReadWord(PC+1); byte addr2_lo = Read(addr1); addr1 = (ushort)((addr1 & 0xFF00) | ((addr1+1)&0xFF)); byte addr2_hi = Read(addr1); addr = (ushort)(addr2_lo | (addr2_hi<<8)); }
NPC = addr;
}
break;
case 0x6E:
{
Encountered("ROR");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
bool oldC = C;
C = (data & 0x01)!=0;
data >>= 1;
if(oldC) data |= 0x80;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x70:
{
Encountered("BVS");
NPC = (ushort)(PC+2);
if(V) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x76:
{
Encountered("ROR");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
bool oldC = C;
C = (data & 0x01)!=0;
data >>= 1;
if(oldC) data |= 0x80;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x78:
{
Encountered("SEI");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: SEI", PC);
I = true;
}
break;
case 0x7E:
{
Encountered("ROR");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
bool oldC = C;
C = (data & 0x01)!=0;
data >>= 1;
if(oldC) data |= 0x80;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0x81:
{
Encountered("STA");
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = A;
Write(addr, data);
}
break;
case 0x84:
{
Encountered("STY");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Y;
Write(addr, data);
}
break;
case 0x85:
{
Encountered("STA");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = A;
Write(addr, data);
}
break;
case 0x86:
{
Encountered("STX");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = X;
Write(addr, data);
}
break;
case 0x88:
{
Encountered("DEY");
NPC = (ushort)(PC+1);
Y--;
data = Y;
Z = (data == 0); N = (data > 127);
}
break;
case 0x8A:
{
Encountered("TXA");
NPC = (ushort)(PC+1);
data = X;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x8C:
{
Encountered("STY");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Y;
Write(addr, data);
}
break;
case 0x8D:
{
Encountered("STA");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = A;
Write(addr, data);
}
break;
case 0x8E:
{
Encountered("STX");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = X;
Write(addr, data);
}
break;
case 0x90:
{
Encountered("BCC");
NPC = (ushort)(PC+2);
if(!C) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x91:
{
Encountered("STA");
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = A;
Write(addr, data);
}
break;
case 0x94:
{
Encountered("STY");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Y;
Write(addr, data);
}
break;
case 0x95:
{
Encountered("STA");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = A;
Write(addr, data);
}
break;
case 0x96:
{
Encountered("STX");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+Y)&0xFF);
data = X;
Write(addr, data);
}
break;
case 0x98:
{
Encountered("TYA");
NPC = (ushort)(PC+1);
data = Y;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x99:
{
Encountered("STA");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = A;
Write(addr, data);
}
break;
case 0x9A:
{
Encountered("TXS");
NPC = (ushort)(PC+1);
SP = X;
}
break;
case 0x9D:
{
Encountered("STA");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = A;
Write(addr, data);
}
break;
case 0xA0:
{
Encountered("LDY");
NPC = (ushort)(PC+2);
data = Read(PC+1);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA1:
{
Encountered("LDA");
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA2:
{
Encountered("LDX");
NPC = (ushort)(PC+2);
data = Read(PC+1);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA4:
{
Encountered("LDY");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA5:
{
Encountered("LDA");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA6:
{
Encountered("LDX");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA8:
{
Encountered("TAY");
NPC = (ushort)(PC+1);
data = A;
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA9:
{
Encountered("LDA");
NPC = (ushort)(PC+2);
data = Read(PC+1);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAA:
{
Encountered("TAX");
NPC = (ushort)(PC+1);
data = A;
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAC:
{
Encountered("LDY");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAD:
{
Encountered("LDA");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAE:
{
Encountered("LDX");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB0:
{
Encountered("BCS");
NPC = (ushort)(PC+2);
if(C) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0xB1:
{
Encountered("LDA");
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB4:
{
Encountered("LDY");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB5:
{
Encountered("LDA");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB6:
{
Encountered("LDX");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+Y)&0xFF);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB8:
{
Encountered("CLV");
NPC = (ushort)(PC+1);
V = false;
}
break;
case 0xB9:
{
Encountered("LDA");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBA:
{
Encountered("TSX");
NPC = (ushort)(PC+1);
data = SP;
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBC:
{
Encountered("LDY");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBD:
{
Encountered("LDA");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBE:
{
Encountered("LDX");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xC0:
{
Encountered("CPY");
NPC = (ushort)(PC+2);
data = Read(PC+1);
C = Y >= data;
data = (byte)(Y - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xC1:
{
Encountered("CMP");
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xC4:
{
Encountered("CPY");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
C = Y >= data;
data = (byte)(Y - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xC5:
{
Encountered("CMP");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xC6:
{
Encountered("DEC");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
--data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xC8:
{
Encountered("INY");
NPC = (ushort)(PC+1);
Y++;
data = Y;
Z = (data == 0); N = (data > 127);
}
break;
case 0xC9:
{
Encountered("CMP");
NPC = (ushort)(PC+2);
data = Read(PC+1);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xCA:
{
Encountered("DEX");
NPC = (ushort)(PC+1);
X--;
data = X;
Z = (data == 0); N = (data > 127);
}
break;
case 0xCC:
{
Encountered("CPY");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
C = Y >= data;
data = (byte)(Y - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xCD:
{
Encountered("CMP");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xCE:
{
Encountered("DEC");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
--data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xD0:
{
Encountered("BNE");
NPC = (ushort)(PC+2);
if(!Z) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0xD1:
{
Encountered("CMP");
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xD5:
{
Encountered("CMP");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xD6:
{
Encountered("DEC");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
--data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xD8:
{
Encountered("CLD");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: CLD", PC);
}
break;
case 0xD9:
{
Encountered("CMP");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xDD:
{
Encountered("CMP");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xDE:
{
Encountered("DEC");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
--data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xE0:
{
Encountered("CPX");
NPC = (ushort)(PC+2);
data = Read(PC+1);
C = X >= data;
data = (byte)(X - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xE4:
{
Encountered("CPX");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
C = X >= data;
data = (byte)(X - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xE6:
{
Encountered("INC");
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
++data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xE8:
{
Encountered("INX");
NPC = (ushort)(PC+1);
X++;
data = X;
Z = (data == 0); N = (data > 127);
}
break;
case 0xEA:
{
Encountered("NOP");
NPC = (ushort)(PC+1);
}
break;
case 0xEC:
{
Encountered("CPX");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
C = X >= data;
data = (byte)(X - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xEE:
{
Encountered("INC");
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
++data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xF0:
{
Encountered("BEQ");
NPC = (ushort)(PC+2);
if(Z) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0xF6:
{
Encountered("INC");
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
++data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xF8:
{
Encountered("SED");
NPC = (ushort)(PC+1);
Console.WriteLine("${0:X4}: SED", PC);
}
break;
case 0xFE:
{
Encountered("INC");
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
++data;
Write(addr, data);
Z = (data == 0); N = (data > 127);
}
break;

/* END SWITCH */
                default:
                    // TODO: Make breaking on this opcode actually fucking work
                    NPC = (ushort)(PC + 1);
                    /*if (ignoreOpcodes < 10)
                    {
                        Console.WriteLine("Invalid Opcode ${0:X2} @ ${1:X4}: treating as NOP", opcode, PC);
                        ++ignoreOpcodes;
                        if (ignoreOpcodes == 10)
                            Console.WriteLine("Suppressing further invalid opcode messages");
                    }*/
                    Console.WriteLine("Invalid Opcode ${0:X2} @ ${1:X4}: treating as NOP", opcode, PC);
                    this.Paused = true;

                    return;
            }

            if ((PC & 0xF000) != (NPC & 0xF000))
                Console.WriteLine("Jumping from ${0:X2} to ${1:X2}", PC, NPC);

            SetPC(NPC);
        }
    }
}
