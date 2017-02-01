// FlareAdminWebApp MasterPage.cs - By Tom Weatherhead - September 1, 2009

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
    /// <summary>
    /// Summary description for MasterPage
    /// </summary>
    public abstract class MasterPage : System.Web.UI.MasterPage
    {
        public MasterPage()
            : base()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public abstract Label StatusMessageLabel { get; }
    } // class MasterPage
} // namespace FlareAdminWebApp

// **** End of File ****
