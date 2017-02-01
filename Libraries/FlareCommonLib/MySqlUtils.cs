// MySqlUtils.cs - By Tom Weatherhead - August 12, 2009

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace SolarFlareCommon
{
    public static class MySqlUtils
    {
        // Boolean to String and String to Boolean.

        public static string DatabaseBooleanToString(bool b)    // ... or should it be called BooleanToDatabaseString() ?  No; see below.
        {
            return StringUtils.DatabaseBooleanToString(b);
        }

        public static string BooleanToDatabaseString(bool b)    // Note that this method wraps the string in single quotes.
        {
            return string.Format(@"'{0}'", DatabaseBooleanToString(b));
        }

        public static bool DatabaseStringToBoolean(string s)
        {
            return StringUtils.DatabaseStringToBoolean(s);
        }

        public static bool DatabaseStringToBoolean(MySqlDataReader reader, int i)
        {
            return StringUtils.DatabaseStringToBoolean(reader, i);
        }

        public static string DateTimeToString(DateTime dt)  // The returned string is parseable via DateTime.Parse()
        {
            // E.g. @"1999-12-31 23:59:59"
            /*
            return string.Format(@"{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}",
                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
             */
            // HH indicates a 24-hour clock; hh indicates a 12-hour clock.
            return dt.ToString(@"yyyy-MM-dd HH:mm:ss");
        }

        public static string DateTimeToDatabaseString(DateTime dt)
        {
            // E.g. @"'1999-12-31 23:59:59'"
            return string.Format(@"'{0}'", DateTimeToString(dt));
        }

        public static string NullableDateTimeToDatabaseString(DateTime? ndt)
        {

            if (!ndt.HasValue)
            {
                return @"NULL";
            }

            return DateTimeToDatabaseString(ndt.Value);
        }

        public static void ExecuteNonQuery(MySqlConnection con, string strCommandText)
        {

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static string RawStringToDatabaseString(string strRaw)
        {
            /*
            if (strRaw == null)
            {
                return @"NULL";
            }
             */

            // How is an empty string represented in SQL?  I assume that two consecutive single quotes
            // would be interpreted as a single literal single quote.

            const char cSingleQuote = '\'';
            StringBuilder sb = new StringBuilder();

            sb.Append(cSingleQuote);

            for (int i = 0; i < strRaw.Length; ++i)
            {
                char c = strRaw[i];

                if (c == cSingleQuote)
                {
                    sb.Append(@"''");
                }
                else
                {
                    sb.Append(c);
                }
            }

            sb.Append(cSingleQuote);
            return sb.ToString();
        }
    } // class MySqlUtils
} // namespace SolarFlareCommon

// **** End of File ****
