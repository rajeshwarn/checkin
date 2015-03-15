<%@ Page Title="Google Docs Loading" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" validateRequest="false" CodeBehind="GoogleDocLoading.aspx.cs" Inherits="IEEECheckin.ASPDocs.MemberPages.GoogleDocLoading" %>

<%@ PreviousPageType VirtualPath="~/MemberPages/Attendance.aspx" %> 

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .hide {
            display: none;
        }
    </style>
    <script type="text/javascript">
        function googleClick() {
            $("#MainContent_GoogleButton").click();
        }
    </script>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="post-header">Loading Google Docs</h1>
    <h2 id="meetingName" class="post-header"></h2>

    <asp:HiddenField ID="SubmitData" runat="server" />
    <asp:HiddenField ID="MeetingName" runat="server" />
    <asp:HiddenField ID="MeetingDate" runat="server" />
    <asp:HiddenField ID="FolderTreeXml" runat="server" />

    <asp:Image CssClass="float-center shaded" ID="LoadingImage" ImageUrl="~/Images/ajax-loader-dark.gif" runat="server" />
    <br /><br />
    <p>We are currently getting a list of your spreadsheets, please be patient as this may take some time.</p>
    <p>You will automatically be taken to a list of your spreadsheets. Do not refresh or navigate away from this page.</p>
    
    <asp:Button CssClass="hide" PostBackUrl="~/MemberPages/GoogleDocSelect.aspx" ID="GoogleButton" runat="server" />

</asp:Content>
<asp:Content ID="JavaScriptContent" ContentPlaceHolderID="JavaScripts" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            var d = new Date();
            var day = d.getDate();
            if (day < 10)
                day = "0" + day;
            var month = (d.getMonth() + 1);
            if (month < 10)
                month = "0" + month;
            var date = d.getFullYear() + "-" + month + "-" + day;
            var meetingName = $("#MainContent_MeetingName").val() + " " + $("#MainContent_MeetingDate").val();
            if (meetingName == null || meetingName.trim() === "")
                meetingName = "all meetings " + date;
            $("#meetingName").html(meetingName);

            if (!isPostBack()) {
                var dataVal = JSON.stringify({ "accessToken": $.cookie("GoogleTokenCode"), "refreshToken": $.cookie("GoogleRefreshCode") });
                $.ajax({
                    type: "POST",
                    url: "GoogleDocLoading.aspx/InitTreeView",
                    data: dataVal,
                    contentType: 'application/json; charset=utf-8',
                    dataType: "json",
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        alert("Request: " + XMLHttpRequest.toString() + "\n\nStatus: " + textStatus + "\n\nError: " + errorThrown);
                        returnValue = false;
                    },
                    success: function (data) {
                        if (data.d === "") {
                            alert("Could not retrieve Google Docs. Try logging off and back in again.");
                            window.history.back();
                        }
                        else {
                            $("#MainContent_FolderTreeXml").val(data.d);
                            googleClick();
                        }
                    }
                });
            }
        });
        function isPostBack() { //function to check if page is a postback-ed one
            var isPostBackObject = document.getElementById('isPostBack');
            if (isPostBackObject != null)
                return true;
            else
                return false;
        }
    </script>
</asp:Content>
