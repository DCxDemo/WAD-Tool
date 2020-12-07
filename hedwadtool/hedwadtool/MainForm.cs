using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using LegacyThps;
using LegacyThps.Containers;

namespace hedwadtool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            ThpsWad.InitDictionary();
        }

        BruteHelper brute;
        bool stop = false;

        private void bruteButtonClick(object sender, EventArgs e)
        {
            stop = false;

            brute = new BruteHelper(prefixBox.Text, postfixBox.Text, checksumBox.Text);

            int counter = 0;

            bruteBox.Clear();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (!stop)
            {
                if (brute.ChecksumMatches())
                {
                    bruteBox.Text += $"Found {brute.GetText()} in {sw.Elapsed.TotalSeconds} seconds\r\n";
                }

                brute.Next();

                counter++;

                if (counter > 100000)
                {
                    counter = 0;
                    Application.DoEvents();
                }
            }

            sw.Stop();
        }


        ThpsWad hed;

        private void button2_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = Checksum.CalcLegacy(textBox2.Text, checkBox1.Checked).ToString("X8");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.Text = Checksum.CalcLegacy(textBox2.Text, checkBox1.Checked).ToString("X8");
        }

        private void groupBox3_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }


        private ThpsWad.ArchiveType GetArchiveType()
        {
            if (radioButton1.Checked) return ThpsWad.ArchiveType.WadRaw;
            if (radioButton2.Checked) return ThpsWad.ArchiveType.WadHashed;
            if (radioButton3.Checked) return ThpsWad.ArchiveType.Pre;

            return ThpsWad.ArchiveType.WadRaw;
        }


        private void groupBox3_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData("FileDrop", false);
            ProcessFile(s[0]);
        }


        private void ProcessFile(string filename)
        {
            //detect whether its a directory or file
            if ((File.GetAttributes(filename) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                ThpsWad wad = ThpsWad.FromFolder(filename);
                wad.archiveType = GetArchiveType();
                wad.Write(Path.Combine(Directory.GetParent(filename).FullName, Path.GetFileName(filename) + ".wad"));
            }
            else
            {
                switch (Path.GetExtension(filename).ToLower())
                {
                    case ".hed":
                    case ".wad":
                        {
                            string path = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));

                            ThpsWad wad = ThpsWad.FromFile(filename);
                            wad.Extract(path);

                            break;
                        }

                    case ".txt":
                        {
                            ThpsWad wad = ThpsWad.FromList(filename);
                            wad.archiveType = GetArchiveType();
                            wad.Write(filename);

                            break;
                        }


                    case ".pre":
                        throw new NotImplementedException("Can't read legacy PRE yet.");

                    default:
                        MessageBox.Show("Doesn't look like a supported file.");
                        break;
                }
            }

            //MessageBox.Show("wow");

            /*
            if (filename.ToLower().Contains(".hed"))
            {
                hed = new ThpsWad(filename);

                StringBuilder sb = new StringBuilder();

                foreach (ThpsWadEntry ss in hed.files)
                {
                    // if (ss.name[0] == '_')
                    sb.Append(ss.ToString() + "\r\n");
                }

                checksumBox.Text = sb.ToString();

                hed.ExtractWAD(Path.ChangeExtension(filename, ".wad"));

                MessageBox.Show("I'm here");
            }
            else
            {
                if (filename.ToLower().Contains("__layout"))
                {
                    hed = new ThpsWad();
                    hed.LoadFromLayout(filename);

                    hed.BuildWAD(filename);
                }
                else
                {
                    if (filename.ToLower().Contains(".pre"))
                    {
                        hed = new ThpsWad();
                        hed.ParsePre(filename);

                        StringBuilder sb = new StringBuilder();
                        foreach (ThpsWadEntry ss in hed.files)
                        {
                            // if (ss.name[0] == '_')
                            sb.Append(ss.ToString() + "\r\n");
                        }

                        checksumBox.Text = sb.ToString();
                    }
                    else
                    {
                        checksumBox.Text = Path.GetFileName(filename) + " doesn't look like WAD, PRE, or layout file.";
                    }
                }
            }

            */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            groupBox3.AllowDrop = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            stop = true;
        }
    }
}