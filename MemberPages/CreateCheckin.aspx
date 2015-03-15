<%@ Page Title="Create Check-in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CreateCheckin.aspx.cs" Inherits="IEEECheckin.ASPDocs.MemberPages.Menu" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h1 class="post-header">Welcome</h1>
    <p class="text-center">Create a new meeting or select one of your previous meetings to begin.</p>

    <p class="section-header">Meeting Options <i class="fa fa-list"></i></p>  
    <div class="boxed-section margin-lg-after">
        <p class="section-label">Select Existing Meeting<i class="fa fa-calendar-o"></i></p>
        <select class="selectpicker" id="meetingDropdown" name="meeting"></select>
        <p class="section-label">Create New Meeting<i class="fa fa-pencil"></i></p>
        <asp:TextBox CssClass="form-control input-lg margin-sm-after" ID="MeetingName" placeholder="Meeting Name" runat="server" />
        <asp:HiddenField ID="DropdownValue" runat="server" />
        <asp:Button CssClass="form-control input-lg btn btn-info check-in" ID="MeetingButton" OnClientClick="return meetingSubmit();" Text="Start Meeting" runat="server" />
        <asp:Button CssClass="hidden" ID="MeetingButtonHidden" PostBackUrl="~/MemberPages/Checkin.aspx" Text="Start Meeting" runat="server" />

    </div>

</asp:Content>
<asp:Content ID="JavaScriptContent" ContentPlaceHolderID="JavaScripts" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            $("#MainContent_MeetingName").keydown(function (e) {
                var charCode = e.charCode || e.keyCode || e.which;
                if (charCode == 13) {
                    meetingSubmit();
                }
            });   
        });
        function dbOpenCallback() {
            try {
                getMeetings();
            }
            catch (err) {
                alert(err.message);
            }
        }
        function meetingCallback(meetings) {
            var htmlVal = "";
            for(var i = 0; i < meetings.length; i++) {
                htmlVal = htmlVal + "<option value='" + "meeting=" + meetings[i].meeting + "&" + "date=" + meetings[i].date + "'>" + meetings[i].meeting + " " + meetings[i].date + "</option>";
            }

            $("#meetingDropdown").html("<option value=''>New Meetings</option>" + htmlVal);
        }

        function meetingSubmit() {
            var dropdown = $("#meetingDropdown option:selected").val();
            var meeting = $("#MainContent_MeetingName").val();
            $("#MainContent_DropdownValue").val(dropdown);
            if ((dropdown == null || dropdown == undefined || dropdown.trim() === "") && (meeting == null || meeting == undefined || meeting.trim() === "")) {
                alert("Must select a meeting or create a new one.")
                return false;
            }

            $("#MainContent_MeetingButtonHidden").click();
            return false;
        }

        function decodeMeeting(meeting) {
            var meet = "", date = "";

            if (meeting !== null && meeting !== undefined && meeting.trim() !== "") {
                var sURLVariables = meeting.split('&');
                for (var i = 0; i < sURLVariables.length; i++) {
                    var sParameterName = sURLVariables[i].split('=');
                    if (sParameterName[0] == "meeting") {
                        meet = sParameterName[1];
                    }
                }
                for (var i = 0; i < sURLVariables.length; i++) {
                    var sParameterName = sURLVariables[i].split('=');
                    if (sParameterName[0] == "date") {
                        date = sParameterName[1];
                    }
                }
            }

            return { "name": meet, "date": date };
        }

    </script>
</asp:Content>
