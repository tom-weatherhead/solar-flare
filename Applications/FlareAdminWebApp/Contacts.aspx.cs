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
    public partial class ContactsPage : FlareAdminWebApp.Page  //System.Web.UI.Page
    {
        private const string m_strContactNameToIDDictionarySessionKey = @"FlareContactNameToIDDictionary";
        private const string m_strAccountNameToIDDictionarySessionKey = @"FlareAccountNameToIDDictionary";
        private const string m_strAccountIDToNameDictionarySessionKey = @"FlareAccountIDToNameDictionary";
        private Dictionary<string, uint> m_dict = null;
        /* ThAW 2009/10/07 : Incomplete code; temporarily commented out.
        private Dictionary<string, uint> m_dictAccountNameToID = null;
        private Dictionary<uint, string> m_dictAccountIDToName = null;
         */

        protected Dictionary<string, uint> ContactNameToIDDictionary
        {
            get
            {
                return Session[m_strContactNameToIDDictionarySessionKey] as Dictionary<string, uint>;
            }
            set
            {
                Session[m_strContactNameToIDDictionarySessionKey] = value;
            }
        }

        protected Dictionary<string, uint> AccountNameToIDDictionary
        {
            get
            {
                return Session[m_strAccountNameToIDDictionarySessionKey] as Dictionary<string, uint>;
            }
            set
            {
                Session[m_strAccountNameToIDDictionarySessionKey] = value;
            }
        }

        protected Dictionary<uint, string> AccountIDToNameDictionary
        {
            get
            {
                return Session[m_strAccountIDToNameDictionarySessionKey] as Dictionary<uint, string>;
            }
            set
            {
                Session[m_strAccountIDToNameDictionarySessionKey] = value;
            }
        }

        protected Contact GetContactObjectBasedOnUI()   // Do not get the Contact object from the database.
        {
            UInt32 unSelectedContactID = 0;
            string strSelectedContactName = lbContactNames.SelectedValue;

            if (strSelectedContactName != null && m_dict.ContainsKey(strSelectedContactName))
            {
                unSelectedContactID = m_dict[strSelectedContactName];
            }

            Contact contact = new Contact();

            contact.ContactID = unSelectedContactID;
            contact.AccountID = Convert.ToUInt32(tbAccountID.Text);
            contact.Enabled = cbEnabled.Checked;
            contact.FirstName = tbFirstName.Text;
            contact.LastName = tbLastName.Text;
            contact.EmailAddress = tbEmailAddress.Text;

            contact.Name = contact.ConstructFullNamePlusID();

            return contact;
        }

        protected void ClearContactUIFields()
        {
            tbContactID.Text = string.Empty;
            tbAccountID.Text = string.Empty;
            cbEnabled.Checked = false;
            tbFirstName.Text = string.Empty;
            tbLastName.Text = string.Empty;
            tbEmailAddress.Text = string.Empty;

            lbContactNames.SelectedValue = null;
        }

        protected override void PageLoadWorker(object sender, EventArgs e)
        {
            bool bPopulateContactNamesListBox = false;

            // Ensure that m_dict is set.
            m_dict = ContactNameToIDDictionary;

            if (m_dict == null)
            {
                MySqlConnection con = GetMySqlConnection();

                m_dict = Contact.GetFullNamePlusIDToIDDictionary(con);
                ContactNameToIDDictionary = m_dict;
                bPopulateContactNamesListBox = true;
            }
            else if (!IsPostBack)
            {
                bPopulateContactNamesListBox = true;
            }

            if (bPopulateContactNamesListBox)
            {
                WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dict.Keys, lbContactNames.Items);
                lbContactNames.Items.Clear();

                foreach (string strName in m_dict.Keys)
                {
                    lbContactNames.Items.Add(strName);
                }
            }

            // ThAW 2009/10/07 : Incomplete feature; list box temporarily disabled.
            lbAccountNames.Enabled = false;
        }

        protected void lbContactNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedContactName = lbContactNames.SelectedValue;

            if (strSelectedContactName == null || !m_dict.ContainsKey(strSelectedContactName))
            {
                ClearContactUIFields();
                return;
            }

            UInt32 unSelectedContactID = m_dict[strSelectedContactName];
            MySqlConnection con = GetMySqlConnection();
            Contact contact = new Contact(con, unSelectedContactID);

            tbContactID.Text = contact.ContactID.ToString();
            tbAccountID.Text = contact.AccountID.ToString();
            cbEnabled.Checked = contact.Enabled;
            tbFirstName.Text = contact.FirstName;
            tbLastName.Text = contact.LastName;
            tbEmailAddress.Text = contact.EmailAddress;
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            ClearContactUIFields();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

            try
            {
                Contact contact = GetContactObjectBasedOnUI();

                // Ensure that the contact fields are valid; e.g. ensure that the strings contain no invalid characters such as the single quote.

                //if (!contact.Validate()) { }

                // Avoid account name collisions

                if (m_dict.ContainsKey(contact.Name) && m_dict[contact.Name] != contact.ContactID)
                {
                    throw new Exception(@"Cannot save: Another contact is using the same name");
                    //return;
                }

                MySqlConnection con = GetMySqlConnection();

                contact.SaveToDatabase(con);
                tbContactID.Text = contact.ContactID.ToString();
                contact.Name = contact.ConstructFullNamePlusID();   // Should this go in the Contact class?

                string strSelectedContactName = lbContactNames.SelectedValue;   // The old contact name, if the name was changed.

                if (strSelectedContactName != null && strSelectedContactName != contact.Name)
                {
                    lbContactNames.Items.Remove(strSelectedContactName);
                    m_dict.Remove(strSelectedContactName);
                }

                //if (!lbContactNames.Items.Contains(ListItem item))
                if (!m_dict.ContainsKey(contact.Name))
                {
                    lbContactNames.Items.Add(contact.Name);
                    m_dict.Add(contact.Name, contact.AccountID);
                }

                lbContactNames.SelectedValue = contact.Name;
            }
            catch (Exception ex)
            {
                SetStatusMessageLabel(ex);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            Contact contact = GetContactObjectBasedOnUI();

            if (contact.AccountID != 0)
            {
                string strSelectedContactName = lbContactNames.SelectedValue;

                if (strSelectedContactName != null)
                {
                    lbContactNames.Items.Remove(strSelectedContactName);
                    m_dict.Remove(strSelectedContactName);
                }

                MySqlConnection con = GetMySqlConnection();

                contact.DeleteFromDatabase(con);
            }

            ClearContactUIFields();
        }
    }
}

// **** End of File ****
