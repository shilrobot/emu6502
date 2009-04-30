using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public interface IMemory
    {
        byte Read(int address);
        void Write(int address, byte data);
    }
}
