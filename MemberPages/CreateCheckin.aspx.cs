using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IEEECheckin.ASPDocs.MemberPages
{
    public partial class Menu : System.Web.UI.Page
    {
        public string MeetingNameStr { get { return MeetingName.Text; } set { MeetingName.Text = value; } }
        public string DropdownStr { get { return DropdownValue.Value; } set { DropdownValue.Value = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
    }
}