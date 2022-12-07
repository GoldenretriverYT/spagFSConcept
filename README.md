# spagFSConcept
spagFS is a PoC implementation for the limited, but simple, spagFS

### Is there any specification?
There is no seperate specification, as the specifications are currently given through the PoC implementation itself. If you implement it similiarly, it should work.

### Is there any future for this FS?
Absolutely not. I will probably abandone it within a few weeks, lol. Also it has way too few features and too many limitations to be actually used seriously.

### Hidden commands in test CLI
5 - Dumps Sector Information

### Where is the virtual disk saved?
The CLI currently doesn't save your data anywhere except your RAM, which is lost after closing the CLI.

### Current limits
 - 4096 files
 - 251 char file name length
 - 509 bytes/sector of data
 - 32309248 bytes / 32,3mb **THEORETICAL** data limit (not tested!) (calculated in the following way: (SectorDataSize * 2^16) - (FileTableSize))
 - Probably glitchy
 - File deletion not added yet
 - Folders do not really exist
 - File size is not stored in the table and must be manually calculated by going through all of the files sectors
 - No permission control
