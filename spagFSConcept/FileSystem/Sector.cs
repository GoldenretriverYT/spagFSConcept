using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.FileSystem {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Sector {
        public byte Exists; // 0xFF = Exists, 0x00 = Does not exist, 0x01-0xFD = Corrupted
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 509)]
        public byte[] Data;

        public ushort NextSectorId;
    }
}
