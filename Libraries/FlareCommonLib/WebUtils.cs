// WebUtils.cs - Tom Weatherhead - September 11, 2009

using System;
using System.Collections.Generic;
//using System.Text;
//using System.Web.UI;
//using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;    // For DropDownList and ListItemCollection
//using System.Web.UI.WebControls.WebParts;

namespace SolarFlareCommon
{
    public static class WebUtils
    {
        public static void PopulateListItemCollectionFromStringEnumerable(IEnumerable<string> keys, ListItemCollection lic)
        {
            List<string> ls = new List<string>();   // ThAW 2009/09/11 : Can we pass the "keys" parameter directly to the constructor?

            foreach (string strKey in keys)
            {
                ls.Add(strKey);
            }

            ls.Sort();
            lic.Clear();

            foreach (string s in ls)
            {
                lic.Add(s);
            }
        }

        /*
        public static void PopulateDropDownListFromStringEnumerable(IEnumerable<string> keys, DropDownList ddl)
        {
            PopulateListItemCollectionFromStringEnumerable(keys, ddl.Items);
        }
        */
    }
}
