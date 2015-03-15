using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Text;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using IEEECheckin.ASPDocs.Models;
using System.Web.Services;
using System.Net;


namespace IEEECheckin.ASPDocs.MemberPages
{
    public partial class Output : System.Web.UI.Page
    {

        public string SubmitDataStr { get { return SubmitData.Value; } set { SubmitData.Value = value; } }
        public string MeetingNameStr { get { return MeetingName.Value; } set { MeetingName.Value = value; } }
        public string MeetingDateStr { get { return MeetingDate.Value; } set { MeetingDate.Value = value; } }
        public string MeetingNameDate { get { return MeetingNameStr + " " + MeetingDateStr; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void SubmitCsv(object sender, EventArgs e)
        {
            try
            {
                string meetingName = "meeting_sign_ins";
                if (!String.IsNullOrWhiteSpace(MeetingNameDate))
                {
                    meetingName = MeetingNameDate;
                    meetingName = meetingName.Replace(' ', '_');
                }
                meetingName += ".csv";

                if (String.IsNullOrWhiteSpace(SubmitDataStr))
                {
                    return; // error
                }

                // clear http header
                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();

                // create file content
                XmlDocument xml = JsonConvert.DeserializeXmlNode(SubmitDataStr, "data", false);

                StringBuilder csvStr = new StringBuilder();

                csvStr.AppendLine("Student Id,First Name,Last Name,Email,Date,Meeting");

                foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                {
                    foreach (XmlNode elem in node.ChildNodes)
                    {
                        csvStr.Append(elem.FirstChild.Value + ",");
                    }
                    csvStr.AppendLine();
                }

                // add header to response
                Response.AddHeader("Content-Disposition", "attachment; filename=" + meetingName);
                Response.AddHeader("Content-Length", csvStr.Length.ToString());
                Response.ContentType = "text/plain";

                // write output to response
                Response.Flush();
                Response.Output.Write(csvStr.ToString());
                Response.End();
            }
            catch(Exception ex)
            {

            }
        }

        protected void SubmitJson(object sender, EventArgs e)
        {
            try
            {
                string meetingName = "meeting_sign_ins";
                if (!String.IsNullOrWhiteSpace(MeetingNameDate))
                {
                    meetingName = MeetingNameDate;
                    meetingName = meetingName.Replace(' ', '_');
                }
                meetingName += ".js";

                if (String.IsNullOrWhiteSpace(SubmitDataStr))
                {
                    return; // error
                }

                // clear http header
                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();

                // add header to response
                Response.AddHeader("Content-Disposition", "attachment; filename=" + meetingName);
                Response.AddHeader("Content-Length", (SubmitDataStr).Length.ToString());
                Response.ContentType = "text/plain";

                // write output to response
                Response.Flush();
                Response.Output.Write(SubmitDataStr);
                Response.End();
            }
            catch(Exception ex)
            {

            }
        }
    }
}