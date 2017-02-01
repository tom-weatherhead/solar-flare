// FlareLib\TargetFormFieldCollection.cs - By Tom Weatherhead - August 4, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace FlareLib
{
    public class TargetFormFieldCollection : ICollection<TargetFormField>
    {
        // Member variable
        private readonly List<TargetFormField> m_lstTargetFormFields;

        // Constructor
        public TargetFormFieldCollection()
        {
            m_lstTargetFormFields = new List<TargetFormField>();
        }

        // Implementation of ICollection<>

        public void Add(TargetFormField field)
        {
            m_lstTargetFormFields.Add(field);
        }

        public void Clear()
        {
            m_lstTargetFormFields.Clear();
        }

        public bool Contains(TargetFormField field)
        {
            return m_lstTargetFormFields.Contains(field);
        }

        public void CopyTo(TargetFormField[] array, int arrayIndex)
        {
            m_lstTargetFormFields.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return m_lstTargetFormFields.Count;
            }
        }

        /* No private/protected/public modifier */ System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_lstTargetFormFields.GetEnumerator();
        }

        /* No private/protected/public modifier */ IEnumerator<TargetFormField> IEnumerable<TargetFormField>.GetEnumerator()
        {
            return m_lstTargetFormFields.GetEnumerator();
            /*

            foreach (TargetFormField field in m_lstTargetFormFields)
            {
                yield return field;
            }
             */
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(TargetFormField field)
        {
            return m_lstTargetFormFields.Remove(field);
        }

        // Methods

        public TargetFormField FindByFieldID(UInt32 unFieldID)
        {

            foreach (TargetFormField field in m_lstTargetFormFields)
            {

                if (field.FieldID == unFieldID)
                {
                    return field;
                }
            }

            return null;
        }

        public TargetFormField FindByFieldName(string strFieldName)
        {

            foreach (TargetFormField field in m_lstTargetFormFields)
            {

                if (field.FieldName == strFieldName)
                {
                    return field;
                }
            }

            return null;
        }

        public void LoadFromDatabase(MySqlConnection con, UInt32 unTargetID)
        {

            if (unTargetID == 0)
            {
                return;
            }

            string strCommandText = TargetFormField.GetSelectCommand(unTargetID);

            using (MySqlCommand cmd = new MySqlCommand(strCommandText, con))
            {

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        Add(new TargetFormField(reader));
                    }
                }
            }
        }

        public void SaveToDatabase(MySqlConnection con)
        {
            List<TargetFormField> lstDeletia = new List<TargetFormField>();

            foreach (TargetFormField field in m_lstTargetFormFields)
            {
                field.SaveToDatabase(con);

                if (field.ToBeDeleted)
                {
                    //m_lstTargetFormFields.Remove(field);  // Try this instead of using lstDeletia ?
                    lstDeletia.Add(field);
                }
            }

            foreach (TargetFormField field in lstDeletia)
            {
                m_lstTargetFormFields.Remove(field);
            }
        }

        public void DeleteFromDatabase(MySqlConnection con, UInt32 unTargetID)
        {
            string strCommandText = string.Format(@"DELETE FROM TargetFormFields WHERE TargetID={0};", unTargetID);

            MySqlUtils.ExecuteNonQuery(con, strCommandText);
        }
    } // class TargetFormFieldCollection
} // namespace FlareLib
