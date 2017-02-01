// FlareLib\Account.cs - By Tom Weatherhead - July 3, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FlareLib
{
    public class Account : NamedAndNumberedDatabaseObject
    {
        private bool m_bEnabled = false;

        public Account()
            : base()
        {
        }

        public Account(MySqlDataReader reader, bool bCloseReader)  // Create the Account object from a row of data.
            : base(reader, 0, 2)
        {
            m_bEnabled = StringUtils.DatabaseStringToBoolean(reader, 1);

            if (bCloseReader)
            {
                reader.Close(); // We are only permitted one open data reader per connection at a time.
            }
        }

        public Account(MySqlConnection con, UInt32 unID)
            : this(GetSingleRowReaderFromSelectCommand(con, SelectAllCommandBasedOnID(unID)), true)
        {
        }

        // **** Member accessor properties ****

        public UInt32 AccountID
        {
            get
            {
                return ID;
            }
            set
            {
                ID = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return m_bEnabled;
            }
            set
            {
                m_bEnabled = value;
            }
        }

        // **** SQL command string properties and methods ****

        public static string SelectAllCommand
        {
            get
            {
                //return @"SELECT AccountID, Enabled, Name FROM flare.Accounts;";
                return @"SELECT AccountID, Enabled, Name FROM Accounts;";
            }
        }

        public static string SelectIDAndNameCommand
        {
            get
            {
                return @"SELECT AccountID, Name FROM Accounts ORDER BY Name;";
            }
        }

        public static string SelectAllCommandBasedOnID(UInt32 unID)
        {
            return string.Format(@"SELECT AccountID, Enabled, Name FROM Accounts WHERE AccountID={0};", unID);
        }

        /*
        public string SelectAllCommandBasedOnName   // Unused?
        {
            get
            {
                // SELECT COUNT(*) FROM ...
                return string.Format(@"SELECT AccountID, Enabled, Name FROM Accounts WHERE Name like '{0}';", Name);
            }
        }
         */

        public override string InsertCommand
        {
            get
            {
                return string.Format(@"INSERT INTO Accounts VALUES (NULL, {0}, {1}); SELECT LAST_INSERT_ID() FROM Accounts;",
                    MySqlUtils.BooleanToDatabaseString(Enabled),
                    MySqlUtils.RawStringToDatabaseString(Name));
            }
        }

        public override string UpdateCommand
        {
            get
            {
                return string.Format(@"UPDATE Accounts SET Enabled={0}, Name={1} WHERE AccountID={2};",
                    MySqlUtils.BooleanToDatabaseString(Enabled),
                    MySqlUtils.RawStringToDatabaseString(Name),
                    AccountID);
            }
        }

        public override string DeleteCommand
        {
            get
            {
                return string.Format(@"DELETE FROM Accounts WHERE AccountID={0};", AccountID);
            }
        }

        // **** Methods ****

        /*
        public static Dictionary<string, UInt32> GetNameToAccountIDDictionary(MySqlConnection con)
        {
            return GetNameToIDDictionary(con, SelectIDAndNameCommand);
        }
         */

        /*
        public static Account GetAccountBasedOnID(MySqlConnection con, UInt32 unID)
        {
            string strCommandText = SelectAllCommandBasedOnID(unID);

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    if (!reader.Read())
                    {
                        throw new ArgumentException(
                            string.Format(@"Account constructor: Accounts table: No record found for ID == {0}", unID),
                            @"unID");
                    }

                    return new Account(reader);
                }
            }
        }
         */

        /*
        public static UInt32 GetAccountIDAfterInsert(MySqlConnection con)
        {
            // Execute this query using the same MySqlConnection that was used to do the insert.
            // Or execute the insert and this LAST_INSERT_ID query in the same command using semicolon-separated command text.

            // SELECT @last := LAST_INSERT_ID();
            // SELECT LAST_INSERT_ID();
            // SELECT LAST_INSERT_ID() FROM Accounts;
            const string strCommandText = @"SELECT LAST_INSERT_ID() FROM Accounts;";

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    if (!reader.Read())
                    {
                        return 0;
                    }

                    return reader.GetUInt32(0);
                }
            }
        }
         */

        protected override void ValidateDataBeforeSave()    // Throw an informative exception if any data is invalid.
        {

            if (Name == null)
            {
                throw new Exception(@"Cannot save the account information: The account name is null.");
            }
            else if (Name == string.Empty)
            {
                throw new Exception(@"Cannot save the account information: The account name is empty.");
            }
        }
    } // class Account
} // namespace FlareLib

// **** End of File ****
