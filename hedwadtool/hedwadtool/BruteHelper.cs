using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hedwadtool
{
    class BruteHelper
    {

        string prefix;
        string suffix;
        string checksum;

        string test;

        uint checksumuint = 0;

        public BruteHelper(string p, string s, string c)
        {
            prefix = p;
            suffix = s;
            checksum = c;

            target.Clear();
            target.Add(0);

            len1 = lets.Length;

            if (!Checksum.TryParseHex(checksum, out checksumuint))
            {
                checksumuint = 0;
            }
        }


        string lets = "_0123456789abcdefghijklmnopqrstuvwxyz";
        int len1;

        List<byte> target = new List<byte>();
        bool stop = false;

        StringBuilder sb = new StringBuilder();


        private void TargetToTest()
        {
            foreach (byte c in target)
                test += lets[c];
        }

        public bool ChecksumMatches()
        {
            if (checksumuint == Checksum.Calc(sb.ToString(), false))
                return true;

            return false;
        }

        public uint GetChecksum()
        {
            return checksumuint;
        }

        public string GetText()
        {
            return sb.ToString();
        }


        public void Next()
        {
            test = "";

            TargetToTest();

            sb.Clear();
            sb.Append(prefix); 
            sb.Append(test); 
            sb.Append(suffix);

            Increase();
        }

        private void Increase()
        {
            for (int i = 0; i < target.Count; i++)
            {
                try
                {
                    if (target[i] < len1 - 1)
                    {
                        target[i] += 1;
                        return;
                    }
                    else
                    {
                        target[i] = 0;
                        if (i == target.Count - 1)
                        {
                            target.Add(0);
                            return;
                        }
                    }
                }
                catch
                {
                    target.Add(0);
                }
            }
        }


    }
}
