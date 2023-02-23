using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.Disk {
    internal abstract class DiskDriver {
        public int Size { get; protected set; }

        public abstract byte ReadByte(uint offset);
        public abstract bool SetByte(uint offset, byte val);

        public virtual byte[] ReadMany(uint offset, int size) {
            byte[] tmp = new byte[size];

            for(uint i = 0; i < size; i++) {
                tmp[i] = ReadByte(offset + i);
            }

            return tmp;
        }

        public virtual bool SetMany(uint offset, byte[] values) {
            for(uint i = 0; i < values.Length; i++) {
                if (!SetByte(offset + i, values[i])) return false;
            }

            return true;
        }
    }
}
