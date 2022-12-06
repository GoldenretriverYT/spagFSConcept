using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.FileSystem {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct FileTableEntry {
        public FileFlag Flag;
        public int SectorOffset;

        [MarshalAs(UnmanagedType.LPStr, SizeConst = 251)]
        public string FileName;
    }
}
