// StringUtils.cs - By Tom Weatherhead - June 1, 2009

using MySql.Data.MySqlClient;
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

        public static string DatabaseBooleanToString(bool b)
        {
            return b ? @"Y" : @"N";
        }

        public static bool DatabaseStringToBoolean(string s)
        {

            if (s == null)
            {
                throw new ArgumentNullException(@"s", @"DatabaseStringToBoolean() : String argument is null");
            }

            s = s.ToUpper();

            if (s == @"Y" || s == @"T")
            {
                return true;
            }

            if (s != @"N" && s != @"F")
            {
                throw new ArgumentException(@"DatabaseStringToBoolean() : Invalid string argument", @"s");
            }

            return false;
        }

        public static bool DatabaseStringToBoolean(MySqlDataReader reader, int i)
        {
            return DatabaseStringToBoolean(reader.GetString(i));
        }

        public static DateTime ValidateDateTimeString(string strDateTime, string strExceptionMessage)
        {

            try
            {
                return DateTime.Parse(strDateTime);
            }
            catch (FormatException ex)
            {
                throw new Exception(strExceptionMessage, ex);
            }
        }
    } // class StringUtils
} // namespace SolarFlareCommon

// **** End of File ****
