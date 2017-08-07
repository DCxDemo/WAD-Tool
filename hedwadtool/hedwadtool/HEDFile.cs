using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hedwadtool
{
    class HEDFile
    {
        public string name;
        public uint checksum;
        public int size;
        public int offset;

        public byte[] data;

        public void SetData(byte[] x)
        {
            data = x;
        }

        public HEDFile(string n, int off, int s )
        {
            name = n;
            size = s;
            offset = off;
            checksum = Checksum.Calc(name, false);
        }

        public HEDFile(uint c, int off, int s)
        {
            checksum = c;
            size = s;
            offset = off;

            if (HED.checksums.Contains(checksum))
            {
                int index = HED.checksums.FindIndex(item => item.Equals(checksum));
                name = HED.filenames[index].ToLower();
            }
            else
            {
                name = "_" + c.ToString("X8") + "_";
            }
        }

        public override string ToString()
        {
            return name + " (" + size + " bytes) at 0x" + offset;
        }


    }
}
