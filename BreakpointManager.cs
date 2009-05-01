using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class Breakpoint
    {
        public ushort Address { get; set; }
        public bool Enabled;

        public Breakpoint(ushort addr) 
        {
            Address = addr;
            Enabled = true;
        }
    }

    public class BreakpointManager
    {
        private Dictionary<ushort, Breakpoint> breakpoints = new Dictionary<ushort, Breakpoint>();

        public Breakpoint SetBreakpoint(ushort addr)
        {
            if (breakpoints.ContainsKey(addr))
                return breakpoints[addr];
            else
            {
                Breakpoint bp = new Breakpoint(addr);
                breakpoints[addr] = bp;
                return bp;
            }
        }

        public void ClearBreakpoint(ushort addr)
        {
            breakpoints.Remove(addr);
        }

        public Breakpoint GetBreakpoint(ushort addr)
        {
            Breakpoint bp = null;
            if(breakpoints.TryGetValue(addr, out bp))
                return bp;
            else
                return null;
        }

        public List<ushort> GetBreakpointAddresses()
        {
            return new List<ushort>(breakpoints.Keys);
        }
    }
}
