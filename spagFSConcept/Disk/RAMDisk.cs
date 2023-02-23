using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.Disk {
    internal class RAMDisk : DiskDriver {
        public RAMDisk(int size) {
            Size = size;
            Data = new byte[size];
        }

        public byte[] Data { get; }

        public override byte ReadByte(uint offset) {
            return Data[offset];
        }

        public override bool SetByte(uint offset, byte val) {
            Data[offset] = val;
            return true;
        }
    }
}
