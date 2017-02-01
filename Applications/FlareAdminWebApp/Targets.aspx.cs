// FlareAdminWeb Targets.aspx.cs - By Tom Weatherhead - July 28, 2009

using FlareLib;
using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
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
    public partial class TargetsPage : FlareAdminWebApp.Page  //System.Web.UI.Page
    {
        private const string m_strTargetNameToIDDictionarySessionKey = @"FlareTargetNameToIDDictionary";
        private const string m_strTargetFormFieldNameToIDDictionarySessionKey = @"FlareTargetFormFieldNameToIDDictionary";
        private Dictionary<string, UInt32> m_dict = null;
        private Dictionary<string, UInt32> m_dictTargetFormFields = null;

        protected Dictionary<string, UInt32> TargetNameToIDDictionary
        {
            get
            {
                return Session[m_strTargetNameToIDDictionarySessionKey] as Dictionary<string, UInt32>;
            }
            set
            {
                Session[m_strTargetNameToIDDictionarySessionKey] = value;
            }
        }

        protected Dictionary<string, UInt32> TargetFormFieldNameToIDDictionary
        {
            get
            {
                return Session[m_strTargetFormFieldNameToIDDictionarySessionKey] as Dictionary<string, UInt32>;
            }
            set
            {
                Session[m_strTargetFormFieldNameToIDDictionarySessionKey] = value;
            }
        }

        protected uint GetTargetIDBasedOnUI()
        {
            uint unSelectedTargetID = 0;
            string strSelectedTargetName = lbTargetNames.SelectedValue;

            if (strSelectedTargetName != null && m_dict.ContainsKey(strSelectedTargetName))
            {
                unSelectedTargetID = m_dict[strSelectedTargetName];
            }

            return unSelectedTargetID;
        }

        protected Target GetTargetObjectBasedOnUI()   // Do not get the Target object (except for the TargetFormFields) from the database.
        {
            Target target = new Target();

            target.TargetID = GetTargetIDBasedOnUI();
            target.AccountID = Convert.ToUInt32(tbAccountID.Text);
            target.Enabled = cbEnabled.Checked;
            target.Name = tbName.Text;
            target.URL = tbURL.Text;
            target.MonitorIntervalAsInt = Convert.ToInt32(tbMonitorInterval.Text);
            //target.LastMonitoredAt = ;

            MonitorTypeCollectionMember mtcm = MonitorTypeCollection.FindUsingUIString(ddlMonitorType.SelectedValue);

            if (mtcm != null)
            {
                target.MonitorType = mtcm.MonitorType;
            }
            else
            {
                target.MonitorType = Target.DefaultMonitorType;
            }

            target.LastTargetLogID = Convert.ToUInt32(tbLastTargetLogID.Text);
            //target.TargetAddedAt = ;
            //target.LastFailedAt = ;

            MySqlConnection con = GetMySqlConnection();

            target.FinishConstruction(con);

            return target;
        }

        protected Dictionary<string, UInt32> GenerateTargetFormFieldNameToIDDictionary(UInt32 unTargetID)
        {

            if (unTargetID == 0)
            {
                return null;
            }

            MySqlConnection con = GetMySqlConnection();
            string strCommandText = string.Format(@"SELECT FieldID, FieldName FROM TargetFormFields WHERE TargetID={0};", unTargetID);

            return NamedAndNumberedDatabaseObject.GetNameToIDDictionary(con, strCommandText);
        }

        protected uint GetTargetFormFieldIDBasedOnUI()
        {
            uint unSelectedTargetFormFieldID = 0;
            string strSelectedTargetFormFieldName = lbTargetFormFieldNames.SelectedValue;

            if (strSelectedTargetFormFieldName != null && m_dictTargetFormFields.ContainsKey(strSelectedTargetFormFieldName))
            {
                unSelectedTargetFormFieldID = m_dictTargetFormFields[strSelectedTargetFormFieldName];
            }

            return unSelectedTargetFormFieldID;
        }

        protected void ClearTargetFormFieldFields()
        {
            tbFieldID.Text = @"0";
            tbFieldName.Text = string.Empty;
            tbFieldValue.Text = string.Empty;
        }

        protected void ClearTargetFields()
        {
            tbTargetID.Text = @"0";
            tbAccountID.Text = @"0";
            cbEnabled.Checked = false;
            tbName.Text = string.Empty;
            tbURL.Text = string.Empty;
            tbMonitorInterval.Text = @"0";
            tbLastMonitoredAt.Text = string.Empty;
            SetMonitorTypeDDLToDefault();
            tbLastTargetLogID.Text = @"0";
            tbTargetAddedAt.Text = string.Empty;
            tbLastFailedAt.Text = string.Empty;

            lbTargetFormFieldNames.Items.Clear();

            ClearTargetFormFieldFields();
        }

        protected void SetMonitorTypeDDL(MonitorType mt)
        {
            MonitorTypeCollectionMember mtcm = MonitorTypeCollection.FindUsingMonitorType(mt);

            ddlMonitorType.SelectedValue = mtcm.UIString;
        }

        protected void SetMonitorTypeDDLToDefault()
        {
            SetMonitorTypeDDL(Target.DefaultMonitorType);
        }

        protected void EnableOrDisableFormFieldButtons()
        {
            uint unTargetID = GetTargetIDBasedOnUI();
            bool bEnable = (unTargetID != 0);

            btnNewField.Enabled = bEnable;
            btnSaveField.Enabled = bEnable;
            btnDeleteField.Enabled = bEnable;
        }

        protected override void PageLoadWorker(object sender, EventArgs e)
        {
            bool bPopulateTargetNamesListBox = false;

            // Ensure that m_dict is set.
            m_dict = TargetNameToIDDictionary;

            if (m_dict == null)
            {
                MySqlConnection con = GetMySqlConnection();

                m_dict = NamedAndNumberedDatabaseObject.GetNameToIDDictionary(con, Target.SelectIDAndNameCommand);
                TargetNameToIDDictionary = m_dict;
                bPopulateTargetNamesListBox = true;
            }
            else if (!IsPostBack)
            {
                bPopulateTargetNamesListBox = true;
            }

            if (bPopulateTargetNamesListBox)
            {
                WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dict.Keys, lbTargetNames.Items);
            }

            m_dictTargetFormFields = TargetFormFieldNameToIDDictionary;

            if (m_dictTargetFormFields == null)
            {
                UInt32 unTargetID = GetTargetIDBasedOnUI();

                m_dictTargetFormFields = GenerateTargetFormFieldNameToIDDictionary(unTargetID);
                TargetFormFieldNameToIDDictionary = m_dictTargetFormFields;
            }

            if (!IsPostBack)
            {
                ddlMonitorType.Items.Clear();

                foreach (MonitorTypeCollectionMember mtcm in MonitorTypeCollection.GetEnumerable())
                {
                    ddlMonitorType.Items.Add(mtcm.UIString);
                }

                SetMonitorTypeDDLToDefault();
            }
        }

        protected void lbTargetNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedTargetName = lbTargetNames.SelectedValue;

            if (strSelectedTargetName == null || !m_dict.ContainsKey(strSelectedTargetName))
            {
                ClearTargetFields();
                return;
            }

            ClearTargetFormFieldFields();

            UInt32 unSelectedTargetID = m_dict[strSelectedTargetName];
            MySqlConnection con = GetMySqlConnection();
            Target target = new Target(con, unSelectedTargetID);

            tbTargetID.Text = target.TargetID.ToString();
            tbAccountID.Text = target.AccountID.ToString();
            cbEnabled.Checked = target.Enabled;
            tbName.Text = target.Name;
            tbURL.Text = target.URL;
            tbMonitorInterval.Text = target.MonitorIntervalAsInt.ToString();
            tbLastMonitoredAt.Text = target.LastMonitoredAtAsUIString;
            SetMonitorTypeDDL(target.MonitorType);
            tbLastTargetLogID.Text = target.LastTargetLogID.ToString();
            tbTargetAddedAt.Text = target.TargetAddedAt.ToString(); // TargetAddedAtAsUIString; or DateTimeToUIString(...);
            tbLastFailedAt.Text = target.LastFailedAtAsUIString;

            m_dictTargetFormFields = GenerateTargetFormFieldNameToIDDictionary(target.TargetID);
            TargetFormFieldNameToIDDictionary = m_dictTargetFormFields;

            WebUtils.PopulateListItemCollectionFromStringEnumerable(m_dictTargetFormFields.Keys, lbTargetFormFieldNames.Items);
        }

        protected void lbTargetFormFieldNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedTargetFormFieldName = lbTargetFormFieldNames.SelectedValue;

            if (strSelectedTargetFormFieldName == null || !m_dictTargetFormFields.ContainsKey(strSelectedTargetFormFieldName))
            {
                ClearTargetFormFieldFields();
                return;
            }

            UInt32 unSelectedTargetID = GetTargetIDBasedOnUI();
            MySqlConnection con = GetMySqlConnection();
            Target target = new Target(con, unSelectedTargetID);
            UInt32 unSelectedFieldID = m_dictTargetFormFields[strSelectedTargetFormFieldName];
            TargetFormField field = target.FormFields.FindByFieldID(unSelectedFieldID);

            tbFieldID.Text = field.FieldID.ToString();
            tbFieldName.Text = field.FieldName;
            tbFieldValue.Text = field.FieldValue;
        }

        // Field buttons

        protected void btnNewField_Click(object sender, EventArgs e)
        {
            ClearTargetFormFieldFields();

            lbTargetFormFieldNames.SelectedValue = null;
        }

        protected void btnSaveField_Click(object sender, EventArgs e)
        {

            try
            {
                string strFieldName = tbFieldName.Text.Trim();
                string strFieldValue = tbFieldValue.Text.Trim();

                if (strFieldName == string.Empty || strFieldValue == string.Empty)
                {
                    return;
                }

                if (GetTargetIDBasedOnUI() == 0)
                {
                    return;
                }

                Target target = GetTargetObjectBasedOnUI();
                //string strFieldID = tbFieldID.Text.Trim();
                bool bSave = false;
                bool bNewOrNameChanged = false;
                string strOldFieldName = null;
                uint unFieldID = GetTargetFormFieldIDBasedOnUI();
                TargetFormField tff1 = null;

                //if (strFieldID == string.Empty)
                if (unFieldID == 0)
                {
                    tff1 = new TargetFormField(0, target.TargetID, strFieldName, strFieldValue);
                    target.FormFields.Add(tff1);
                    bSave = true;
                    bNewOrNameChanged = true;
                }
                else
                {
                    //uint unFieldID = Convert.ToUInt32(strFieldID);
                    tff1 = target.FormFields.FindByFieldID(unFieldID);

                    if (tff1 != null)
                    {
                        string strOldFieldValue = tff1.FieldValue;

                        strOldFieldName = tff1.FieldName;
                        bNewOrNameChanged = (strFieldName != strOldFieldName);

                        if (bNewOrNameChanged || strFieldValue != strOldFieldValue)
                        {
                            tff1.FieldName = strFieldName;
                            tff1.FieldValue = strFieldValue;
                            tff1.ToBeUpdated = true;
                            bSave = true;
                        }
                    }
                }

                if (!bSave)
                {
                    return;
                }

                if (bNewOrNameChanged && m_dictTargetFormFields.ContainsKey(strFieldName))
                {
                    throw new Exception(string.Format(@"Cannot save target form field: There is already another target form field named '{0}'.", strFieldName));
                }

                tff1.ValidateDataBeforeSave();

                MySqlConnection con = GetMySqlConnection();

                target.FormFields.SaveToDatabase(con);

                TargetFormField tff2 = target.FormFields.FindByFieldName(strFieldName);

                // Update the text box tbFieldID
                tbFieldID.Text = tff2.FieldID.ToString();

                if (strOldFieldName == null || strOldFieldName != strFieldName)
                {
                    // Update the list box lbTargetFormFieldNames

                    if (strOldFieldName != null)
                    {
                        ListItem li = new ListItem(strOldFieldName);

                        if (lbTargetFormFieldNames.Items.Contains(li))
                        {
                            lbTargetFormFieldNames.Items.Remove(strOldFieldName);
                        }
                    }

                    lbTargetFormFieldNames.Items.Add(strFieldName);
                    lbTargetFormFieldNames.SelectedValue = strFieldName;

                    // Update the dictionary m_dictTargetFormFields

                    if (m_dictTargetFormFields != null)
                    {

                        if (strOldFieldName != null && m_dictTargetFormFields.ContainsKey(strOldFieldName))
                        {
                            m_dictTargetFormFields.Remove(strOldFieldName);
                        }

                        m_dictTargetFormFields.Add(strFieldName, tff2.FieldID);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatusMessageLabel(ex);
            }
        }

        protected void btnDeleteField_Click(object sender, EventArgs e)
        {

            if (GetTargetIDBasedOnUI() == 0)
            {
                return;
            }

            Target target = GetTargetObjectBasedOnUI();
            //string strFieldID = tbFieldID.Text.Trim();
            //uint unFieldID = Convert.ToUInt32(strFieldID);
            uint unFieldID = GetTargetFormFieldIDBasedOnUI();
            TargetFormField tff = target.FormFields.FindByFieldID(unFieldID);

            if (tff != null)
            {
                string strOldFieldName = tff.FieldName;

                tff.ToBeDeleted = true;

                MySqlConnection con = GetMySqlConnection();

                target.FormFields.SaveToDatabase(con);

                // Update the target form field fields on the Web form (all are text boxes)
                ClearTargetFormFieldFields();

                // Update the list box lbTargetFormFieldNames

                if (strOldFieldName != null)    // This test may be unnecessary
                {
                    ListItem li = new ListItem(strOldFieldName);

                    if (lbTargetFormFieldNames.Items.Contains(li))
                    {
                        lbTargetFormFieldNames.Items.Remove(strOldFieldName);
                    }
                }

                lbTargetFormFieldNames.SelectedValue = null;

                // Update the dictionary m_dictTargetFormFields

                if (m_dictTargetFormFields != null && strOldFieldName != null && m_dictTargetFormFields.ContainsKey(strOldFieldName))
                {
                    m_dictTargetFormFields.Remove(strOldFieldName);
                }
            }
        }

        // Target buttons

        protected void btnNewTarget_Click(object sender, EventArgs e)
        {
            ClearTargetFields();
            m_dictTargetFormFields.Clear();

            lbTargetNames.SelectedValue = null;
        }

        protected void btnSaveTarget_Click(object sender, EventArgs e)
        {

            try
            {
                // ThAW 2009/08/19 TO_DO : If this target has not been saved before, or if the name is being changed,
                // ensure that the new name is not already being used by another target.  Also, perform the same check for target form fields.

                Target target = GetTargetObjectBasedOnUI();
                MySqlConnection con = GetMySqlConnection();
                uint unTargetID = GetTargetIDBasedOnUI();
                bool bSave = false;
                bool bNewOrNameChanged = false;

                if (unTargetID == 0)
                {
                    bSave = true;
                    bNewOrNameChanged = true;
                }
                else
                {
                    Target targetInDatabase = new Target(con, unTargetID);

                    bNewOrNameChanged = (target.Name != targetInDatabase.Name);

                    if (target.AccountID != targetInDatabase.AccountID ||
                        target.Enabled != targetInDatabase.Enabled ||
                        bNewOrNameChanged ||    // I.e. target name changed
                        target.URL != targetInDatabase.URL ||
                        target.MonitorIntervalAsInt != targetInDatabase.MonitorIntervalAsInt ||
                        target.LastMonitoredAt != targetInDatabase.LastMonitoredAt ||
                        target.MonitorType != targetInDatabase.MonitorType ||
                        target.LastTargetLogID != targetInDatabase.LastTargetLogID ||
                        target.TargetAddedAt != targetInDatabase.TargetAddedAt ||
                        target.LastFailedAt != targetInDatabase.LastFailedAt)
                    {
                        bSave = true;
                    }
                }

                if (!bSave)
                {
                    return;
                }

                if (bNewOrNameChanged && m_dict.ContainsKey(target.Name))
                {
                    throw new Exception(string.Format(@"Cannot save target: There is already another target named '{0}'.", target.Name));
                }

                target.SaveToDatabase(con);

                // Update the text box tbTargetID
                tbTargetID.Text = target.TargetID.ToString();

                // Update the list box lbTargetNames
                string strOldTargetName = lbTargetNames.SelectedValue;

                if (target.Name != strOldTargetName)
                {

                    if (strOldTargetName != null)
                    {
                        lbTargetNames.Items.Remove(strOldTargetName);
                    }

                    lbTargetNames.Items.Add(target.Name);
                    lbTargetNames.SelectedValue = target.Name;
                }

                // Update the dictionary m_dict

                if (m_dict != null)
                {

                    if (strOldTargetName != null && m_dict.ContainsKey(strOldTargetName))
                    {
                        m_dict.Remove(strOldTargetName);
                    }

                    m_dict.Add(target.Name, target.TargetID);
                }
            }
            catch (Exception ex)
            {
                SetStatusMessageLabel(ex);
            }
        }

        protected void btnDeleteTarget_Click(object sender, EventArgs e)
        {
            uint unTargetID = GetTargetIDBasedOnUI();

            if (unTargetID == 0)
            {
                // This target is not in the database; there is nothing to delete.
                return;
            }

            Target target = GetTargetObjectBasedOnUI();
            MySqlConnection con = GetMySqlConnection();

            target.DeleteFromDatabase(con);

            ClearTargetFields();
            m_dictTargetFormFields.Clear();

            string strSelectedTargetName = lbTargetNames.SelectedValue;

            if (strSelectedTargetName == null)
            {
                return;
            }

            // Update the list box lbTargetNames
            lbTargetNames.Items.Remove(strSelectedTargetName);
            lbTargetNames.SelectedValue = null;

            // Update the dictionary m_dict

            if (m_dict.ContainsKey(strSelectedTargetName))
            {
                m_dict.Remove(strSelectedTargetName);
            }
        }
    } // class TargetsPage
} // namespace FlareAdminWebApp

// **** End of File ****
