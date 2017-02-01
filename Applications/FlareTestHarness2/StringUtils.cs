// StringUtils.cs - By Tom Weatherhead - June 1, 2009

using System;
using System.Collections.Generic;
//using System.Text;

namespace SolarFlareCommon
{
    public static class StringUtils
    {
        public static bool IsDegenerateString(string s)
        {
            return (s == null || s.Trim() == string.Empty);
        }

        public static void ThrowExceptionIfStringArgumentIsDegenerate(string strMethodName, string strStringArgumentName, string strStringArgument)
        {

            if (strStringArgument == null)
            {
                throw new ArgumentNullException(strStringArgumentName,
                    string.Format(@"{0}: string parameter '{1}' is null.", strMethodName, strStringArgumentName));
            }
            else if (strStringArgument.Trim() == string.Empty)
            {
                throw new ArgumentException(
                    string.Format(@"{0}: string parameter '{1}', when trimmed, is empty.", strMethodName, strStringArgumentName),
                    strStringArgumentName);
            }
        }

        public static void EnsureStringNotDegenerate_ByRef(ref string s, string strDefault)
        {

            if (IsDegenerateString(s))
            {
                
                if (IsDegenerateString(strDefault))
                {
                    throw new ArgumentException(
                        @"SolarFlareCommon.StringUtils.EnsureStringNotDegenerate_ByRef() : Default string is degenerate.",
                        @"strDefault");
                }

                s = strDefault;
            }
        }

        public static string EnsureStringNotDegenerate_ByValue(string s, string strDefault)
        {
            EnsureStringNotDegenerate_ByRef(ref s, strDefault);
            return s;
        }

        /*
        public static List<string> CaseInsensitiveStringMatchFilter(string str, IEnumerable<string> iestrIn)
        {
            List<string> lstrOut = new List<string>();

            foreach (string strIn in iestrIn)
            {

                if (string.Compare(str, strIn, true) == 0)  // The "true" means that case will be ignored.
                {
                    lstrOut.Add(strIn);
                }
            }

            return lstrOut;
        }
        */

        public static string FindFirstCaseInsensitiveString(string str, IEnumerable<string> iestrIn)
        {

            foreach (string strIn in iestrIn)
            {

                if (string.Compare(str, strIn, true) == 0)  // The "true" means that case will be ignored.
                {
                    return strIn;
                }
            }

            return null;
        }
    } // class StringUtils
} // namespace SolarFlareCommon

// **** End of File ****
