<%@ Page Title="Google Doc Export" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" validateRequest="false" CodeBehind="GoogleDocSelect.aspx.cs" Inherits="IEEECheckin.ASPDocs.MemberPages.GoogleDocSelect" %>

<%@ PreviousPageType VirtualPath="~/MemberPages/GoogleDocLoading.aspx" %> 

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .col-custom-offset {
            margin-left: 25.0%;
        }
        .col-custom {
            width: 50.0%;
        }
        .selected {
            font-weight: bold;
        }
        .tree-elements {
            color: rgba(0, 0, 0, 0.8);
        }
    </style>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="post-header">Google Doc Export</h1>
    <h2 id="meetingName" class="post-header"></h2>
    </div><!--Not an orphaned /div in VS, just starts on the master page. Same for closing tag of div below.-->
    <div class="col-custom-offset col-custom col-lg-4 col-lg-offset-4 col-md-6 col-md-offset-3 col-sm-8 col-sm-offset-2 col-xs-12">
        <p class="section-header">Export <i class="fa fa-download"></i></p> 
        <div class="boxed-section margin-lg-after">
            <p class="section-label">Select or Create Spreadsheet</p>
            <p>To create a new spreadsheet:</p>
            <ul>
                <li>Select a folder from the tree provided (default is "root").
                    <ul>
                        <li>Not all folders are listed, only those currently with spreadsheets are listed.</li>
                    </ul>
                </li>
                <li>Give a name for the new spreadsheet.</li>
                <li>Click the "Select Doc" button.</li>
            </ul>
            <p>To use an existing spreadsheet:</p>
            <ul>
                <li>Select an existing spreadsheet from the tree provided.
                    <ul>
                        <li>Not all of your spreadsheets may show up due to limitations in this app.</li>
                    </ul>
                </li>
                <li>Click the "Select Doc" button.</li>
            </ul>
            <p>Other notes/considerations:</p>
            <ul>
                <li>Data will be added to a worksheet in the selected/new spreadsheet.
                    <ul>
                        <li>If a worksheet with the same meeting name is present, data will be appended to the end of the worksheet.</li>
                        <li>If no worksheet with the same meeting name is present, then a new worksheet will be created.</li>
                        <li>The name of the worksheet will be the name of the meeting (same as at the top of page, but with spaces replaced by '_').</li>
                    </ul>
                </li>
                <li>Best practice would be to save all meetings in a given year to the same spreadsheet.</li>
                <li>Spreadsheets can be shared and multiple users can write data to the spreadsheets.</li>
                <li>You can add rows and columns to the worksheets, but you must keep the names and position of the first row.
                    <ul>
                        <li>Changing the first row may lead to unexpected results or data not being added.</li>
                    </ul>
                </li>
            </ul>
            <br />

            <asp:HiddenField ID="SubmitData" runat="server" />
            <asp:HiddenField ID="MeetingName" runat="server" />
            <asp:HiddenField ID="MeetingDate" runat="server" />
            <asp:HiddenField ID="FolderTreeXml" runat="server" />
            <asp:XmlDataSource ID="SheetTreeDataSource" runat="server" />

            <asp:TreeView ID="SheetTree" DataSourceID="SheetTreeDataSource" runat="server" SelectedNodeStyle-CssClass="selected" ShowLines="True" NodeStyle-HorizontalPadding="5" NodeStyle-VerticalPadding="5" NodeStyle-CssClass="tree-elements">
                <DataBindings>
                    <asp:TreeNodeBinding DataMember="GoogleFolder" TextField="Title" ValueField="Id" ToolTip="Google Folder" />
                    <asp:TreeNodeBinding DataMember="GoogleSheet" TextField="Title" ValueField="FeedUri" ToolTip="Google Sheet" />
                </DataBindings>
            </asp:TreeView>
            <br /><br />
            <asp:TextBox class="form-control input-lg margin-sm-after" ID="NewDocument" placeholder="New Spreadsheet Name" runat="server" />
            <asp:Button CssClass="form-control input-lg btn btn-info check-in" ID="SubmitButton" OnClick="SubmitGoogle" Text="Select Doc" runat="server" />
        </div>
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

            var bc = $.cookie("body-color"); // text color
            if (bc != null && bc != undefined) {
                $(".tree-elements").css("color", "#" + bc);
            }
            else {
                $(".tree-elements").css("color", "rgba(0, 0, 0, 0.8)");
            }
        });
    </script>
</asp:Content>
