using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Emu6502
{
    public class RomFormatException : Exception
    {
        public RomFormatException(string message) : base(message) { }
    }

    // Reads in a .NES format ROM file
    public class Rom
    {
        public byte[][] PrgRomBanks;
        public byte[][] ChrRomBanks;
        public byte[][] RamBanks;

        private static string GetMapperName(int number)
        {
            switch (number)
            {
                case 0: return "NROM (no mapper)";
                case 1: return "MMC1";
                case 2: return "UNROM switch";
                case 3: return "CNROM switch";
                case 4: return "MMC3";
                case 5: return "MMC5";
                case 9: return "MMC2";
                case 10: return "MMC4";
                default:
                    return "???";
            }
        }

        public Rom(string filename)
        {
            Console.WriteLine("Loading ROM: {0}", filename);

            using (BinaryReader r = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read)))
            {
                byte sig1, sig2, sig3, sig4;
                sig1 = r.ReadByte();
                sig2 = r.ReadByte();
                sig3 = r.ReadByte();
                sig4 = r.ReadByte();

                if (sig1 != 'N' ||
                    sig2 != 'E' ||
                    sig3 != 'S' ||
                    sig4 != 0x1A)
                    throw new RomFormatException("Invalid ROM signature");

                byte prgRomBanks = r.ReadByte();
                byte vromBanks = r.ReadByte();
                byte rcb1 = r.ReadByte();
                byte rcb2 = r.ReadByte();
                int mapper = (rcb1 >> 4) | (rcb2 & 0xF0);
                byte ramBanks = r.ReadByte();

                // Read reserved bytes
                for(int i=0; i<7; ++i)
                    r.ReadByte();

                Console.WriteLine("16KB PRG-ROM banks: {0}", prgRomBanks);
                Console.WriteLine("8KB CHR-ROM (VROM) banks: {0}", vromBanks);
                Console.WriteLine("ROM Ctrl Byte 1: 0x{0:X2}", rcb1);
                Console.WriteLine("ROM Ctrl Byte 2: 0x{0:X2}", rcb2);
                Console.WriteLine("    Mapper: {0}: {1}", mapper, GetMapperName(mapper));
                Console.WriteLine("    Mirroring: {0}", (rcb1 & 0x01) != 0 ? "Horizontal" : "Vertical");
                Console.WriteLine("    Battery-Backed RAM: {0}", (rcb1 & 0x02) != 0 ? "Yes" : "No");
                Console.WriteLine("    512-Byte Trainer: {0}", (rcb1 & 0x04) != 0 ? "Yes" : "No");
                Console.WriteLine("    4-Screen Mirroring: {0}", (rcb1 & 0x08) != 0 ? "Yes" : "No");
                Console.WriteLine("8KB RAM banks: {0}", ramBanks);

                PrgRomBanks = new byte[prgRomBanks][];
                ChrRomBanks = new byte[vromBanks][];
                RamBanks = new byte[ramBanks][];

                for (int i = 0; i < prgRomBanks; ++i)
                    PrgRomBanks[i] = r.ReadBytes(16 * 1024);

                for (int i = 0; i < vromBanks; ++i)
                    ChrRomBanks[i] = r.ReadBytes(8 * 1024);

                for (int i = 0; i < ramBanks; ++i)
                    RamBanks[i] = r.ReadBytes(8 * 1024);

                Console.WriteLine("Loaded all banks.");

                //Console.WriteLine("Position = {0}",r.BaseStream.Position);
            }
        }
    }
}
