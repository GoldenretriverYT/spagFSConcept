using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.FileSystem {
    [Flags]
    internal enum SectorFlag : byte {
        EXISTS = 0b1000_0000
    }
}
