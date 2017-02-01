// FlareLib\SystemConfigurationEntry.cs - By Tom Weatherhead - September 14, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlareLib
{
    public static class SystemConfigurationEntryKeys
    {
        public static string SMTPServerAddress { get { return @"SMTPServerAddress"; } }
        public static string SMTPServerPassword { get { return @"SMTPServerPassword"; } }
        public static string SMTPServerUserName { get { return @"SMTPServerUserName"; } }
    }

    public class SystemConfigurationEntry : NamedAndNumberedDatabaseObject
    {
        // Column: ID (INT) (Primary key) (Auto increment)
        // Column: Name (VARCHAR(100))
        // Column: Value (VARCHAR(255))
        private string m_strValue = string.Empty;

        public SystemConfigurationEntry()
            : base()
        {
        }

        public SystemConfigurationEntry(MySqlDataReader reader, bool bCloseReader)  // Create the SystemConfigurationEntry object from a row of data.
            : base(reader, 0, 1)
        {
            m_strValue = reader.GetString(2);

            if (bCloseReader)
            {
                reader.Close(); // We are only permitted one open data reader per connection at a time.
            }
        }

        public SystemConfigurationEntry(MySqlConnection con, UInt32 unID)
            : this(GetSingleRowReaderFromSelectCommand(con, SelectAllCommandBasedOnID(unID)), true)
        {
        }

        // **** Member accessor properties ****

        public string Value
        {
            get
            {
                return m_strValue;
            }
            set
            {
                m_strValue = value;
            }
        }

        // **** SQL command string properties and methods ****

        public static string SelectAllCommand
        {
            get
            {
                return @"SELECT ID, Name, Value FROM SystemConfiguration;";
            }
        }

        public static string SelectIDAndNameCommand
        {
            get
            {
                return @"SELECT ID, Name FROM SystemConfiguration ORDER BY Name;";
            }
        }

        public static string SelectAllCommandBasedOnID(uint unID)
        {
            return string.Format(@"SELECT ID, Name, Value FROM SystemConfiguration WHERE ID={0};", unID);
        }

        public override string InsertCommand
        {
            get
            {
                return string.Format(@"INSERT INTO SystemConfiguration VALUES (NULL, {0}, {1}); SELECT LAST_INSERT_ID() FROM SystemConfiguration;",
                    MySqlUtils.RawStringToDatabaseString(Name),
                    MySqlUtils.RawStringToDatabaseString(Value));
            }
        }

        public override string UpdateCommand
        {
            get
            {
                return string.Format(@"UPDATE SystemConfiguration SET Name={0}, Value={1} WHERE ID={2};",
                    MySqlUtils.RawStringToDatabaseString(Name),
                    MySqlUtils.RawStringToDatabaseString(Value),
                    ID);
            }
        }

        public override string DeleteCommand
        {
            get
            {
                return string.Format(@"DELETE FROM SystemConfiguration WHERE ID={0};", ID);
            }
        }

        // **** Methods ****

        public static Dictionary<string, string> GetDictionary(MySqlConnection con)
        {
            string strCommandText = SelectAllCommand;

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();

                    while (reader.Read())
                    {
                        SystemConfigurationEntry sce = new SystemConfigurationEntry(reader, false);

                        dict.Add(sce.Name, sce.Value);
                    }

                    return dict;
                }
            }
        }

        protected override void ValidateDataBeforeSave()    // Throw an informative exception if any data is invalid.
        {

            if (Name == null)
            {
                throw new Exception(@"Cannot save the information: The entry name is null.");
            }
            else if (Name == string.Empty)
            {
                throw new Exception(@"Cannot save the information: The entry name is empty.");
            }

            if (Value == null)
            {
                throw new Exception(@"Cannot save the information: The entry value is null.");
            }

            // We will allow the value to be the empty string.
        }
    } // class SystemConfigurationEntry
} // namespace FlareLib

// **** End of File ****
