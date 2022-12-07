using spagFSConcept.Disk;
using spagFSConcept.FileSystem;
using System.Text;

namespace spagFSConcept {
    internal class Program {
        static void Main(string[] args) {
            RAMDisk ramDisk = new(1024 * 1024 * 32);
            FSContext ctx = new(ramDisk);

            while(true) {
                Console.WriteLine("==========================");
                Console.WriteLine("Choose option:");
                Console.WriteLine("1) Write File");
                Console.WriteLine("2) Read File");
                Console.WriteLine("3) List Files");
                Console.WriteLine("4) Dump File Information");

                string selStr = Console.ReadLine();
                if (!int.TryParse(selStr, out int sel)) continue;

                if(sel == 1) {
                    Console.WriteLine("Path:");
                    string path = Console.ReadLine();
                    Console.WriteLine("Content:");
                    string content = Console.ReadLine();

                    bool success = ctx.WriteFile(path, Encoding.ASCII.GetBytes(content));

                    if(!success) {
                        Console.WriteLine("Write Failed: " + ctx.LastError);
                    }
                }else if(sel == 2) {
                    Console.WriteLine("Path:");
                    string path = Console.ReadLine();

                    byte[] data = ctx.ReadFile(path);

                    if (ctx.LastError == FSError.FILE_NOT_FOUND) {
                        Console.WriteLine("FILE NOT FOUND!!!");
                    } else {
                        Console.WriteLine(Encoding.ASCII.GetString(data));
                    }
                }else if(sel == 3) {
                    Console.WriteLine("BasePath:");
                    string path = Console.ReadLine();

                    FileTableEntry[] files = ctx.GetFiles(path);

                    foreach(var fte in files) {
                        Console.WriteLine("- " + fte.FileName);
                    }
                }else if(sel == 4) {
                    Console.WriteLine("Path:");
                    string path = Console.ReadLine();

                    FileTableEntry fte = ctx.GetFile(path).Value;

                    Console.WriteLine(@$"Flags: {fte.Flag}
FileName: {fte.FileName}
SectorId: {fte.SectorId}
FileTableEntryId: {fte.FileTableEntryId}");
                } else if (sel == 5) {
                    Console.WriteLine("SectorId:");
                    string secStr = Console.ReadLine();
                    if (!ushort.TryParse(secStr, out ushort sec)) continue;

                    Sector sector = ctx.GetSector(sec);

                    Console.WriteLine(@$"Exists: {sector.Exists}
Data: {sector.Data}
NextSectorId: {sector.NextSectorId}");
                }
            }
        }
    }
}