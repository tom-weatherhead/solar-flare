// FlareAdminWebApp Site.master.cs - Stolen on August 31, 2009

using System;
using System.Collections;
using System.Configuration;
using System.Data;
//using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
//using System.Xml.Linq;

namespace FlareAdminWebApp
{
    public partial class SiteMasterPage : FlareAdminWebApp.MasterPage   //System.Web.UI.MasterPage
    {
        public override Label StatusMessageLabel
        {
            get
            {
                return lblStatusMessage;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            lblDateDisplay.Text = DateTime.Now.ToString(@"dddd, MMMM dd, yyyy");
        }
    } // class SiteMasterPage
} // namespace FlareAdminWebApp

// **** End of File ****
