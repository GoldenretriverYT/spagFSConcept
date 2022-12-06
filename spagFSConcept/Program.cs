using spagFSConcept.Disk;

namespace spagFSConcept {
    internal class Program {
        static void Main(string[] args) {
            RAMDisk ramDisk = new(1024 * 1024 * 256);
        }
    }
}