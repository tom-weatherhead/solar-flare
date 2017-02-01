// FlareAdminWebApp Page.cs - By Tom Weatherhead - July 3, 2009

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Configuration;
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
    /// <summary>
    /// The base class for FlareAdminWebApp pages
    /// </summary>
    public abstract class Page : System.Web.UI.Page, IMySqlConnectionProvider
    {
        /*
        public Page()
        {
            //
            // TODO: Add constructor logic here
            //
        }
         */

        public MySqlConnection GetMySqlConnection()
        {
            //return MySqlConnectionProvider.GetFlareConnectionFromWebSession(Session);

            IMySqlConnectionProvider cp = new FlareWebSessionCachedMySqlConnectionProvider(Session);

            return cp.GetMySqlConnection();
        }

        protected void SetStatusMessageLabel(Exception ex)
        {

            if (!(Master is FlareAdminWebApp.MasterPage))
            {
                return;
            }

            FlareAdminWebApp.MasterPage mp = Master as FlareAdminWebApp.MasterPage;
            Label lblStatusMessage = mp.StatusMessageLabel;

            lblStatusMessage.Text = string.Format(@"{0} caught: {1}", ex.GetType().FullName, ex.Message);
        }

        protected abstract void PageLoadWorker(object sender, EventArgs e);

        protected void Page_Load(object sender, EventArgs e)
        {

            try
            {
                PageLoadWorker(sender, e);
            }
            catch (Exception ex)
            {
                SetStatusMessageLabel(ex);
            }
        }
    } // class Page
} // namespace FlareAdminWebApp

// **** End of File ****
