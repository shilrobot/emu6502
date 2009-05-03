using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emu6502
{
    public class MMC1 : Mapper
    {
        int writeCount = 0;
        int shiftReg = 0;
        byte[] PrgRomBank0;
        byte[] PrgRomBank1;

        private int CtrlReg;
        private int ChrBank0Reg;
        private int ChrBank1Reg;
        private int PrgBankReg;

        public MMC1(Nes nes)
            : base(nes)
        {
        }

        public override void Reset()
        {
            CtrlReg = 0;
            ChrBank0Reg = 0;
            ChrBank1Reg = 0;
            PrgBankReg = 0;

            // TODO: Do the right thing here... requires changing PPU :(
            if (nes.Rom.ChrRomBanks.Length > 0)
                nes.Ppu.PatternTables = nes.Rom.ChrRomBanks[0];
            else
                nes.Ppu.PatternTables = new byte[8 * 1024];

            PrgRomBank0 = nes.Rom.PrgRomBanks[0];
            PrgRomBank1 = nes.Rom.PrgRomBanks[nes.Rom.PrgRomBanks.Length - 1];
            CtrlReg = 0xC;
            //SetRegister(0, 0xC);
        }

        public override byte PrgRomRead(int addr)
        {
            if (addr >= 0x8000 && addr < 0xC000)
                return PrgRomBank0[addr & 0x3FFF];
            else
                return PrgRomBank1[addr & 0x3FFF];
        }

        private void UpdatePrgBanks()
        {
            if ((CtrlReg & 0x8) != 0)
            {
                // Switch 16K at address specified in prgRomLocation bit (0=$C000, 1=$8000)
                if ((CtrlReg & 0x4) != 0)
                {
                    /*Console.WriteLine("Switching PRG-ROM banks @ PC=${0:X}", nes.Cpu.PC);
                    Console.WriteLine("  $8000 -> Bank ${0:X2}", PrgBankReg & 0xF);
                    Console.WriteLine("  $C000 -> Bank ${0:X2}", nes.Rom.PrgRomBanks.Length - 1);*/

                    // Switch $8000
                    PrgRomBank0 = nes.Rom.PrgRomBanks[PrgBankReg & 0xF];
                    PrgRomBank1 = nes.Rom.PrgRomBanks[nes.Rom.PrgRomBanks.Length - 1];
                }
                else
                {
                    /*Console.WriteLine("Switching PRG-ROM banks @ PC=${0:X}", nes.Cpu.PC);
                    Console.WriteLine("  $8000 -> Bank ${0:X2}", 0);
                    Console.WriteLine("  $C000 -> Bank ${0:X2}", PrgBankReg & 0xF);*/

                    // Switch $C000
                    PrgRomBank0 = nes.Rom.PrgRomBanks[0];
                    PrgRomBank1 = nes.Rom.PrgRomBanks[PrgBankReg & 0xF];
                }
            }
            else
            {
                // Switch 32 KB at $C000
                // TODO
            }
        }

        private void SetRegister(int whichReg, int regValue)
        {
            //Console.WriteLine("Register{1} = ${0:X2}", regValue, whichReg);
            regValue &= 0x1F;

            // Control
            if (whichReg == 0)
            {
                CtrlReg = regValue;

                int mirrorMode = regValue & 0x3;
                bool prgRomLocation = (regValue & 0x4) != 0;
                bool prgRomMode = (regValue & 0x8) != 0;
                bool chrRomMode = (regValue & 0x10) != 0;

                MirrorType mirrorType;

                if (mirrorMode == 0)
                    mirrorType = MirrorType.SingleScreenLower;
                else if (mirrorMode == 1)
                    mirrorType = MirrorType.SingleScreenUpper;
                else if (mirrorMode == 2)
                    mirrorType = MirrorType.Vertical;
                else
                    mirrorType = MirrorType.Horizontal;
                //mirrorType = MirrorType.FourScreen;

                nes.Ppu.SetMirroring(mirrorType);

                /*
                Console.WriteLine("Control Reg:");
                Console.WriteLine("  Mirror={0} ({1})", mirrorMode, mirrorType);
                Console.WriteLine("  PRG-ROM Location={0} ({1})", prgRomLocation ? 1 : 0, prgRomLocation ? "Switch $8000" : "Switch $C000");
                Console.WriteLine("  PRG-ROM Mode={0} ({1})", prgRomMode ? 1 : 0, prgRomMode ? "Switch 16KB at address from Location bit" : "Switch 32K at $8000, ignore low bit");
                Console.WriteLine("  CHR-ROM Mode={0} ({1})", chrRomMode ? 1 : 0, chrRomMode ? "Separate 4K banks" : "Switch 8K at a time");
                */

                UpdatePrgBanks();
            }
            // CHR-ROM 0
            else if (whichReg == 1)
            {
                ChrBank0Reg = regValue;
            }
            // CHR-ROM 1
            else if (whichReg == 2)
            {
                ChrBank1Reg = regValue;

            }
            // PRG-ROM
            else if (whichReg == 3)
            {
                PrgBankReg = regValue;
                UpdatePrgBanks();
            }
        }

        public override void PrgRomWrite(int addr, byte data)
        {
            //Console.WriteLine("PRG-ROM write: ${0:X4} = ${1:X2}", addr,data);
            int whichReg = (addr & 0x7fff) >> 13;

            if ((data & 0x80) != 0)
            {
                //CtrlReg |= 0x0C;
                SetRegister(0, CtrlReg | 0x0C);
                writeCount = 0;
                shiftReg = 0;
            }
            else
            {
                if ((data & 0x1) != 0)
                    shiftReg |= data << writeCount;
                ++writeCount;
                if (writeCount == 5)
                {
                    SetRegister(whichReg, shiftReg);
                    writeCount = 0;
                    shiftReg = 0;
                }
            }
        }
    }
}
