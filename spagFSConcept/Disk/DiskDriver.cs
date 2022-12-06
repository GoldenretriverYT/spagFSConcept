using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.Disk {
    internal abstract class DiskDriver {
        public int Size { get; protected set; }

        public abstract byte ReadByte(int offset);
        public abstract bool SetByte(int offset, byte val);

        public virtual byte[] ReadMany(int offset, int size) {
            byte[] tmp = new byte[size];

            for(var i = 0; i < size; i++) {
                tmp[i] = ReadByte(offset + i);
            }

            return tmp;
        }

        public virtual bool SetMany(int offset, byte[] values) {
            for(var i = 0; i < values.Length; i++) {
                if (!SetByte(offset + i, values[i])) return false;
            }

            return true;
        }
    }
}
