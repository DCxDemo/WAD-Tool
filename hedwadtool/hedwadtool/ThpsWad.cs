using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace hedwadtool
{
    class ThpsWad
    {
        public enum ArchiveType
        {
            WadRaw,
            WadHashed,
            Pre
        }

        public ArchiveType archiveType = ArchiveType.WadRaw;

        public int WadSize
        {
            get
            {
                int size = 0;

                foreach (ThpsWadEntry w in Entries)
                    size += w.SizePadded;

                return size;
            }
        }


        public static Dictionary<uint, string> checksums = new Dictionary<uint, string>();

        public static void InitDictionary(string filename = "filenames.txt")
        {
            checksums.Clear();

            if (File.Exists(filename))
            {
                string[] lines = File.ReadAllLines(filename);

                foreach (string s in lines)
                {
                    string temp = s.Trim().ToLower();

                    if (temp != "")
                        if (!temp.Contains("#"))
                        {
                            uint crc = Checksum.Calc(temp, true);
                            if (!checksums.ContainsKey(crc))
                                checksums.Add(crc, temp);
                        }
                
                }

            }
        }

        public static string GetName(uint checksum)
        {
            if (checksums.ContainsKey(checksum))
                return checksums[checksum];

            Console.WriteLine(checksum.ToString("X8") + " not found");

            return $"_{checksum.ToString("X8")}_.bmp";
        }



        List<ThpsWadEntry> Entries = new List<ThpsWadEntry>();

        public ThpsWad(BinaryReader hed, BinaryReader wad)
        {
            Read(hed, wad);
        }

        public static ThpsWad FromReader(BinaryReader hed, BinaryReader wad)
        {
            return new ThpsWad(hed, wad);
        }

        public static ThpsWad FromFile(string file)
        {
            return ThpsWad.FromFile(Path.ChangeExtension(file, ".hed"), Path.ChangeExtension(file, ".wad"));
        }

        public static ThpsWad FromFile(string hedFile, string wadFile)
        {
            if (!File.Exists(hedFile))
                throw new Exception($"Not found: {hedFile}");

            if (!File.Exists(wadFile))
                throw new Exception($"Not found: {wadFile}");

            using (BinaryReader hed = new BinaryReader(File.OpenRead(hedFile)))
            {
                using (BinaryReader wad = new BinaryReader(File.OpenRead(wadFile)))
                {
                    return ThpsWad.FromReader(hed, wad);
                }
            }
        }

        public static ThpsWad FromList(string filename)
        {
            ThpsWad wad = new ThpsWad();

            if (!File.Exists(filename))
                throw new Exception($"Not found: {filename}");

            wad.Entries.Clear();

            string[] lines = File.ReadAllLines(filename);

            string path = Path.ChangeExtension(filename, "");

            foreach (string s in lines)
            {
                if (s != "")
                {
                    ThpsWadEntry en = new ThpsWadEntry(Path.Combine(path, s.Trim()));
                    wad.Entries.Add(en);
                }
            }

            wad.RecalcOffsets();

            return wad;
        }

        public static ThpsWad FromFolder(string path)
        {
            if (!Directory.Exists(path))
                throw new Exception($"Not found: {path}");

            string[] lines = Directory.GetFiles(path);

            ThpsWad wad = new ThpsWad();

            foreach (string s in lines)
            {
                if (s != "")
                {
                    ThpsWadEntry en = new ThpsWadEntry(s);
                    wad.Entries.Add(en);
                }
            }

            wad.RecalcOffsets();

            return wad;
        }

        public void RecalcOffsets()
        {
            int offset = 0;

            foreach (ThpsWadEntry w in Entries)
            {
                w.offset = offset;
                offset += w.SizePadded;
            }
        }

        public void Read(BinaryReader hed, BinaryReader wad)
        {
            if (hed.BaseStream.Length % 12 != 0)
                if ((hed.BaseStream.Length - 4) % 12 != 0)
                    throw new Exception("not a THPS2 WAD");

            do
            {
                ThpsWadEntry en = new ThpsWadEntry(hed);
                wad.BaseStream.Position = en.offset;
                en.Data = wad.ReadBytes(en.size);

                Entries.Add(en);
            }
            while (hed.BaseStream.Length - hed.BaseStream.Position >= 12 );
        }

        public void Extract(string path)
        {
            StringBuilder sb = new StringBuilder();

            foreach (ThpsWadEntry en in Entries)
            {
                en.Save(path);
                sb.AppendLine(en.name);
            }

            File.WriteAllText(path + ".txt", sb.ToString());
        }


        public void Write(string filename)
        {
            //make sure the offsets are ok
            RecalcOffsets();

            if (archiveType != ArchiveType.Pre)
            {

                if (archiveType == ArchiveType.WadHashed)
                {
                    using (BinaryWriter hed = new BinaryWriter(File.OpenWrite(Path.ChangeExtension(filename, ".hed"))))
                    {
                        foreach (ThpsWadEntry w in Entries)
                            w.WriteWadHashed(hed);
                    }
                }

                if (archiveType == ArchiveType.WadRaw)
                {
                    throw new NotImplementedException("can't write raw WAD yet");
                }

                using (BinaryWriter wad = new BinaryWriter(File.OpenWrite(Path.ChangeExtension(filename, ".wad"))))
                {
                    foreach (ThpsWadEntry w in Entries)
                    {
                        wad.BaseStream.Position = w.offset;
                        wad.Write(w.Data);
                    }

                    wad.BaseStream.Position = WadSize - 1;
                    wad.Write((byte)0);
                }
            }
            else
            {
                throw new NotImplementedException("can't write PRE yet");
            }
        }


        bool isPRE = false;

        public string WADname = "CD";

        public List<ThpsWadEntry> files = new List<ThpsWadEntry>();

        //MemoryStream ms;
        //BinaryReader br;

        public ThpsWad()
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

                    ThpsWadEntry ff = new ThpsWadEntry(lol[i], 0, data.Length);

                    ff.Data = data;

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
            checksums.Clear();
            files.Clear();

            InitDictionary();
        }
        
        /*
        public ThpsWad(string fn)
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
        */

        ~ThpsWad()
        {
            //if (br!=null) br.Close();
            //if (ms != null) ms.Close();
            //br = null;
            //ms = null;
        }

        /*
        private bool testTH1formatFast()
        {
            br.BaseStream.Position = 0;
            byte[] test = br.ReadBytes(4);

            foreach (byte b in test) 
                if (!isValidChar(b)) 
                    return false;

            return true;
        }
        */

        private bool isValidChar(byte x)
        {
            if (x >= 0x2E & (char)x < 'z') return true;
            return false;
        }

        /*
        private void ParseTH1HED()
        {
            br.BaseStream.Position = 0;

            do files.Add(new ThpsWadEntry(ReadNTString(br), br.ReadInt32(), br.ReadInt32()));
            while (br.BaseStream.Position < br.BaseStream.Length - 1);
        }
        */
        /*
        private void ParseTH2HED()
        {
            br.BaseStream.Position = 0;

            do files.Add(new ThpsWadEntry(br.ReadUInt32(), br.ReadInt32(), br.ReadInt32()));
            while (br.BaseStream.Position < br.BaseStream.Length - 4);
        }
        */

        /*
        public void ParsePre(string fn)
        {
            ms = new MemoryStream(File.ReadAllBytes(fn));
            br = new BinaryReader(ms);

            br.BaseStream.Position = 0;
            int fileCount = br.ReadInt32();

            for (int i = 0; i < fileCount; i++)
            {
                ThpsWadEntry f = new ThpsWadEntry(0, 0, 0);
                f.name = ReadNTString(br);
                f.size = br.ReadInt32();
                f.Data = br.ReadBytes(f.size);

                if (f.size % 4 > 0) br.BaseStream.Position += 4 - f.size % 4;

                files.Add(f);
            }

            Directory.CreateDirectory(".\\" + Path.GetFileNameWithoutExtension(fn) + "\\");

            foreach (ThpsWadEntry h in files)
            {
                File.WriteAllBytes(".\\" + Path.GetFileNameWithoutExtension(fn) + "\\" + h.name, h.Data);
            }

            isPRE = true;
        }

        */

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

            foreach (ThpsWadEntry h in files)
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
            string hed1file = fn.ToLower().Replace("__layout.txt", ".hed1");
            string wadfile = fn.ToLower().Replace("__layout.txt", ".wad");

            BinaryWriter wad = new BinaryWriter(File.Open(wadfile, FileMode.Create));
            BinaryWriter hed = new BinaryWriter(File.Open(hedfile, FileMode.Create));
            BinaryWriter hed1 = new BinaryWriter(File.Open(hed1file, FileMode.Create));

            foreach (ThpsWadEntry h in files)
            {
                h.offset = (int)wad.BaseStream.Position;
                
                wad.Write(h.Data);

                int diff = h.size % 2048;
                wad.Write(new byte[(diff == 0) ? 2048 : 2048 - diff]);

                hed.Write(h.checksum);
                hed.Write(h.offset);
                hed.Write(h.size);


                hed1.Write(h.name.ToCharArray());

                int diff2 = h.name.Length % 4;
                hed1.Write(new byte[(diff == 0) ? 4 : 4 - diff2]);

                hed1.Write(h.offset);
                hed1.Write(h.size);
            }

            hed1.Write((byte)0xFF);

            wad.Close();
            hed.Close();
            hed1.Close();
        }

    }
}
