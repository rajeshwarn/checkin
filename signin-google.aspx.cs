using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Configuration;
using Newtonsoft.Json;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using IEEECheckin.ASPDocs.Models;

namespace IEEECheckin.ASPDocs
{
    public partial class google_signin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    GoogleOAuth2.GoogleAuthToken(Request, Response);

                    Response.Redirect("~/MemberPages/CreateCheckin.aspx");
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}