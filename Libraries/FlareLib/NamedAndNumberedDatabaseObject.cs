// FlareLib\NamedAndNumberedDatabaseObject.cs - By Tom Weatherhead - July 8, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FlareLib
{
    public abstract class NamedAndNumberedDatabaseObject
    {
        // **** Member variables ****

        private UInt32 m_unID = 0;
        //private bool m_bEnabled = false;
        private string m_strName = string.Empty;

        // **** Constructors ****

        public NamedAndNumberedDatabaseObject()
        {
        }

        public NamedAndNumberedDatabaseObject(MySqlDataReader reader, int nIDColumn, int nNameColumn)
        {
            m_unID = reader.GetUInt32(nIDColumn);
            m_strName = reader.GetString(nNameColumn);
        }

        // **** Member accessor properties ****

        public UInt32 ID
        {
            get
            {
                return m_unID;
            }
            set
            {
                m_unID = value;
            }
        }

        public string Name
        {
            get
            {
                return m_strName;
            }
            set
            {
                m_strName = value;
            }
        }

        // **** SQL command string properties ****

        // Select command string properties are static; they are defined in the derived classes.

        public abstract string InsertCommand { get; }

        public abstract string UpdateCommand { get; }

        public abstract string DeleteCommand { get; }

        // **** Methods ****

        public static Dictionary<string, UInt32> GetNameToIDDictionary(MySqlConnection con, string strSelectIDAndNameCommand)
        {
            string strCommandText = strSelectIDAndNameCommand;

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Dictionary<string, UInt32> dict = new Dictionary<string, UInt32>();

                    while (reader.Read())
                    {
                        UInt32 unID = reader.GetUInt32(0);
                        string strName = reader.GetString(1);

                        dict.Add(strName, unID);
                    }

                    return dict;
                }
            }
        }

        public static Dictionary<uint, string> GetIDToNameDictionary(Dictionary<string, uint> dictNameToID)
        {
            // The LINQ version would be something like this:
            //return from kvp in dictNameToID select new KeyValuePair<uint, string>{ kvp.Value, kvp.Key };
            // ... except that the resulting expression would be of type IEnumerable<KeyValuePair<uint, string>>; it would not be a Dictionary.
            var dictIDToName = new Dictionary<uint, string>();

            foreach (KeyValuePair<string, uint> kvp in dictNameToID)
            {
                dictIDToName.Add(kvp.Value, kvp.Key);
            }

            return dictIDToName;
        }

        public static MySqlDataReader GetSingleRowReaderFromSelectCommand(MySqlConnection con, string strCommandText)
        {
            MySqlCommand cmd = new MySqlCommand(strCommandText, con);
            MySqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                throw new ArgumentException(
                    string.Format(@"GetSingleRowReaderFromSelectCommand() : No record found for command '{0}'", strCommandText),
                    @"strCommandText");
            }

            return reader;
        }

        protected abstract void ValidateDataBeforeSave();   // Throw an informative exception if any data is invalid.

        protected virtual void AdditionalInsertCode(MySqlConnection con)
        {
        }

        protected virtual void AdditionalUpdateCode(MySqlConnection con)
        {
        }

        public void SaveToDatabase(MySqlConnection con)
        {
            ValidateDataBeforeSave();

            if (ID == 0)
            {
                // Insert
                string strCommandText = InsertCommand;

                using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
                {

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {

                        if (!reader.Read())
                        {
                            throw new Exception(@"Insert: 'SELECT LAST_INSERT_ID()' seems to have failed");
                        }

                        ID = reader.GetUInt32(0);
                    }
                }

                AdditionalInsertCode(con);
            }
            else
            {
                // Update
                MySqlUtils.ExecuteNonQuery(con, UpdateCommand);

                AdditionalUpdateCode(con);
            }
        }

        protected virtual void AdditionalDeleteCode(MySqlConnection con)
        {
        }

        public void DeleteFromDatabase(MySqlConnection con)
        {

            if (ID == 0)
            {
                // This object's data has not yet been inserted into the database table; there is nothing to delete.
                return;
            }

            AdditionalDeleteCode(con);

            MySqlUtils.ExecuteNonQuery(con, DeleteCommand);
        }
    } // class NamedAndNumberedDatabaseObject
}

// **** End of File ****
