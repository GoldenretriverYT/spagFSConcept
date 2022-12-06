using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.FileSystem {
    [Flags]
    internal enum FileFlag : byte {
        Exists = 0b1000_0000,
        ReadOnly = 0b0100_0000
    }
}
