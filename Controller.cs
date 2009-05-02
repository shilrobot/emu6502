using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class Controller
    {
        public bool A;
        public bool B;
        public bool Select;
        public bool Start;
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;

        public byte Captured;

        public void Capture()
        {
            Console.WriteLine("Controller Status: {0}{1}{2}{3}{4}{5}{6}{7}",
                A ? "A " : "",
                B ? "B " : "",
                Select ? "Select " : "",
                Start ? "Start " : "",
                Up ? "Up " : "",
                Down ? "Down " : "",
                Left ? "Left " : "",
                Right ? "Right " : "");
            Captured = 0;
            if (A)      Captured |= 0x01;
            if (B)      Captured |= 0x02;
            if (Select) Captured |= 0x04;
            if (Start)  Captured |= 0x08;
            if (Up)     Captured |= 0x10;
            if (Down)   Captured |= 0x20;
            if (Left)   Captured |= 0x40;
            if (Right)  Captured |= 0x80;
        }
    };
}
