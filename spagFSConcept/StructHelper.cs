using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept {
    internal class StructHelper {
        internal static T FromBytes<T>(byte[] bytes) {
            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        internal static byte[] GetBytes<T>(T str) {
            int size = Marshal.SizeOf(str);

            byte[] arr = new byte[size];

            GCHandle h = default(GCHandle);

            try {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                Marshal.StructureToPtr<T>(str, h.AddrOfPinnedObject(), false);
            } finally {
                if (h.IsAllocated) {
                    h.Free();
                }
            }

            return arr;
        }
    }
}
