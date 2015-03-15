using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.IO;

using GDrive = Google.Apis.Drive.v2.Data;
using Google.Apis.Drive.v2;
using Google.GData.Spreadsheets;

using IEEECheckin.ASPDocs.Models;

namespace IEEECheckin.ASPDocs.MemberPages
{
    public partial class GoogleDocLoading : System.Web.UI.Page
    {

        public string SubmitDataStr { get { return SubmitData.Value; } set { SubmitData.Value = value; } }
        public string MeetingNameStr { get { return MeetingName.Value; } set { MeetingName.Value = value; } }
        public string MeetingDateStr { get { return MeetingDate.Value; } set { MeetingDate.Value = value; } }
        public string MeetingNameDate { get { return MeetingNameStr + " " + MeetingDateStr; } }
        public string FolderTreeXmlStr { get { return FolderTreeXml.Value; } set { FolderTreeXml.Value = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack)
            {
                ClientScript.RegisterHiddenField("isPostBack", "1");
            }
            else
            {
                if (PreviousPage != null)
                {
                    SubmitDataStr = PreviousPage.SubmitDataStr;
                    MeetingNameStr = PreviousPage.MeetingNameStr;
                    MeetingDateStr = PreviousPage.MeetingDateStr;
                }
            }
        }

        [WebMethod]
        public static string InitTreeView(string accessToken, string refreshToken)
        {
            try
            {
                // Make the Auth request to Google
                SpreadsheetsService sheetsService = GoogleOAuth2.GoogleAuthSheets(accessToken, refreshToken);
                if (sheetsService == null)
                {
                    return "";
                }

                // Get list of sheets
                DriveService driveService = GoogleOAuth2.GoogleAuthDrive(accessToken, refreshToken);
                if (driveService == null)
                {
                    return "";
                }
                List<GoogleSheet> sheetList = GoogleDriveHelpers.GoogleRetrieveAllSheets(sheetsService);
                GoogleFolder root = GoogleDriveHelpers.GoogleRetrieveSheetTree(driveService, null, ref sheetList);

                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(root.GetType());
                //using (var stream = new StreamWriter(@"", false))
                //{
                //    x.Serialize(stream, root);
                //}

                string xmlStr = GoogleDriveHelpers.SerializeXml<GoogleFolder>(root);
                xmlStr = xmlStr.Replace("<Children>", "").Replace("<Children />", "").Replace("</Children>", "");
                xmlStr = xmlStr.Replace("<Sheets>", "").Replace("<Sheets />", "").Replace("</Sheets>", "");

                return xmlStr;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

    }
}