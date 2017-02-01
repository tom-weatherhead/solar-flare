// FlareAdminWebApp Contacts.aspx.cs - By Tom Weatherhead - September 15, 2009

using FlareLib;
using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
//using System.Xml.Linq;

namespace FlareAdminWebApp
{
    public partial class SystemConfigurationPage : FlareAdminWebApp.Page  //System.Web.UI.Page
    {
        private const string m_strSystemConfigurationEntryNameToIDDictionarySessionKey = @"FlareSystemConfigurationEntryNameToIDDictionary";
        private Dictionary<string, UInt32> m_dict = null;

        protected Dictionary<string, UInt32> SystemConfigurationEntryNameToIDDictionary
        {
            get
            {
                return Session[m_strSystemConfigurationEntryNameToIDDictionarySessionKey] as Dictionary<string, UInt32>;
            }
            set
            {
                Session[m_strSystemConfigurationEntryNameToIDDictionarySessionKey] = value;
            }
        }

        protected SystemConfigurationEntry GetSystemConfigurationEntryObjectBasedOnUI()   // Do not get the SystemConfigurationEntry object from the database.
        {
            UInt32 unSelectedEntryID = 0;
            string strSelectedEntryName = lbEntryNames.SelectedValue;

            if (strSelectedEntryName != null && m_dict.ContainsKey(strSelectedEntryName))
            {
                unSelectedEntryID = m_dict[strSelectedEntryName];
            }

            SystemConfigurationEntry entry = new SystemConfigurationEntry();

            entry.ID = unSelectedEntryID;
            entry.Name = tbName.Text;
            entry.Value = tbValue.Text;
            return entry;
        }

        protected void ClearEntryUIFields()
        {
            tbID.Text = string.Empty;
            tbName.Text = string.Empty;
            tbValue.Text = string.Empty;

            lbEntryNames.SelectedValue = null;
        }

        protected override void PageLoadWorker(object sender, EventArgs e)
        {
            bool bPopulateEntryNamesListBox = false;

            // Ensure that m_dict is set.
            m_dict = SystemConfigurationEntryNameToIDDictionary;

            if (m_dict == null)
            {
                MySqlConnection con = GetMySqlConnection();

                m_dict = NamedAndNumberedDatabaseObject.GetNameToIDDictionary(con, SystemConfigurationEntry.SelectIDAndNameCommand);
                SystemConfigurationEntryNameToIDDictionary = m_dict;
                bPopulateEntryNamesListBox = true;
            }
            else if (!IsPostBack)
            {
                bPopulateEntryNamesListBox = true;
            }

            if (bPopulateEntryNamesListBox)
            {
                WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dict.Keys, lbEntryNames.Items);
            }
        }

        protected void lbEntryNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedEntryName = lbEntryNames.SelectedValue;

            if (strSelectedEntryName == null || !m_dict.ContainsKey(strSelectedEntryName))
            {
                ClearEntryUIFields();
                return;
            }

            UInt32 unSelectedEntryID = m_dict[strSelectedEntryName];
            MySqlConnection con = GetMySqlConnection();
            SystemConfigurationEntry entry = new SystemConfigurationEntry(con, unSelectedEntryID);

            tbID.Text = entry.ID.ToString();
            tbName.Text = entry.Name;
            tbValue.Text = entry.Value;
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            ClearEntryUIFields();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

            try
            {
                SystemConfigurationEntry entry = GetSystemConfigurationEntryObjectBasedOnUI();

                // Ensure that the entry fields are valid; e.g. ensure that the strings contain no invalid characters such as the single quote.

                //if (!entry.Validate()) { }

                // Avoid entry name collisions

                if (m_dict.ContainsKey(entry.Name) && m_dict[entry.Name] != entry.ID)
                {
                    throw new Exception(@"Cannot save: Another system configuration entry is using the specified name");
                    //return;
                }

                MySqlConnection con = GetMySqlConnection();

                entry.SaveToDatabase(con);
                tbID.Text = entry.ID.ToString();

                string strSelectedEntryName = lbEntryNames.SelectedValue;   // The old entry name, if the name was changed.

                if (strSelectedEntryName != null && strSelectedEntryName != entry.Name)
                {
                    lbEntryNames.Items.Remove(strSelectedEntryName);
                    m_dict.Remove(strSelectedEntryName);
                }

                //if (!lbEntryNames.Items.Contains(ListItem item))
                if (!m_dict.ContainsKey(entry.Name))
                {
                    lbEntryNames.Items.Add(entry.Name);
                    m_dict.Add(entry.Name, entry.ID);
                }

                lbEntryNames.SelectedValue = entry.Name;
            }
            catch (Exception ex)
            {
                SetStatusMessageLabel(ex);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            SystemConfigurationEntry entry = GetSystemConfigurationEntryObjectBasedOnUI();

            if (entry.ID != 0)
            {
                string strSelectedEntryName = lbEntryNames.SelectedValue;

                if (strSelectedEntryName != null)
                {
                    lbEntryNames.Items.Remove(strSelectedEntryName);
                    m_dict.Remove(strSelectedEntryName);
                }

                MySqlConnection con = GetMySqlConnection();

                entry.DeleteFromDatabase(con);
            }

            ClearEntryUIFields();
        }
    }
}

// **** End of File ****
