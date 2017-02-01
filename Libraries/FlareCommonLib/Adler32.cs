// Adler32.cs - Tom Weatherhead - September 11, 2009

using System;
//using System.Collections.Generic;
//using System.Text;

namespace SolarFlareCommon
{
    public static class Adler32
    {
        public static UInt32 ComputeChecksum(byte[] srcData)
        {
            UInt32 a = 1;
            UInt32 b = 0;

            foreach (byte srcByte in srcData)
            {
                a += (UInt32)srcByte;

                if (a >= 65521)
                {
                    a -= 65521;
                }

                b += a;

                if (b >= 65521)
                {
                    b -= 65521;
                }
            }

            return b * 65536 + a;
        }
    }
}
