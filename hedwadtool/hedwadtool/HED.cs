using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace hedwadtool
{
    class HED
    {
        bool isPRE = false;

        public string WADname = "CD";

        public List<HEDFile> files = new List<HEDFile>();
        MemoryStream ms;
        BinaryReader br;

        public static List<string> filenames = new List<string>();
        public static List<uint> checksums = new List<uint>();

        public HED()
        {
            Reset();
        }

        public void LoadFromLayout(string f)
        {
            string[] lol = File.ReadAllLines(f);

            string rootpath = Path.GetDirectoryName(f);
            string wadfold = Path.GetFileNameWithoutExtension(f).Replace("__layout", "");
            WADname = wadfold;

            for (int i = 0; i < lol.Count(); i++)
            {       
                string path = rootpath + "\\" + wadfold + "\\" + lol[i];

                if (File.Exists(path))
                {
                    byte[] data = File.ReadAllBytes(path);

                    HEDFile ff = new HEDFile(lol[i], 0, data.Length);

                    ff.SetData(data);

                    if (lol[i][0] == '_' && lol[i][lol[i].Length - 1] == '_')
                    {
                        string nname = lol[i].Trim('_');
                        uint hex = 0;

                        Checksum.TryParseHex(nname, out hex);

                        ff.checksum = hex;
                    }
                    else
                    {
                        ff.checksum = Checksum.Calc(lol[i], false);
                    }

                    files.Add(ff);
                }
            }
        }

        public void Reset()
        {
            filenames.Clear();
            checksums.Clear();
            files.Clear();

            filenames = File.ReadAllLines("filenames.txt").ToList();
            foreach (string s in filenames) checksums.Add(Checksum.Calc(s, false));
        }

        public HED(string fn)
        {
            WADname = Path.GetFileNameWithoutExtension(fn);

            Reset();

            ms = new MemoryStream(File.ReadAllBytes(fn));
            br = new BinaryReader(ms);

            if (testTH1formatFast())
            {
                ParseTH1HED();
            }
            else
            {
                ParseTH2HED();
            }
        }

        ~HED()
        {
            if (br!=null) br.Close();
            if (ms != null) ms.Close();
            br = null;
            ms = null;
        }

        private bool testTH1formatFast()
        {
            br.BaseStream.Position = 0;
            byte[] test = br.ReadBytes(4);

            foreach (byte b in test) 
                if (!isValidChar(b)) 
                    return false;

            return true;
        }

        private bool isValidChar(byte x)
        {
            if ((char)x > '0' & (char)x < 'z') return true;
            return false;
        }

        private void ParseTH1HED()
        {
            br.BaseStream.Position = 0;

            do files.Add(new HEDFile(ReadNTString(br), br.ReadInt32(), br.ReadInt32()));
            while (br.BaseStream.Position < br.BaseStream.Length - 1);
        }

        private void ParseTH2HED()
        {
            br.BaseStream.Position = 0;

            do files.Add(new HEDFile(br.ReadUInt32(), br.ReadInt32(), br.ReadInt32()));
            while (br.BaseStream.Position < br.BaseStream.Length - 4);
        }


        public void ParsePre(string fn)
        {
            ms = new MemoryStream(File.ReadAllBytes(fn));
            br = new BinaryReader(ms);

            br.BaseStream.Position = 0;
            int fileCount = br.ReadInt32();

            for (int i = 0; i < fileCount; i++)
            {
                HEDFile f = new HEDFile(0, 0, 0);
                f.name = ReadNTString(br);
                f.size = br.ReadInt32();
                f.SetData(br.ReadBytes(f.size));

                if (f.size % 4 > 0) br.BaseStream.Position += 4 - f.size % 4;

                files.Add(f);
            }

            Directory.CreateDirectory(".\\" + Path.GetFileNameWithoutExtension(fn) + "\\");

            foreach (HEDFile h in files)
            {
                File.WriteAllBytes(".\\" + Path.GetFileNameWithoutExtension(fn) + "\\" + h.name, h.data);
            }

            isPRE = true;
        }



        private string ReadNTString(BinaryReader br)
        {
            List<byte> x = new List<byte>();

            do x.AddRange(br.ReadBytes(4).ToList());
            while (!x.Contains(0));

            x.RemoveAll(item => item.Equals(0));

            return System.Text.Encoding.ASCII.GetString(x.ToArray());
        }

        public void ExtractWAD(string s)
        {
            MemoryStream ms = new MemoryStream(File.ReadAllBytes(s));
            BinaryReader br = new BinaryReader(ms);

            string rootpath = Path.GetDirectoryName(s) + "\\";
            string wadpath = rootpath + WADname + "\\";
            string layout = rootpath + WADname+"__layout.txt";

            if (!Directory.Exists(wadpath)) Directory.CreateDirectory(wadpath);

            if (File.Exists(layout)) File.Delete(layout);
            
            StringBuilder sb = new StringBuilder();

            foreach (HEDFile h in files)
            {
                br.BaseStream.Position = h.offset;
                byte[] x = br.ReadBytes(h.size);
                File.WriteAllBytes(wadpath + h.name, x);

                sb.Append(h.name + "\r\n");               
            }

            File.AppendAllText(layout, sb.ToString());
        }



        public void BuildWAD(string fn)
        {
            string hedfile = fn.ToLower().Replace("__layout.txt", ".hed");
            string wadfile = fn.ToLower().Replace("__layout.txt", ".wad");

            BinaryWriter wad = new BinaryWriter(File.Open(wadfile, FileMode.Create));
            BinaryWriter hed = new BinaryWriter(File.Open(hedfile, FileMode.Create));

            foreach (HEDFile h in files)
            {
                h.offset = (int)wad.BaseStream.Position;
                
                wad.Write(h.data);

                int diff = h.size % 2048;
                wad.Write(new byte[(diff == 0) ? 2048 : 2048 - diff]);

                hed.Write(h.checksum);
                hed.Write(h.offset);
                hed.Write(h.size);
            }

            wad.Close();
            hed.Close();
        }

    }
}
