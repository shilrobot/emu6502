using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public abstract class Mapper
    {
        protected Nes nes;

        public Mapper(Nes nes)
        {
            this.nes = nes;
        }

        public abstract void Reset();

        public abstract byte PrgRomRead(int addr);

        public virtual void PrgRomWrite(int addr, byte data)
        {
        }
    }
}
