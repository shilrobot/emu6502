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
        
        // From FCEUX, no need to retype all this crap
        static int[] Cycles =
        {                             
        /*0x00*/ 7,6,2,8,3,3,5,5,3,2,2,2,4,4,6,6,
        /*0x10*/ 2,5,2,8,4,4,6,6,2,4,2,7,4,4,7,7,
        /*0x20*/ 6,6,2,8,3,3,5,5,4,2,2,2,4,4,6,6,
        /*0x30*/ 2,5,2,8,4,4,6,6,2,4,2,7,4,4,7,7,
        /*0x40*/ 6,6,2,8,3,3,5,5,3,2,2,2,3,4,6,6,
        /*0x50*/ 2,5,2,8,4,4,6,6,2,4,2,7,4,4,7,7,
        /*0x60*/ 6,6,2,8,3,3,5,5,4,2,2,2,5,4,6,6,
        /*0x70*/ 2,5,2,8,4,4,6,6,2,4,2,7,4,4,7,7,
        /*0x80*/ 2,6,2,6,3,3,3,3,2,2,2,2,4,4,4,4,
        /*0x90*/ 2,6,2,6,4,4,4,4,2,5,2,5,5,5,5,5,
        /*0xA0*/ 2,6,2,6,3,3,3,3,2,2,2,2,4,4,4,4,
        /*0xB0*/ 2,5,2,5,4,4,4,4,2,4,2,4,4,4,4,4,
        /*0xC0*/ 2,6,2,8,3,3,5,5,2,2,2,2,4,4,6,6,
        /*0xD0*/ 2,5,2,8,4,4,6,6,2,4,2,7,4,4,7,7,
        /*0xE0*/ 2,6,3,8,3,3,5,5,2,2,2,2,4,4,6,6,
        /*0xF0*/ 2,5,2,8,4,4,6,6,2,4,2,7,4,4,7,7,
        };

        public void Tick()
        {
            if (Paused)
                return;

            if (cycles > 1)
            {
                cycles--;
                return;
            }
            else
                cycles = 0;
            
            ushort NPC;
            ushort addr;
            byte data;

            byte opcode = Read(PC);
            switch (opcode)
            {
                /* BEGIN SWITCH */
case 0x00:
{
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: BRK", PC);
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
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: PHP", PC);
PushStatus(false);
}
break;
case 0x09:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
data |= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x0A:
{
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
NPC = (ushort)(PC+2);
if(!N) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x11:
{
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
NPC = (ushort)(PC+1);
C = false;
}
break;
case 0x19:
{
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
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
PushWord((ushort)(NPC-1));
NPC = addr;
}
break;
case 0x21:
{
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
NPC = (ushort)(PC+2);
addr = Read(PC+1);
if(false) Console.WriteLine("${0:X4}: BIT", PC);
data = Read(addr);
N = (data & 0x80)!=0;
V = (data & 0x40)!=0;
Z = (A & data) == 0;
}
break;
case 0x25:
{
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
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: PLP", PC);
PullStatus();
}
break;
case 0x29:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
data &= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x2A:
{
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
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
if(false) Console.WriteLine("${0:X4}: BIT", PC);
data = Read(addr);
N = (data & 0x80)!=0;
V = (data & 0x40)!=0;
Z = (A & data) == 0;
}
break;
case 0x2D:
{
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
NPC = (ushort)(PC+2);
if(N) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x31:
{
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
NPC = (ushort)(PC+1);
C = true;
}
break;
case 0x39:
{
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
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: RTI", PC);
PullStatus();
NPC = PullWord();
}
break;
case 0x41:
{
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
NPC = (ushort)(PC+1);
Push(A);
}
break;
case 0x49:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
data ^= A;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x4A:
{
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
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
NPC = addr;
}
break;
case 0x4D:
{
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
NPC = (ushort)(PC+2);
if(!V) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x51:
{
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
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: CLI", PC);
I = false;
}
break;
case 0x59:
{
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
NPC = (ushort)(PC+1);
NPC = (ushort)(PullWord()+1);
}
break;
case 0x61:
{
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x65:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x66:
{
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
NPC = (ushort)(PC+1);
data = Pull();
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x69:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x6A:
{
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
NPC = (ushort)(PC+3);
{ ushort addr1 = ReadWord(PC+1); byte addr2_lo = Read(addr1); addr1 = (ushort)((addr1 & 0xFF00) | ((addr1+1)&0xFF)); byte addr2_hi = Read(addr1); addr = (ushort)(addr2_lo | (addr2_hi<<8)); }
NPC = addr;
}
break;
case 0x6D:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x6E:
{
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
NPC = (ushort)(PC+2);
if(V) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x71:
{
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x75:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x76:
{
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
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: SEI", PC);
I = true;
}
break;
case 0x79:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x7D:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
int result = A + data + (C?1:0);
C = (result & 0xFF00) != 0;
V = ( (~(A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0x7E:
{
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
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = A;
Write(addr, data);
}
break;
case 0x84:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Y;
Write(addr, data);
}
break;
case 0x85:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = A;
Write(addr, data);
}
break;
case 0x86:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = X;
Write(addr, data);
}
break;
case 0x88:
{
NPC = (ushort)(PC+1);
Y--;
data = Y;
Z = (data == 0); N = (data > 127);
}
break;
case 0x8A:
{
NPC = (ushort)(PC+1);
data = X;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x8C:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Y;
Write(addr, data);
}
break;
case 0x8D:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = A;
Write(addr, data);
}
break;
case 0x8E:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = X;
Write(addr, data);
}
break;
case 0x90:
{
NPC = (ushort)(PC+2);
if(!C) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0x91:
{
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = A;
Write(addr, data);
}
break;
case 0x94:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Y;
Write(addr, data);
}
break;
case 0x95:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = A;
Write(addr, data);
}
break;
case 0x96:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+Y)&0xFF);
data = X;
Write(addr, data);
}
break;
case 0x98:
{
NPC = (ushort)(PC+1);
data = Y;
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0x99:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = A;
Write(addr, data);
}
break;
case 0x9A:
{
NPC = (ushort)(PC+1);
SP = X;
}
break;
case 0x9D:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = A;
Write(addr, data);
}
break;
case 0xA0:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA1:
{
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA2:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA4:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA5:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA6:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA8:
{
NPC = (ushort)(PC+1);
data = A;
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xA9:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAA:
{
NPC = (ushort)(PC+1);
data = A;
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAC:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAD:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xAE:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB0:
{
NPC = (ushort)(PC+2);
if(C) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0xB1:
{
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB4:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB5:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB6:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+Y)&0xFF);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xB8:
{
NPC = (ushort)(PC+1);
V = false;
}
break;
case 0xB9:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBA:
{
NPC = (ushort)(PC+1);
data = SP;
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBC:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
Y = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBD:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
A = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xBE:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
X = data;
Z = (data == 0); N = (data > 127);
}
break;
case 0xC0:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
C = Y >= data;
data = (byte)(Y - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xC1:
{
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
NPC = (ushort)(PC+1);
Y++;
data = Y;
Z = (data == 0); N = (data > 127);
}
break;
case 0xC9:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
C = A >= data;
data = (byte)(A - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xCA:
{
NPC = (ushort)(PC+1);
X--;
data = X;
Z = (data == 0); N = (data > 127);
}
break;
case 0xCC:
{
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
NPC = (ushort)(PC+2);
if(!Z) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0xD1:
{
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
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: CLD", PC);
}
break;
case 0xD9:
{
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
NPC = (ushort)(PC+2);
data = Read(PC+1);
C = X >= data;
data = (byte)(X - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xE1:
{
NPC = (ushort)(PC+2);
addr = ReadWord((ushort)(Read(PC+1)+X));
data = Read(addr);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xE4:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
C = X >= data;
data = (byte)(X - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xE5:
{
NPC = (ushort)(PC+2);
addr = Read(PC+1);
data = Read(addr);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xE6:
{
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
NPC = (ushort)(PC+1);
X++;
data = X;
Z = (data == 0); N = (data > 127);
}
break;
case 0xE9:
{
NPC = (ushort)(PC+2);
data = Read(PC+1);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xEA:
{
NPC = (ushort)(PC+1);
}
break;
case 0xEC:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
C = X >= data;
data = (byte)(X - data);
Z = (data == 0); N = (data > 127);
}
break;
case 0xED:
{
NPC = (ushort)(PC+3);
addr = ReadWord(PC+1);
data = Read(addr);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xEE:
{
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
NPC = (ushort)(PC+2);
if(Z) { byte lo = Read(PC+1); int offset = (lo <= 127) ? lo : lo - 256; NPC = (ushort)(PC+2+offset); };
}
break;
case 0xF1:
{
NPC = (ushort)(PC+2);
addr = (ushort)(ReadWord(Read(PC+1))+Y);
data = Read(addr);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xF5:
{
NPC = (ushort)(PC+2);
addr = (ushort)((Read(PC+1)+X)&0xFF);
data = Read(addr);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xF6:
{
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
NPC = (ushort)(PC+1);
if(false) Console.WriteLine("${0:X4}: SED", PC);
}
break;
case 0xF9:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+Y);
data = Read(addr);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xFD:
{
NPC = (ushort)(PC+3);
addr = (ushort)(ReadWord(PC+1)+X);
data = Read(addr);
int result = A - data - (C?0:1);
C = (result & 0xFF00) == 0;
V = ( ((A ^ data) & (A ^ (byte)result)) &0x80) != 0;
data = A = (byte)result;
Z = (data == 0); N = (data > 127);
}
break;
case 0xFE:
{
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
                        if(false) Console.WriteLine("Invalid Opcode ${0:X2} @ ${1:X4}: treating as NOP", opcode, PC);
                        ++ignoreOpcodes;
                        if (ignoreOpcodes == 10)
                            if(false) Console.WriteLine("Suppressing further invalid opcode messages");
                    }*/
                    if(false) Console.WriteLine("Invalid Opcode ${0:X2} @ ${1:X4}: treating as NOP", opcode, PC);
                    this.Paused = true;

                    return;
            }

            cycles += Cycles[opcode];

            if ((PC & 0xF000) != (NPC & 0xF000))
                if(false) Console.WriteLine("Jumping from ${0:X2} to ${1:X2}", PC, NPC);

            PC = NPC;
            if (SingleStep)
            {
                Paused = true;
                SingleStep = false;
            }
        }
    }
}
