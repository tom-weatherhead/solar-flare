// CRC32.cs - Tom Weatherhead - September 11, 2009

using System;
//using System.Collections.Generic;
//using System.Text;

namespace SolarFlareCommon
{
    public class CRC32
    {
        private UInt32[] m_table = null;

        public CRC32()
        {
            m_table = new UInt32[256];

            for( UInt32 n = 0; n < 256; ++n )
            {
                UInt32 c = n;

                for( int k = 0; k < 8; ++k )    // 8 bits per byte
                {

                    if( (c & 1) != 0 )
                    {
                        c = 0xedb88320 ^ (c >> 1);
                    }
                    else
                    {
                        c >>= 1;
                    }
                }

                m_table[n] = c;
            }
        }

        private UInt32 UpdateCRC(UInt32 c, byte[] srcData)
        {

            foreach( byte srcByte in srcData )
            {
                c = m_table[(c ^ srcByte) & 0xff] ^ (c >> 8);
            }

            return c;
        }

        public UInt32 ComputeChecksum(byte[] srcData)
        {
            return UpdateCRC(0xffffffff, srcData) ^ 0xffffffff;
        }
   }
}
