// FlareAdminWebApp Accounts.aspx.cs - By Tom Weatherhead - July 6, 2009

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
    public partial class AccountsPage : FlareAdminWebApp.Page  //System.Web.UI.Page
    {
        private const string m_strAccountNameToIDDictionarySessionKey = @"FlareAccountNameToIDDictionary";
        private Dictionary<string, UInt32> m_dict = null;

        protected Dictionary<string, UInt32> AccountNameToIDDictionary
        {
            get
            {
                return Session[m_strAccountNameToIDDictionarySessionKey] as Dictionary<string, UInt32>;
            }
            set
            {
                Session[m_strAccountNameToIDDictionarySessionKey] = value;
            }
        }

        protected Account GetAccountObjectBasedOnUI()   // Do not get the Account object from the database.
        {
            UInt32 unSelectedAccountID = 0;
            string strSelectedAccountName = lbAccountNames.SelectedValue;

            if (strSelectedAccountName != null && m_dict.ContainsKey(strSelectedAccountName))
            {
                unSelectedAccountID = m_dict[strSelectedAccountName];
            }

            Account acct = new Account();

            acct.AccountID = unSelectedAccountID;
            acct.Enabled = cbEnabled.Checked;
            acct.Name = tbName.Text;
            return acct;
        }

        protected void ClearAccountUIFields()
        {
            tbAccountID.Text = string.Empty;
            cbEnabled.Checked = false;
            tbName.Text = string.Empty;

            lbAccountNames.SelectedValue = null;
        }

        protected override void PageLoadWorker(object sender, EventArgs e)
        {
            bool bPopulateAccountNamesListBox = false;

            // Ensure that m_dict is set.
            m_dict = AccountNameToIDDictionary;

            if (m_dict == null)
            {
                MySqlConnection con = GetMySqlConnection();

                m_dict = NamedAndNumberedDatabaseObject.GetNameToIDDictionary(con, Account.SelectIDAndNameCommand);
                AccountNameToIDDictionary = m_dict;
                bPopulateAccountNamesListBox = true;
            }
            else if (!IsPostBack)
            {
                bPopulateAccountNamesListBox = true;
            }

            if (bPopulateAccountNamesListBox)
            {
                WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dict.Keys, lbAccountNames.Items);
            }
        }

        protected void lbAccountNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedAccountName = lbAccountNames.SelectedValue;

            if (strSelectedAccountName == null || !m_dict.ContainsKey(strSelectedAccountName))
            {
                ClearAccountUIFields();
                return;
            }

            UInt32 unSelectedAccountID = m_dict[strSelectedAccountName];
            MySqlConnection con = GetMySqlConnection();
            Account acct = new Account(con, unSelectedAccountID);

            tbAccountID.Text = acct.AccountID.ToString();
            cbEnabled.Checked = acct.Enabled;
            tbName.Text = acct.Name;
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            ClearAccountUIFields();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

            try
            {
                Account acct = GetAccountObjectBasedOnUI();

                // Avoid account name collisions

                if (m_dict.ContainsKey(acct.Name) && m_dict[acct.Name] != acct.AccountID)
                {
                    throw new Exception(@"Cannot save: Another account is using the specified name");
                }

                MySqlConnection con = GetMySqlConnection();

                acct.SaveToDatabase(con);
                tbAccountID.Text = acct.AccountID.ToString();

                string strSelectedAccountName = lbAccountNames.SelectedValue;   // The old account name, if the name was changed.

                if (strSelectedAccountName != null && strSelectedAccountName != acct.Name)
                {
                    //lbAccountNames.Items.Remove(strSelectedAccountName);
                    m_dict.Remove(strSelectedAccountName);
                }

                //if (!lbAccountNames.Items.Contains(ListItem item))
                if (!m_dict.ContainsKey(acct.Name))
                {
                    //lbAccountNames.Items.Add(acct.Name);
                    m_dict.Add(acct.Name, acct.AccountID);
                }

                WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dict.Keys, lbAccountNames.Items); // This ensured alphabetic sorting.
                lbAccountNames.SelectedValue = acct.Name;
            }
            catch (Exception ex)
            {
                SetStatusMessageLabel(ex);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {

            try
            {
                Account acct = GetAccountObjectBasedOnUI();

                if (acct.AccountID != 0)
                {
                    string strSelectedAccountName = lbAccountNames.SelectedValue;

                    if (strSelectedAccountName != null)
                    {
                        lbAccountNames.Items.Remove(strSelectedAccountName);
                        m_dict.Remove(strSelectedAccountName);
                    }

                    MySqlConnection con = GetMySqlConnection();

                    acct.DeleteFromDatabase(con);
                }

                ClearAccountUIFields();
            }
            catch (Exception ex)
            {
                SetStatusMessageLabel(ex);
            }
        }
    } // class AccountsPage
} // namespace FlareAdminWebApp
