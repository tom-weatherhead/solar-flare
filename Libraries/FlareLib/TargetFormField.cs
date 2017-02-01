// FlareLib\TargetFormField.cs - By Tom Weatherhead - August 4, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FlareLib
{
    public class TargetFormField : IEquatable<TargetFormField>     // ThAW 2009/08/05 : We probably don't need to implement IEquatable<> here.
    {
        // Member variables
        private UInt32 m_unFieldID = 0;
        private UInt32 m_unTargetID = 0;
        private string m_strFieldName = string.Empty;
        private string m_strFieldValue = string.Empty;
        private bool m_bToBeUpdated = false;
        private bool m_bToBeDeleted = false;

        // Constructors

        public TargetFormField()
        {
        }

        public TargetFormField(UInt32 unFieldID, UInt32 unTargetID, string strFieldName, string strFieldValue)
        {
            m_unFieldID = unFieldID;
            m_unTargetID = unTargetID;
            m_strFieldName = strFieldName;
            m_strFieldValue = strFieldValue;
        }

        public TargetFormField(MySqlDataReader reader)
        {
            m_unFieldID = reader.GetUInt32(0);
            m_unTargetID = reader.GetUInt32(1);
            m_strFieldName = reader.GetString(2);
            m_strFieldValue = reader.GetString(3);
        }

        // Implementation of IEquatable<>

        public bool Equals(TargetFormField other)
        {
            return (FieldID == other.FieldID &&
                TargetID == other.TargetID &&
                FieldName == other.FieldName &&
                FieldValue == other.FieldValue &&
                ToBeUpdated == other.ToBeUpdated &&
                ToBeDeleted == other.ToBeDeleted);
        }

        // Methods

        public static string GetSelectCommand(UInt32 unTargetID)
        {
            return string.Format(@"SELECT FieldID, TargetID, FieldName, FieldValue FROM TargetFormFields WHERE TargetID={0};", unTargetID);
        }

        private void InsertIntoDatabase(MySqlConnection con)
        {
            // Assert(FieldID == 0);
            string strCommandText = string.Format(@"INSERT INTO TargetFormFields VALUES ({0}, {1}, {2}, NULL); SELECT LAST_INSERT_ID() FROM TargetFormFields;",
                TargetID,
                MySqlUtils.RawStringToDatabaseString(FieldName),
                MySqlUtils.RawStringToDatabaseString(FieldValue));

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    if (!reader.Read())
                    {
                        throw new Exception(@"Insert: 'SELECT LAST_INSERT_ID()' seems to have failed");
                    }

                    FieldID = reader.GetUInt32(0);
                }
            }
        }

        private void UpdateInDatabase(MySqlConnection con)
        {
            // Assert(FieldID > 0);
            string strCommandText = string.Format(@"UPDATE TargetFormFields SET TargetID={0}, FieldName={1}, FieldValue={2} WHERE FieldID={3};",
                TargetID,
                MySqlUtils.RawStringToDatabaseString(FieldName),
                MySqlUtils.RawStringToDatabaseString(FieldValue),
                FieldID);

            MySqlUtils.ExecuteNonQuery(con, strCommandText);
        }

        private void DeleteFromDatabase(MySqlConnection con)
        {
            // Assert(FieldID > 0);
            string strCommandText = string.Format(@"DELETE FROM TargetFormFields WHERE FieldID={0};", FieldID);

            MySqlUtils.ExecuteNonQuery(con, strCommandText);
        }

        public void ValidateDataBeforeSave()    // Throw an informative exception if any data is invalid.
        {

            if (FieldName == null)
            {
                throw new Exception(@"Cannot save the target form field: The field name is null.");
            }
            else if (FieldName == string.Empty)
            {
                throw new Exception(@"Cannot save the target form field: The field name is empty.");
            }

            if (FieldValue == null)
            {
                throw new Exception(@"Cannot save the target form field: The field value is null.");
            }

            // We will allow the value to be the empty string.
        }

        public void SaveToDatabase(MySqlConnection con)
        {

            if (FieldID == 0)
            {

                if (!ToBeDeleted)
                {
                    InsertIntoDatabase(con);
                }
                // else do nothing.
            }
            else if (ToBeDeleted)
            {
                DeleteFromDatabase(con);
            }
            else if (ToBeUpdated)
            {
                UpdateInDatabase(con);
            }

            ToBeUpdated = false;
        }

        // Properties

        public UInt32 FieldID
        {
            get
            {
                return m_unFieldID;
            }
            set
            {
                m_unFieldID = value;
            }
        }

        public UInt32 TargetID
        {
            get
            {
                return m_unTargetID;
            }
            set
            {
                m_unTargetID = value;
            }
        }

        public string FieldName
        {
            get
            {
                return m_strFieldName;
            }
            set
            {
                m_strFieldName = value;
            }
        }

        public string FieldValue
        {
            get
            {
                return m_strFieldValue;
            }
            set
            {
                m_strFieldValue = value;
            }
        }

        public bool ToBeUpdated
        {
            get
            {
                return m_bToBeUpdated;
            }
            set
            {
                m_bToBeUpdated = value;
            }
        }

        public bool ToBeDeleted
        {
            get
            {
                return m_bToBeDeleted;
            }
            set
            {
                m_bToBeDeleted = value;
            }
        }
    } // class TargetFormField
} // namespace FlareLib
