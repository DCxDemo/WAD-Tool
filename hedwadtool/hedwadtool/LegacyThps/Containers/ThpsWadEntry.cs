using System.IO;
using System;

namespace LegacyThps.Containers
{
    class ThpsWadEntry
    {
        public string name;
        public uint checksum;
        public int size;
        public int offset;

        public byte[] Data;

        public int SizePadded
        {
            get
            {
                int val = size;
                while (val % 2048 != 0) val++;
                return val;
            }
        }

        public ThpsWadEntry()
        {
        }

        public ThpsWadEntry(BinaryReader br)
        {
            checksum = br.ReadUInt32();

            if (checksum != 0)
            {
                offset = br.ReadInt32();
                size = br.ReadInt32();
                name = ThpsWad.GetName(checksum);
            }
        }

        public ThpsWadEntry(string filename)
        {
            name = Path.GetFileName(filename);
            checksum = Checksum.CalcLegacy(name, false);
            Data = File.ReadAllBytes(filename);
            size = Data.Length;
            offset = 0;
        }

        public ThpsWadEntry(string n, int off, int s )
        {
            name = n;
            size = s;
            offset = off;
            checksum = Checksum.CalcLegacy(name, false);
        }

        public void Save(string path)
        {
            File.WriteAllBytes(Path.Combine(path, name), Data);
        }

        public void WriteWadHashed(BinaryWriter bw)
        {
            bw.Write(checksum);
            bw.Write(offset);
            bw.Write(size);
        }

        public void WriteWadRaw(BinaryWriter bw)
        {
            bw.Write(name.ToCharArray());

            int diff = size % 2048;
            int diff2 = name.Length % 4;
            bw.Write(new byte[(diff == 0) ? 4 : 4 - diff2]);

            bw.Write(offset);
            bw.Write(size);
        }

        public ThpsWadEntry(uint c, int off, int s)
        {
            checksum = c;
            size = s;
            offset = off;
            name = ThpsWad.GetName(checksum);
        }

        public override string ToString()
        {
            return name + " (" + size + " bytes) at 0x" + offset;
        }
    }
}
