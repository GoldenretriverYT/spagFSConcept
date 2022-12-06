using spagFSConcept.Disk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.FileSystem {
    internal class FSContext {
        public const int TABLE_SIZE = 1024 * 1024;
        public const int TABLE_ENTRY_SIZE = 256;

        public DiskDriver Disk { get; set; }

        private int entriesInTable => TABLE_SIZE / TABLE_ENTRY_SIZE;

        public FSContext(DiskDriver disk) {
            if(disk.Size < TABLE_SIZE) {
                throw new Exception($"Disk must be atleast {TABLE_SIZE} bytes in order to be useable with spagFS");
            }

            this.Disk = disk;
        }

        // file table
        // Every 256 bytes represents a file (which means you can have up to 4096 files @ 1mb table)

        // file
        // 1 byte: flags
        // 4 bytes: sector offset (each sector can store 508 bytes + 4 bytes referencing the next sector, if needed
        // 251 bytes: name

        // folders are purely virtual and included in the file name

        public FileTableEntry[] GetFiles(string rootPath) {
            List<FileTableEntry> filePaths = new();

            for(var entryId = 0; entryId < entriesInTable; entryId++) {
                int entryOffset = entryId * TABLE_ENTRY_SIZE;
                FileTableEntry entry = StructHelper.FromBytes<FileTableEntry>(Disk.ReadMany(entryOffset, 256));

                if(entry.FileName.StartsWith(FixPath(rootPath + "/"))) {
                    filePaths.Add(entry);
                }
            }

            return filePaths.ToArray();
        }

        public FileTableEntry? GetFile(string path) {
            string fixedPath = FixPath(path);

            for (var entryId = 0; entryId < entriesInTable; entryId++) {
                int entryOffset = entryId * TABLE_ENTRY_SIZE;
                FileTableEntry entry = StructHelper.FromBytes<FileTableEntry>(Disk.ReadMany(entryOffset, 256));

                if (entry.FileName.StartsWith(fixedPath)) {
                    return entry;
                }
            }

            return null;
        }

        private string FixPath(string inpStr) {
            return inpStr.Replace("\\", "/").Replace("//", "/");
        }
    }
}
