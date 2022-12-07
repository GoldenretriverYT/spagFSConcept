using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.FileSystem {
    internal enum FSError {
        NONE = 0,
        NO_FREE_SECTORS,
        FILE_TABLE_FULL,
        FILE_NOT_FOUND
    }
}
