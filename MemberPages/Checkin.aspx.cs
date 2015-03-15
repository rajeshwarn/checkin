using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IEEECheckin.ASPDocs.MemberPages
{
    public partial class Checkin : System.Web.UI.Page
    {
        private const string _dateFormat = "yyyy-MM-dd";

        public string MeetingNameStr { get { return MeetingName.Value; } set { MeetingName.Value = value; } }
        public string MeetingDateStr { get { return MeetingDate.Value; } set { MeetingDate.Value = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (PreviousPage != null)
                {
                    if (!String.IsNullOrWhiteSpace(PreviousPage.MeetingNameStr))
                    {
                        MeetingNameStr = PreviousPage.MeetingNameStr;
                        MeetingDateStr = DateTime.Now.ToString(_dateFormat);
                    }
                    else
                    {
                        NameValueCollection col = HttpUtility.ParseQueryString(PreviousPage.DropdownStr);
                        MeetingNameStr = col.Get("meeting");
                        MeetingDateStr = col.Get("date");
                    }
                }
            }
        }
    }
}