// MD5.cs - Tom Weatherhead - September 11, 2009

using System;
//using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SolarFlareCommon
{
    public static class MD5
    {
        public static string ComputeChecksum(byte[] sourceData)
        {

            if (sourceData == null)
            {
                throw new ArgumentNullException(@"sourceData", @"SolarFlareCommon.MD5.ComputeChecksum(byte[]) : Source data ref is null");
            }

            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(sourceData);
            StringBuilder sb = new StringBuilder();

            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X02"));
            }

            return sb.ToString();
        }

        public static string ComputeChecksum(string filePath)
        {

            if (filePath == null)
            {
                throw new ArgumentNullException(@"filePath", @"SolarFlareCommon.MD5.ComputeChecksum(string) : File path is null");
            }

            byte[] sourceData = File.ReadAllBytes(filePath);

            return ComputeChecksum(sourceData);
        }
    }
}
