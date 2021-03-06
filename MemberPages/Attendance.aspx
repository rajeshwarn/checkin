﻿<%@ Page Title="Attendance" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="IEEECheckin.ASPDocs.MemberPages.Output" %>
<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        td, th {
            padding-left: 10px;
            padding-right: 10px;
            width: 250px;
        }
        th {
            padding-bottom: 10px;
        }
        td {
            padding-bottom: 5px;
        }
        .col-custom-offset {
            margin-left: 12.5%;
        }
        .col-custom {
            width: 75.0%;
        }
        #output {
            background: white;
            color: black;
            border: 1px solid;
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="post-header">Meeting Attendance</h1>

    <p class="section-header">Filter Meeting Attendance <i class="fa fa-file-text"></i></p> 
    <div class="boxed-section margin-lg-after">
        <select class="selectpicker" id="ouputDropdown" onchange="selectionChanged()"></select>
    </div>

    <p class="section-header">Export <i class="fa fa-download"></i></p> 
    <div class="boxed-section margin-lg-after">
        <asp:HiddenField ID="SubmitData" runat="server" />
        <asp:HiddenField ID="MeetingName" runat="server" />
        <asp:HiddenField ID="MeetingDate" runat="server" />
        <asp:Button CssClass="form-control input-lg btn btn-info check-in" OnClick="SubmitCsv" Text="Get CSV" id="CsvButton" runat="server" />
        <br /><br />
        <asp:Button CssClass="form-control input-lg btn btn-info check-in" OnClick="SubmitJson" Text="Get JSON" id="JsonButton" runat="server" />
        <br /><br />
        <asp:Button CssClass="form-control input-lg btn btn-info check-in" PostBackUrl="~/MemberPages/GoogleDocLoading.aspx" Text="Add to Google" id="GoogleButton" runat="server" />
    </div>

    <p class="section-header">Clear Attendance <i class="fa fa-eraser"></i></p>
    <div class="boxed-section margin-lg-after">
        <asp:Button CssClass="form-control input-lg btn btn-info check-in" OnClientClick="return clearSubmit();" ID="ClearButton" Text="Clear" runat="server" />
    </div>

    <p class="section-header">Lottery <i class="fa fa-random"></i></p>
    <div class="boxed-section margin-lg-after">
        <asp:Button CssClass="form-control input-lg btn btn-info check-in" OnClientClick="return lotteryRoll();" ID="RandomButton" Text="Roll" runat="server" />
        <p class="text-center">And the winner is...</p>
        <div class="text-center" id="WinnerBox"></div>
    </div>

    </div><!--Not an orphaned /div in VS, just starts on the master page. Same for closing tag of div below.-->
    <div class="col-custom-offset col-custom col-lg-4 col-lg-offset-4 col-md-6 col-md-offset-3 col-sm-8 col-sm-offset-2 col-xs-12">
    <div id="outputDiv">
        <table id="output">
            <tbody>
                <tr id="headerRow">
                    <th>Index</th>
                    <th>First Name</th>
                    <th>Last Name</th>
                    <th>Student ID</th>
                    <th>Email</th>
                    <th>Meeting</th>
                    <th>Date</th>
                </tr>
            </tbody>
        </table>
</asp:Content>
<asp:Content ID="JavaScriptContent" ContentPlaceHolderID="JavaScripts" runat="server">
    <script type="text/javascript">
        // callback for when database is opened
        function dbOpenCallback() {
            try {
                getMeetings(); // get the meetings in the database
            }
            catch (err) {
                alert(err.message);
            }
        }
        // callback for when the json is ready
        function jsonCallback(outputVal) {
            var decoded = decodeMeeting($("#ouputDropdown option:selected").val());
            var data = { "data": outputVal };
            var dataVal = JSON.stringify(data);
            $("#MainContent_SubmitData").val(dataVal);
            $("#MainContent_MeetingName").val(decoded["name"]);
            $("#MainContent_MeetingDate").val(decoded["date"]);
        }
        // callback for when the list of meetings is ready, populate meeting selection dropdown
        function meetingCallback(meetings) {
            var htmlVal = "";
            for (var i = 0; i < meetings.length; i++) {
                htmlVal = htmlVal + "<option value='" + "meeting=" + meetings[i].meeting + "&" + "date=" + meetings[i].date + "'>" + meetings[i].meeting + " " + meetings[i].date + "</option>";
            }

            htmlVal = "<option value='meeting=&date='>All Meetings</option>" + htmlVal;
            $("#ouputDropdown").html(htmlVal);

            // initially populated data with all meetings and dates
            var decoded = decodeMeeting($("#ouputDropdown option:selected").val());
            createOutput("#output tr:last", decoded["name"], decoded["date"]);
            getJson("", "");
        }
        // event handler for when meeting dropdown selection is changed
        function selectionChanged() {
            var decoded = decodeMeeting($("#ouputDropdown option:selected").val());
            numRows = 1;
            $("#output tr:not(#headerRow)").remove();
            createOutput("#output tr:last", decoded["name"], decoded["date"]);
            getJson(decoded["name"], decoded["date"]);
        }
        // event handler for when clear button is clicked
        function clearSubmit() {
            var decoded = decodeMeeting($("#ouputDropdown option:selected").val());

            var msg = "Are you sure you want to clear '";
            if (decoded["name"] != null && decoded["name"].trim() != "")
                msg = msg + decoded["name"].trim() + "'?";
            else
                msg = msg + "All'?"

            var r = confirm(msg);
            if (r == true) {
                try {
                    if ((decoded["name"] != null && decoded["date"] != null) && (decoded["name"].trim() != "" && decoded["date"].trim() != ""))
                        deleteItems(decoded["name"].trim(), decoded["date"].trim());
                    else
                        clearData();
                }
                catch (err) {
                    alert(err.message);
                }
            }

            return false;
        }
        // gets the meeting and date from the query encoded data string
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
        function lotteryRoll() {
            var roll = Math.floor((Math.random() * (numRows-1)) + 1);
            var firstName = $("#row" + roll + " td:nth-child(2)").text();
            var lastName = $("#row" + roll + " td:nth-child(3)").text();
            $("#WinnerBox").html("<h2>" + firstName + " " + lastName + "!</h2>");
            return false;
        }
    </script>
</asp:Content>
