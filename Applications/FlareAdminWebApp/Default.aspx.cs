// FlareAdminWebApp Default.aspx.cs - By Tom Weatherhead - June 26, 2009

#define USE_CONNECTION_PROVIDER
//#define SET_LABELS_FROM_SESSION
#define SolarFlareCommon

using SolarFlareCommon;
using MySql.Data.MySqlClient;
using System;
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
    public partial class DefaultPage : FlareAdminWebApp.Page   // System.Web.UI.Page
    {
        protected override void PageLoadWorker(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                tbServerName.Text = @"localhost";
                tbDatabaseName.Text = @"flare";
                tbUserID.Text = @"flare";
                tbPassword.Text = string.Empty;

                lblTest.Text = @"In Page_Load(); non-postback";
            }
            else
            {
#if SET_LABELS_FROM_SESSION
                string strMessage = Session[@"MySQLConnectionMessage"] as string;

                if (strMessage != null && Session[@"MySQLConnectionMessageColour"] is System.Drawing.Color)
                {
                    lblMessage.Text = strMessage;
                    lblMessage.ForeColor = (System.Drawing.Color)Session[@"MySQLConnectionMessageColour"];
                }

                string strInnerExceptionMessage = Session[@"MySQLConnectionInnerException"] as string;

                if (strInnerExceptionMessage != null)
                {
                    lblInnerExceptionMessage.Text = strInnerExceptionMessage;
                }
#endif

                lblTest.Text = @"In Page_Load(); postback";
            }
        }

        protected void btnConnect_Click(object sender, EventArgs e)
        {
#if SET_LABELS_FROM_SESSION
            Session[@"MySQLConnectionInnerException"] = @"None";
#endif

#if SolarFlareCommon
            lblInnerExceptionMessage.Text = @"None";
#endif

            try
            {
                // Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;
                string strConnectionString = string.Format(@"Server={0};Database={1};Uid={2};Pwd={3};",
                    tbServerName.Text, tbDatabaseName.Text, tbUserID.Text, tbPassword.Text);

                // "pooling=false;"
                // "Connect Timeout=20;"
                // "Data Source=localhost;"
                // "Port=MyPort;"

                // ThAW 2009/06/29 : Removing McAfee Internet Security (which included a WinSock filter)
                // and issuing the following command seemed to work:
                // The command: netsh winsock reset (see http://forums.mysql.com/read.php?38,200409,200478#msg-200478)
                string strServerVersionString = @"Version string not set yet";

#if USE_CONNECTION_PROVIDER
                //MySqlConnection con = MySqlConnectionProvider.GetFlareConnectionFromWebSession(Session);
                MySqlConnection con = GetMySqlConnection();

                strServerVersionString = string.Format(@"Connected to MySQL database server version {0}", con.ServerVersion);

                // Don't close or dispose of the connection; let either the session or the connection time out.
#else
                using (MySqlConnection con = new MySqlConnection(strConnectionString))
                {
                    con.Open();
                    strServerVersionString = string.Format(@"Connected to MySQL database server version {0}", con.ServerVersion);
                    con.Close();
                }
#endif

#if SET_LABELS_FROM_SESSION
                Session[@"MySQLConnectionMessage"] = strServerVersionString;
                Session[@"MySQLConnectionMessageColour"] = System.Drawing.Color.Green;
#endif

#if SolarFlareCommon
                lblMessage.Text = strServerVersionString;
                lblMessage.ForeColor = System.Drawing.Color.Green;
#endif
            }
            catch (Exception ex)
            {
#if SET_LABELS_FROM_SESSION
                Session[@"MySQLConnectionMessage"] = string.Format(@"{0} caught: {1}", ex.GetType().FullName, ex.Message);
                Session[@"MySQLConnectionMessageColour"] = System.Drawing.Color.Red;

                if (ex.InnerException != null)
                {
                    Session[@"MySQLConnectionInnerException"] = string.Format(@"{0}; {1}",
                        ex.InnerException.GetType().FullName, ex.InnerException.Message);
                }
#endif

#if SolarFlareCommon
                lblMessage.Text = string.Format(@"{0} caught: {1}", ex.GetType().FullName, ex.Message);
                lblMessage.ForeColor = System.Drawing.Color.Red;

                lblInnerExceptionMessage.Text = string.Format(@"{0}; {1}",
                    ex.InnerException.GetType().FullName, ex.InnerException.Message);
#endif
            }

            lblTest.Text = @"In btnConnect_Click()";

            // Force a reload of this page.
            //Response.Redirect(@"/FlareAdminWeb/Default.aspx");
        }
    } // class DefaultPage
} // namespace FlareAdminWebApp

// **** End of File ****
