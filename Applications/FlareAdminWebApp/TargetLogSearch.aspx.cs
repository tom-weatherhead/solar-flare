// FlareAdminWeb TargetLogSearch.aspx.cs - By Tom Weatherhead - August 28, 2009

// ThAW 2009/09/23 : In the Page directive at the top of the .aspx file, ValidateRequest is set to "false";
// otherwise, ASP.NET throws an exception when reading the HTML in the search results' Message field,
// thinking that it is some form of attack (cross-site scripting?)

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
    public partial class TargetLogSearchPage : FlareAdminWebApp.Page  //System.Web.UI.Page
    {
        private const string m_strTargetNameToIDDictionarySessionKey = @"FlareTargetNameToIDDictionary";
        private const string m_strTargetIDToNameDictionarySessionKey = @"FlareTargetIDToNameDictionary";
        private const string m_strAccountNameToIDDictionarySessionKey = @"FlareAccountNameToIDDictionary";
        private const string m_strSearchResultsSessionKey = @"FlareTargetLogSearchResultsDictionary";
        private Dictionary<string, uint> m_dictTargets = null;  // Name to ID
        private Dictionary<uint, string> m_dictTargetsIDToName = null;
        private Dictionary<string, uint> m_dictAccounts = null;
        //private Dictionary<string, TargetLogRecord> m_dictSearchResults = null;

        protected Dictionary<string, uint> TargetNameToIDDictionary
        {
            get
            {
                return Session[m_strTargetNameToIDDictionarySessionKey] as Dictionary<string, uint>;
            }
            set
            {
                Session[m_strTargetNameToIDDictionarySessionKey] = value;
            }
        }

        protected Dictionary<uint, string> TargetIDToNameDictionary
        {
            get
            {
                return Session[m_strTargetIDToNameDictionarySessionKey] as Dictionary<uint, string>;
            }
            set
            {
                Session[m_strTargetIDToNameDictionarySessionKey] = value;
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

        protected Dictionary<string, TargetLogRecord> SearchResults
        {
            get
            {
                return Session[m_strSearchResultsSessionKey] as Dictionary<string, TargetLogRecord>;
            }
            set
            {
                Session[m_strSearchResultsSessionKey] = value;
            }
        }

        protected void ClearSelectedSearchResultFields()
        {
            lbSearchResultSummaries.SelectedValue = null;

            tbLogRecordID.Text = string.Empty;
            tbTargetID.Text = string.Empty;
            tbTimeStamp.Text = string.Empty;
            tbStatus.Text = string.Empty;
            tbMessage.Text = string.Empty;
            tbErrorCode.Text = string.Empty;
            tbLocationID.Text = string.Empty;
            tbResponseTime.Text = string.Empty;
        }

        protected override void PageLoadWorker(object sender, EventArgs e)
        {
            bool bPopulateTargetNamesListBox = false;

            // Ensure that m_dictTargets is set.
            m_dictTargets = TargetNameToIDDictionary;
            m_dictTargetsIDToName = TargetIDToNameDictionary;

            if (m_dictTargets == null)
            {
                MySqlConnection con = GetMySqlConnection();

                m_dictTargets = NamedAndNumberedDatabaseObject.GetNameToIDDictionary(con, Target.SelectIDAndNameCommand);
                TargetNameToIDDictionary = m_dictTargets;
                m_dictTargetsIDToName = null;   // Force a rebuild of the Targets ID To Name dictionary.
                bPopulateTargetNamesListBox = true;
            }
            else if (!IsPostBack)
            {
                bPopulateTargetNamesListBox = true;
            }

            if (bPopulateTargetNamesListBox)
            {
                WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dictTargets.Keys, lbTargetNames.Items);
            }

            if (m_dictTargetsIDToName == null)
            {
                /*
                m_dictTargetsIDToName = new Dictionary<uint, string>();

                foreach (KeyValuePair<string, uint> kvp in m_dictTargets)
                {
                    m_dictTargetsIDToName.Add(kvp.Value, kvp.Key);
                }
                 */
                m_dictTargetsIDToName = NamedAndNumberedDatabaseObject.GetIDToNameDictionary(m_dictTargets);

                TargetIDToNameDictionary = m_dictTargetsIDToName;
            }

            bool bPopulateAccountNamesListBox = false;

            // Ensure that m_dictAccounts is set.
            m_dictAccounts = AccountNameToIDDictionary;

            if (m_dictAccounts == null)
            {
                MySqlConnection con = GetMySqlConnection();

                m_dictAccounts = NamedAndNumberedDatabaseObject.GetNameToIDDictionary(con, Account.SelectIDAndNameCommand);
                AccountNameToIDDictionary = m_dictAccounts;
                bPopulateAccountNamesListBox = true;
            }
            else if (!IsPostBack)
            {
                bPopulateAccountNamesListBox = true;
            }

            if (bPopulateAccountNamesListBox)
            {
                WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dictAccounts.Keys, lbAccountNames.Items);
            }

            if (!IsPostBack)
            {
                string strNow = MySqlUtils.DateTimeToString(DateTime.UtcNow); //dtNow.ToString(@"yyyy-MM-dd HH:mm:ss");

                rbAny.Checked = true;
                lbTargetNames.Enabled = false;
                lbAccountNames.Enabled = false;

                tbStartTimeStamp.Enabled = false;
                tbStartTimeStamp.Text = strNow;
                tbEndTimeStamp.Enabled = false;
                tbEndTimeStamp.Text = strNow;
            }

            if (SearchResults == null)
            {
                ClearSelectedSearchResultFields();
            }
        }

        protected void rbTarget_CheckedChanged(object sender, EventArgs e)
        {

            if (rbTarget.Checked)
            {
                lbTargetNames.Enabled = true;
                rbAccount.Checked = false;
                lbAccountNames.Enabled = false;
                rbAny.Checked = false;
            }
        }

        protected void rbAccount_CheckedChanged(object sender, EventArgs e)
        {

            if (rbAccount.Checked)
            {
                rbTarget.Checked = false;
                lbTargetNames.Enabled = false;
                lbAccountNames.Enabled = true;
                rbAny.Checked = false;
            }
        }

        protected void rbAny_CheckedChanged(object sender, EventArgs e)
        {

            if (rbAny.Checked)
            {
                rbTarget.Checked = false;
                lbTargetNames.Enabled = false;
                rbAccount.Checked = false;
                lbAccountNames.Enabled = false;
            }
        }

        protected void cbStartTimeStamp_CheckedChanged(object sender, EventArgs e)
        {
            tbStartTimeStamp.Enabled = cbStartTimeStamp.Checked;
        }

        protected void cbEndTimeStamp_CheckedChanged(object sender, EventArgs e)
        {
            tbEndTimeStamp.Enabled = cbEndTimeStamp.Checked;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            uint unTargetID = 0;

            if (rbTarget.Checked)
            {
                string strSelectedTargetName = lbTargetNames.SelectedValue;

                if (strSelectedTargetName == null)
                {
                    throw new Exception(@"Cannot search by target: no target name is selected");
                }

                unTargetID = m_dictTargets[strSelectedTargetName];
            }

            uint unAccountID = 0;

            if (rbAccount.Checked)
            {
                string strSelectedAccountName = lbAccountNames.SelectedValue;

                if (strSelectedAccountName == null)
                {
                    throw new Exception(@"Cannot search by account: no account name is selected");
                }

                unAccountID = m_dictAccounts[strSelectedAccountName];
            }

            bool bFailedOnly = cbFailuresOnly.Checked;
            DateTime? ndtStartTimeStamp = null;

            if (cbStartTimeStamp.Checked)
            {
                ndtStartTimeStamp = DateTime.Parse(tbStartTimeStamp.Text);
            }

            DateTime? ndtEndTimeStamp = null;

            if (cbEndTimeStamp.Checked)
            {
                ndtEndTimeStamp = DateTime.Parse(tbEndTimeStamp.Text);
            }

            string strCommandText = TargetLogRecord.GetSelectCommand(unTargetID, unAccountID, bFailedOnly, ndtStartTimeStamp, ndtEndTimeStamp);
            MySqlConnection con = GetMySqlConnection();
            List<TargetLogRecord> lstSearchResults = TargetLogRecord.GetRecords(con, strCommandText);
            Dictionary<string, TargetLogRecord> dictSearchResults = new Dictionary<string, TargetLogRecord>();

            foreach (TargetLogRecord tlr in lstSearchResults)
            {
                uint unTargetID2 = tlr.TargetID;
                string strRecordSummary = string.Format(@"{0} {1} {2}",
                    //tlr.TimeStamp.ToString(@"yyyy-MM-dd HH:mm:ss"),
                    MySqlUtils.DateTimeToString(tlr.TimeStamp),
                    tlr.Status,
                    m_dictTargetsIDToName.ContainsKey(unTargetID2) ? m_dictTargetsIDToName[unTargetID2] : unTargetID2.ToString());

                dictSearchResults.Add(strRecordSummary, tlr);
            }

            // Store (lstSearchResults and) dictSearchResults in the Session.
            SearchResults = dictSearchResults;

            WebUtils.PopulateListItemCollectionFromStringEnumerable(dictSearchResults.Keys, lbSearchResultSummaries.Items);

            ClearSelectedSearchResultFields();
        }

        protected void lbSearchResultSummaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedSearchResultSummary = lbSearchResultSummaries.SelectedValue;

            if (strSelectedSearchResultSummary == null || !SearchResults.ContainsKey(strSelectedSearchResultSummary))
            {
                ClearSelectedSearchResultFields();
                return;
            }

            TargetLogRecord tlrSearchResult = SearchResults[strSelectedSearchResultSummary];

            tbLogRecordID.Text = tlrSearchResult.LogID.ToString();
            tbTargetID.Text = tlrSearchResult.TargetID.ToString();
            tbTimeStamp.Text = MySqlUtils.DateTimeToString(tlrSearchResult.TimeStamp);
            tbStatus.Text = tlrSearchResult.Status.ToString();
            tbMessage.Text = tlrSearchResult.Message;
            tbErrorCode.Text = tlrSearchResult.ErrorCode.ToString();
            tbLocationID.Text = tlrSearchResult.LocationID.ToString();
            tbResponseTime.Text = tlrSearchResult.ResponseTime.ToString();
        }
    } // class TargetLogSearchPage
} // namespace FlareAdminWebApp

// **** End of File ****
