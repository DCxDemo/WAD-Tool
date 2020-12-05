using System;
using System.Windows.Forms;
using System.IO;

namespace hedwadtool
{
    public partial class Form1 : Form
    {
        public Form1()
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

            while (!stop)
            {
                if (brute.ChecksumMatches())
                {
                    stop = true;
                    MessageBox.Show("Match found!");        
                }
                else
                {
                    brute.Next();
                }

                counter++;

                if (counter > 10000)
                {
                    counter = 0;
                    Application.DoEvents();
                }
            }

            bruteBox.Text = brute.GetText();
        }


        ThpsWad hed;

        private void button2_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox3.Text = Checksum.Calc(textBox2.Text, checkBox1.Checked).ToString("X8");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.Text = Checksum.Calc(textBox2.Text, checkBox1.Checked).ToString("X8");
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

            //detect whether its a directory or file
            if ((File.GetAttributes(s[0]) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                ThpsWad wad = ThpsWad.FromFolder(s[0]);
                wad.archiveType = GetArchiveType();
                wad.Write(Directory.GetParent(s[0]).FullName + "\\kek.wad");
            }
            else
            {
                switch (Path.GetExtension(s[0]).ToLower())
                {
                    case ".hed":
                    case ".wad":
                        {
                            string path = Path.Combine(Path.GetDirectoryName(s[0]), Path.GetFileNameWithoutExtension(s[0]));

                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            ThpsWad wad = ThpsWad.FromFile(s[0]);
                            wad.Extract(path);

                            break;
                        }

                    case ".txt":
                        {
                            ThpsWad wad = ThpsWad.FromList(s[0]);
                            wad.archiveType = GetArchiveType();
                            wad.Write(s[0]);

                            break;
                        }


                    case ".pre":
                        throw new NotImplementedException();

                    default:
                        MessageBox.Show("Doesn't look like a supported file.");
                        break;
                }
            }

            //MessageBox.Show("wow");

            /*
            if (s[0].ToLower().Contains(".hed"))
            {
                hed = new ThpsWad(s[0]);

                StringBuilder sb = new StringBuilder();

                foreach (ThpsWadEntry ss in hed.files)
                {
                    // if (ss.name[0] == '_')
                    sb.Append(ss.ToString() + "\r\n");
                }

                checksumBox.Text = sb.ToString();

                hed.ExtractWAD(Path.ChangeExtension(s[0], ".wad"));

                MessageBox.Show("I'm here");
            }
            else
            {
                if (s[0].ToLower().Contains("__layout"))
                {
                    hed = new ThpsWad();
                    hed.LoadFromLayout(s[0]);

                    hed.BuildWAD(s[0]);
                }
                else
                {
                    if (s[0].ToLower().Contains(".pre"))
                    {
                        hed = new ThpsWad();
                        hed.ParsePre(s[0]);

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
                        checksumBox.Text = Path.GetFileName(s[0]) + " doesn't look like WAD, PRE, or layout file.";
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