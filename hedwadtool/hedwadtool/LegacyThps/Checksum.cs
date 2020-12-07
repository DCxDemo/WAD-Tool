using System;

namespace LegacyThps
{
    class Checksum
    {
        public static uint result = 0xFFFFFFFF;
        static int c = 0;
        static int i = 0;

        public static uint CalcLegacy(string k, bool allowCaps)
        {
            if (!allowCaps)
                k = k.ToLower();

            result = 0xFFFFFFFF;

            for (c = 0; c < k.Length; c++)
            {
                uint temp = result ^ k[c];

                for (i = 0; i < 8; i++)
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

            if (hex == "")
                return false;

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
