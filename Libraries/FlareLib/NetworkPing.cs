// FlareLib NetworkPing.cs - By Tom Weatherhead - August 21, 2009

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace FlareLib
{
    public static class NetworkPing
    {
        private static string GetHostNameFromURL(string strURL)
        {
            string strHostName = strURL;
            int nIndexOfFirstDoubleSlash = strHostName.IndexOf(@"//");

            if (nIndexOfFirstDoubleSlash >= 0)
            {
                strHostName = strHostName.Substring(nIndexOfFirstDoubleSlash + 2);
            }

            int nIndexOfFirstSlash = strHostName.IndexOf('/');

            if (nIndexOfFirstSlash >= 0)
            {
                strHostName = strHostName.Substring(0, nIndexOfFirstSlash);
            }

            int nIndexOfFirstColon = strHostName.IndexOf(':');  // In case of www.example.org:8000

            if (nIndexOfFirstColon >= 0)
            {
                strHostName = strHostName.Substring(0, nIndexOfFirstColon);
            }

            return strHostName;
        }

        // args[0] can be an IPaddress or host name.

        public static bool Send(string strURL, out string strResultMessageForLog, out long lResponseTime)
        {
            string strHostName = GetHostNameFromURL(strURL);
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted.
            const string data = @"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            const int timeout = 120;    // Millieconds
            PingReply reply = pingSender.Send(strHostName, timeout, buffer, options);
            bool bSuccess = (reply.Status == IPStatus.Success);

            strResultMessageForLog = string.Format(@"Ping reply status: {0}", reply.Status);

            if (bSuccess)
            {
                /*
                Console.WriteLine("Address: {0}", reply.Address.ToString());
                Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime);
                Console.WriteLine("Time to live: {0}", reply.Options.Ttl);
                Console.WriteLine("Don't fragment: {0}", reply.Options.DontFragment);
                Console.WriteLine("Buffer size: {0}", reply.Buffer.Length);
                 */
                strResultMessageForLog = strResultMessageForLog + string.Format(
                    @"; Address: {0}; RoundTrip time: {1} milliseconds; Time to live: {2}; Don't fragment: {3}; Buffer size: {4}",
                    reply.Address.ToString(),
                    reply.RoundtripTime,
                    reply.Options.Ttl,
                    reply.Options.DontFragment,
                    reply.Buffer.Length);
                lResponseTime = reply.RoundtripTime;
            }
            else
            {
                lResponseTime = 0;
            }

            return bSuccess;
        }
    } // class NetworkPing
} // namespace FlareLib
