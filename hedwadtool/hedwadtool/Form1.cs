using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace hedwadtool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        BruteHelper brute;
        bool stop = false;

        private void bruteButtonClick(object sender, EventArgs e)
        {
            stop = false;

            brute = new BruteHelper(prefixBox.Text, postfixBox.Text, checksumBox.Text);

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

                Application.DoEvents();
            }

            bruteBox.Text = brute.GetText();
        }


        HED hed;

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

        private void groupBox3_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData("FileDrop", false);

            if (s[0].ToLower().Contains(".hed"))
            {
                hed = new HED(s[0]);

                StringBuilder sb = new StringBuilder();

                foreach (HEDFile ss in hed.files)
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
                    hed = new HED();
                    hed.LoadFromLayout(s[0]);

                    hed.BuildWAD(s[0]);
                }
                else
                {
                    if (s[0].ToLower().Contains(".pre"))
                    {
                        hed = new HED();
                        hed.ParsePre(s[0]);

                        StringBuilder sb = new StringBuilder();
                        foreach (HEDFile ss in hed.files)
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            groupBox3.AllowDrop = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            stop = true;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }


    }
}