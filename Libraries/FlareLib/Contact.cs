// FlareLib\Contact.cs - By Tom Weatherhead - September 14, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FlareLib
{
    public class Contact : NamedAndNumberedDatabaseObject
    {
        // Column: ContactID (10-digit integer) (INT) (Primary key) (Auto increment)
        // Column: AccountID (10-digit integer) (INT) (Foreign key?)
        private uint m_unAccountID = 0;
        // Column: Enabled (Boolean; 1-character string: "Y" or "N") (CHAR(1))
        private bool m_bEnabled = false;
        // Column: FirstName (50-character string) (VARCHAR(50))
        private string m_strFirstName = string.Empty;
        // Column: LastName (50-character string) (VARCHAR(50))
        private string m_strLastName = string.Empty;
        // Column: EmailAddress (255-character string) (VARCHAR(255))
        private string m_strEmailAddress = string.Empty;

        public Contact()
            : base()
        {
        }

        public Contact(MySqlDataReader reader, bool bCloseReader)  // Create the Contact object from a row of data.
            : base(reader, 0, 4)    // The "4" here sets the Name property to the last name; we will set it more specifically below.
        {
            m_unAccountID = reader.GetUInt32(1);
            m_bEnabled = StringUtils.DatabaseStringToBoolean(reader, 2);
            m_strFirstName = reader.GetString(3);
            m_strLastName = reader.GetString(4);
            m_strEmailAddress = reader.GetString(5);

            Name = ConstructFullNamePlusID(ID, m_strFirstName, m_strLastName);   // Name will be unique because the ID is unique.

            if (bCloseReader)
            {
                reader.Close(); // We are only permitted one open data reader per connection at a time.
            }
        }

        public Contact(MySqlConnection con, UInt32 unID)
            : this(GetSingleRowReaderFromSelectCommand(con, SelectAllCommandBasedOnID(unID)), true)
        {
        }

        // **** Member accessor properties ****

        public UInt32 ContactID
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

        public UInt32 AccountID
        {
            get
            {
                return m_unAccountID;
            }
            set
            {
                m_unAccountID = value;
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

        public string FirstName
        {
            get
            {
                return m_strFirstName;
            }
            set
            {
                m_strFirstName = value;
            }
        }

        public string LastName
        {
            get
            {
                return m_strLastName;
            }
            set
            {
                m_strLastName = value;
            }
        }

        public string EmailAddress
        {
            get
            {
                return m_strEmailAddress;
            }
            set
            {
                m_strEmailAddress = value;
            }
        }

        // **** SQL command string properties and methods ****

        public static string SelectAllCommand
        {
            get
            {
                return @"SELECT ContactID, AccountID, Enabled, FirstName, LastName, EmailAddress FROM Contacts;";
            }
        }

        public static string SelectAllCommandBasedOnID(uint unID)
        {
            return string.Format(@"SELECT ContactID, AccountID, Enabled, FirstName, LastName, EmailAddress FROM Contacts WHERE ContactID={0};", unID);
        }

        public override string InsertCommand
        {
            get
            {
                return string.Format(@"INSERT INTO Contacts VALUES (NULL, {0}, {1}, {2}, {3}, {4}); SELECT LAST_INSERT_ID() FROM Contacts;",
                    AccountID,
                    MySqlUtils.BooleanToDatabaseString(Enabled),
                    MySqlUtils.RawStringToDatabaseString(FirstName),
                    MySqlUtils.RawStringToDatabaseString(LastName),
                    MySqlUtils.RawStringToDatabaseString(EmailAddress));
            }
        }

        public override string UpdateCommand
        {
            get
            {
                return string.Format(@"UPDATE Contacts SET AccountID={0}, Enabled={1}, FirstName={2}, LastName={3}, EmailAddress={4} WHERE ContactID={5};",
                    AccountID,
                    MySqlUtils.BooleanToDatabaseString(Enabled),
                    MySqlUtils.RawStringToDatabaseString(FirstName),
                    MySqlUtils.RawStringToDatabaseString(LastName),
                    MySqlUtils.RawStringToDatabaseString(EmailAddress),
                    ContactID);
            }
        }

        public override string DeleteCommand
        {
            get
            {
                return string.Format(@"DELETE FROM Contacts WHERE ContactID={0};", ContactID);
            }
        }

        // **** Methods ****

        public static string ConstructFullNamePlusID(uint unContactID, string strFirstName, string strLastName)
        {
            //return string.Format(@"{0}: {1}, {2}", unContactID, strLastName, strFirstName);
            return string.Format(@"{0}, {1} ({2})", strLastName, strFirstName, unContactID);
        }

        public string ConstructFullNamePlusID()
        {
            return ConstructFullNamePlusID(ContactID, FirstName, LastName);
        }

        public static Dictionary<string, uint> GetFullNamePlusIDToIDDictionary(MySqlConnection con)
        {
            const string strCommandText = @"SELECT ContactID, FirstName, LastName FROM Contacts;";  // ORDER BY LastName, FirstName;

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Dictionary<string, uint> dict = new Dictionary<string, uint>();

                    while (reader.Read())
                    {
                        uint unContactID = reader.GetUInt32(0);
                        string strFirstName = reader.GetString(1);
                        string strLastName = reader.GetString(2);
                        string strFullNamePlusID = ConstructFullNamePlusID(unContactID, strFirstName, strLastName);

                        dict.Add(strFullNamePlusID, unContactID);
                    }

                    return dict;
                }
            }
        }

        public static List<Contact> GetAllContacts(MySqlConnection con)
        {
            string strCommandText = SelectAllCommand;

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    List<Contact> lstContacts = new List<Contact>();

                    while (reader.Read())
                    {
                        Contact c = new Contact(reader, false);

                        lstContacts.Add(c);
                    }

                    return lstContacts;
                }
            }
        }

        protected override void ValidateDataBeforeSave()    // Throw an informative exception if any data is invalid.
        {

            if (FirstName == null)
            {
                throw new Exception(@"Cannot save the contact information: The contact's first name is null.");
            }
            else if (FirstName == string.Empty)
            {
                throw new Exception(@"Cannot save the contact information: The contact's first name is empty.");
            }

            if (LastName == null)
            {
                throw new Exception(@"Cannot save the contact information: The contact's last name is null.");
            }
            else if (LastName == string.Empty)
            {
                throw new Exception(@"Cannot save the contact information: The contact's last name is empty.");
            }

            if (EmailAddress == null)
            {
                throw new Exception(@"Cannot save the contact information: The contact's e-mail address is null.");
            }
            else if (EmailAddress == string.Empty)
            {
                throw new Exception(@"Cannot save the contact information: The contact's e-mail address is empty.");
            }
        }
    } // class Contact
} // namespace FlareLib

// **** End of File ****
