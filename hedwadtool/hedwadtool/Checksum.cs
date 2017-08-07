using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hedwadtool
{
    class Checksum
    {

        public static uint Calc(string k, bool allowCaps)
        {
            string str = allowCaps ? k : k.ToLower();

            uint result = 0xFFFFFFFF;

            foreach (char c in str)
            {
                uint temp = result ^ c;

                for (int i = 0; i < 8; i++)
                {
                    result = (result >> 31) | 2 * result;
                    if ((temp & 1) != 0) result ^= 0xEDB88320;
                    temp >>= 1;
                }
            }

            return result;
        }


        public static bool TryParseHex(string hex, out UInt32 result)
        {
            result = 0;

            if (hex == null) return false;

            try
            {
                result = Convert.ToUInt32(hex, 16);
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }
    }
}
