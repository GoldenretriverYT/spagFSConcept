using spagFSConcept.Disk;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace spagFSConcept.FileSystem {
    internal class FSContext {
        public const int TABLE_SIZE = 1024 * 1024;
        public const int TABLE_ENTRY_SIZE = 256;

        public DiskDriver Disk { get; set; }

        private int entriesInTable => TABLE_SIZE / TABLE_ENTRY_SIZE;
        private uint sectorCount => (Disk.Size / 512 > uint.MaxValue ? uint.MaxValue : (uint)(Disk.Size / 512));
        public FSError LastError = (FSError)0;

        public FSContext(DiskDriver disk) {
            if(disk.Size < TABLE_SIZE) {
                throw new Exception($"Disk must be atleast {TABLE_SIZE} bytes in order to be useable with spagFS");
            }

            this.Disk = disk;

            // As sector 0 is used for error returning, we reserve it

            disk.SetByte(GetOffset(0), 0xFF);
        }

        // file table
        // Every 256 bytes represents a file (which means you can have up to 4096 files @ 1mb table)

        // file
        // 1 byte: flags
        // 2 bytes: sector id (each sector can store 1 byte (EXISTS BOOL) + 507 bytes data + 4 bytes referencing the next sector (id), if needed
        // 253 bytes: name

        // folders are purely virtual and included in the file name

        public FileTableEntry[] GetFiles(string rootPath) {
            List<FileTableEntry> filePaths = new();

            for(var entryId = 0; entryId < entriesInTable; entryId++) {
                uint entryOffset = (uint)(entryId * TABLE_ENTRY_SIZE);
                FileTableEntry entry = StructHelper.FromBytes<FileTableEntry>(Disk.ReadMany(entryOffset, 256));

                if (entry.Equals(default(FileTableEntry))) continue;

                if (entry.FileName.StartsWith(FixPath(rootPath + "/"))) {
                    filePaths.Add(entry);
                }
            }

            return filePaths.ToArray();
        }

        public FileTableEntry? GetFile(string path) {
            string fixedPath = FixPath(path);

            for (var entryId = 0; entryId < entriesInTable; entryId++) {
                uint entryOffset = (uint)(entryId * TABLE_ENTRY_SIZE);
                FileTableEntry entry = StructHelper.FromBytes<FileTableEntry>(Disk.ReadMany(entryOffset, 256));

                if (entry.Equals(default(FileTableEntry))) return null;

                if (entry.FileName.StartsWith(fixedPath)) {
                    return entry;
                }
            }

            return null;
        }

        public bool WriteFile(string path, byte[] content) {
            var tmpFile = GetFile(path);

            if (tmpFile == null) {
                tmpFile = CreateFile(path);

                if (tmpFile == null)
                    return false; // Its null again, something must have went wrong
            }

            var file = tmpFile.Value;

            // Unreserve old sectors

            if(file.SectorId != 0) {
                RecursiveUnreserve(file.SectorId);
            }

            uint sectorsRequired = (uint)((content.Length / 507)+1);
            uint[] sectors = FindFreeSectors(sectorsRequired, sectorsRequired);

            if(sectors.Length == 0) {
                return false; // LastError is already set by FindFreeSectors
            }

            file.SectorId = sectors[0];

            for(var i = 0; i < sectors.Length; i++) {
                int contentOffset = i * 507;
                uint sectorOffset = GetOffset(sectors[i]);

                Sector sector = new();
                sector.Exists = 0xFF;
                sector.Data = content.Skip(contentOffset).Take(507).ToArray().PadRight(507);
                sector.NextSectorId = (i >= sectors.Length - 1 ? (uint)0 : sectors[i + 1]);

                Disk.SetMany(sectorOffset, StructHelper.GetBytes(sector));
            }

            Disk.SetMany(file.FileTableEntryId * 256, StructHelper.GetBytes(file));

            return true;
        }

        public byte[] ReadFile(string path) {
            var tmpFile = GetFile(path);

            if (tmpFile == null) {
                LastError = FSError.FILE_NOT_FOUND;
                return new byte[0];
            }

            var file = tmpFile.Value;
            uint[] sectors = RecursiveGetAllSectors(file.SectorId);
            List<byte> result = new();

            for(var i = 0; i < sectors.Length; i++) {
                result.AddRange(Disk.ReadMany(GetOffset(sectors[i]) + 1, 507));
            }

            return result.ToArray();
        }

        public FileTableEntry? CreateFile(string path) {
            path = FixPath(path);

            uint availableTableOffset = UInt32.MaxValue;

            for (var entryId = 0; entryId < entriesInTable; entryId++) {
                uint entryOffset = (uint)(entryId * TABLE_ENTRY_SIZE);
                FileTableEntry entry = StructHelper.FromBytes<FileTableEntry>(Disk.ReadMany(entryOffset, 256));

                if(!entry.Flag.HasFlag(FileFlag.Exists)) {
                    availableTableOffset = entryOffset;
                    break;
                } 
            }

            if (availableTableOffset == UInt32.MaxValue) {
                LastError = FSError.FILE_TABLE_FULL;
                return null;
            }

            FileTableEntry fte = new();

            fte.FileName = path;
            fte.Flag = FileFlag.Exists;
            fte.FileTableEntryId = (uint)(availableTableOffset / 256);
            fte.SectorId = 0;

            Disk.SetMany(availableTableOffset, StructHelper.GetBytes(fte));

            return fte;
        }

        private uint FindFreeSector(uint sectorId = 0) {
            for(uint i = sectorId; i < sectorCount; i++) {
                uint sectorOffset = GetOffset(sectorId);

                if(Disk.ReadByte(sectorOffset) == 0x00) {
                    return i;
                }
            }

            return 0;
        }

        public Sector GetSector(uint sectorId) {
            return StructHelper.FromBytes<Sector>(Disk.ReadMany(GetOffset(sectorId), 256));
        }

        private uint[] FindFreeSectors(uint requiredSectors, uint sectorId = 0) {
            uint[] freeSectors = new uint[requiredSectors];
            int arrIdx = 0;

            for (uint i = sectorId; i < sectorCount; i++) {
                uint sectorOffset = GetOffset(sectorId);

                if (Disk.ReadByte(sectorOffset) == 0x00) {
                    freeSectors[arrIdx] = i;
                    arrIdx++;

                    if (arrIdx == freeSectors.Length) break;
                }
            }

            if(arrIdx != freeSectors.Length) {
                LastError = FSError.NO_FREE_SECTORS;
                return Array.Empty<uint>();
            }

            return freeSectors;
        }

        private void RecursiveUnreserve(uint sectorId) {
            uint offset = GetOffset(sectorId);

            if(Disk.ReadByte(offset) == 0xFF) {
                Disk.SetByte(offset, 0x00);

                uint nextSectorId = BitConverter.ToUInt16(Disk.ReadMany(offset + 510, 2), 0);

                if (nextSectorId != 0x0000) {
                    RecursiveUnreserve(nextSectorId);
                }
            }
        }

        private uint[] RecursiveGetAllSectors(uint sectorId, List<uint> _secs = null) {
            var secs = _secs ?? new List<uint>();

            uint offset = GetOffset(sectorId);

            if (Disk.ReadByte(offset) == 0xFF) {
                secs.Add(sectorId);

                uint nextSectorId = BitConverter.ToUInt16(Disk.ReadMany(offset + 510, 2), 0);

                if (nextSectorId != 0x0000) {
                    return RecursiveGetAllSectors(nextSectorId, secs);
                }else {
                    return secs.ToArray();
                }
            }

            return Array.Empty<uint>();
        }

        private uint GetOffset(uint sectorId) => TABLE_SIZE + (sectorId * 512);

        private string FixPath(string inpStr) {
            return ("/" + inpStr).Replace("\\", "/").Replace("//", "/");
        }
    }
}
