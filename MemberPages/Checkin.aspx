<%@ Page Title="Check-in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Checkin.aspx.cs" Inherits="IEEECheckin.ASPDocs.MemberPages.Checkin" %>

<%@ PreviousPageType VirtualPath="~/MemberPages/CreateCheckin.aspx" %> 

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1 class="post-header">Meeting Check-In for:</h1>
    <h2 id="meetingName" class="post-header"></h2>

    <div class="overlay" id="umnOverlay" style="display: none;">
        <div class="modal">
            <asp:Button runat="server" ID="overlayClose" Text="X" OnClientClick="return hideOverlay('#umnOverlay')" style="float: right;" />
            <div id="links" style="margin-top: 45px;"></div>
        </div>
    </div>
    <div id="swipe-section">
        <p class="section-label">ID Card Swipe Entry <i class="fa fa-credit-card"></i></p>
        <div class="boxed-section margin-lg-after">
            <div class="form-group no-margin-after">
                <asp:HiddenField ID="MeetingName" runat="server" />
                <asp:HiddenField ID="MeetingDate" runat="server" />
                <input class="form-control input-lg margin-sm-after clearable" autofocus="autofocus" type="password" id="cardtxt" placeholder="Click here, then swipe your card">
            </div>
        </div>
    </div>
    <p class="section-label">Manual Entry <i class="fa fa-pencil"></i></p>
    <div class="boxed-section margin-lg-after">
        <input class="form-control input-lg margin-sm-after clearable" type="text" id="firstname" placeholder="First Name">
        <input class="form-control input-lg margin-sm-after clearable" type="text" id="lastname" placeholder="Last Name">
        <input class="form-control input-lg margin-sm-after clearable" type="text" id="email" placeholder="Email">
        <input class="form-control input-lg margin-sm-after clearable" type="hidden" id="studentid">
        <button class="form-control input-lg btn btn-info check-in" onclick="return entrySubmit();" type="submit" id="checkinbutton" name="checkinbutton"><i class="fa fa-check"></i> Check In</button>
    </div>
</asp:Content>
<asp:Content ID="JavaScriptContent" ContentPlaceHolderID="JavaScripts" runat="server">
    <script src="../Scripts/card-parser.min.js"></script>
    <script type="text/javascript">
        $(document).ready(function () {
            setFocus();
            $("#meetingName").html($("#MainContent_MeetingName").val());
            // subscribe to the keydown events
            // move from firstname to lastname
            $("#firstname").keydown(function (e) {
                var charCode = e.charCode || e.keyCode || e.which;
                if (charCode == 13) {
                    e.preventDefault();
                    $("#lastname").focus();
                    return false;
                }
            });
            // move from lastname to email
            $("#lastname").keydown(function (e) {
                var charCode = e.charCode || e.keyCode || e.which;
                if (charCode == 13) {
                    e.preventDefault();
                    $("#email").focus();
                    return false;
                }
            });
            // submit the form
            $("#email, #cardtxt").keydown(function (e) {
                var charCode = e.charCode || e.keyCode || e.which;
                if (charCode == 13) {
                    e.preventDefault();
                    entrySubmit();
                    return false;
                }
            });
        });
        function entrySubmit() {
            try {
                // perform the parse function if the student id card input is not empty
                if (checkStr($("#cardtxt").val())) {
                    // create/retrieve regular expression
                    var regex = "^%(\\w+)\\^(\\d+)\\^{3}(\\d+)\\^(\\w+),\\s(?:([\\w\\s]+)\\s(\\w{1})\\?;|([\\w\\s]+)\\?;)(\\d+)=(\\d+)\\?$";
                    var indices = {
                        "firstName": "5,7",
                        "lastName": "4",
                        "middleName": "6",
                        "studentId": "2",
                        "email": "-1"
                    };
                    var re = $.cookie("card-regex");
                    if (re != null && re != undefined) {
                        var rejson = JSON.parse(re);
                        if (checkStr(rejson["regex"]))
                            regex = rejson["regex"];
                        if (rejson["indices"] != null && rejson["indices"] != undefined)
                            indices = rejson["indices"];
                    }

                    // parse the card
                    var result = parseCard($("#cardtxt").val().trim(), regex, indices);
                    if (result === null || result === undefined) {
                        alert("Failed to parse card data. Try again or use manual entry.");
                        clearForm();
                        setFocus();
                        return false;
                    }

                    // check to make sure first name, last name, and student id are present in the results
                    var datas = ["firstName", "lastName", "studentId"];
                    var datasReadable = ["First Name", "Last Name", "Student ID"];
                    var index;
                    for (index = 0; index < datas.length; index++) {
                        if (!checkStr(result[datas[index]])) {
                            alert("Missing " + datasReadable[index] + ".");
                            clearForm();
                            setFocus();
                            return false;
                        }
                    }

                    // check if email is present
                    var emailVal = "";
                    if (checkStr($("#email").val()))
                        emailVal = $("#email").val().trim();

                    // create new database entry
                    var entry = {
                        "firstname": result["firstName"],
                        "lastname": result["lastName"],
                        "studentid": result["studentId"],
                        "email": emailVal,
                        "meeting": $("#MainContent_MeetingName").val().trim(),
                        "date": $("#MainContent_MeetingDate").val().trim()
                    };

                    // add data to the database
                    if (!addData(entry)) {
                        clearForm();
                        setFocus();
                        return false;
                    }

                    // clear the form, ready for adding the next entry
                    clearForm();
                    setFocus();

                    // navigate to the confirmation page
                    window.location.href = "Confirm.aspx";
                }
                // student id card slot empty so check for valid manual entry
                else if (checkStr($("#firstname").val()) && checkStr($("#lastname").val())) {
                    // check if email is present
                    var emailVal = "";
                    if (checkStr($("#email").val()))
                        emailVal = $("#email").val().trim();
                    // check if student id present (for whatever reason)
                    var studentId = "";
                    if (checkStr($("#studentid").val()))
                        studentId = $("#studentid").val().trim();

                    // create new database entry
                    var entry = {
                        "firstname": $("#firstname").val().trim(),
                        "lastname": $("#lastname").val().trim(),
                        "studentid": studentId,
                        "email": emailVal,
                        "meeting": $("#MainContent_MeetingName").val().trim(),
                        "date": $("#MainContent_MeetingDate").val().trim()
                    };

                    // add data to the database
                    if (!addData(entry)) {
                        return false;
                    }

                    // clear the form, ready for adding the next entry
                    clearForm();
                    setFocus();

                    // navigate to the confirmation page
                    window.location.href = "Confirm.aspx";
                }
                // both are invalid
                else {
                    // card input is empty, but manual is not empty, but incomplete
                    if (!checkStr($("#cardtxt").val()) && (checkStr($("#firstname").val()) || checkStr($("#lastname").val()))) {
                        // firstname missing
                        if (!checkStr($("#firstname").val()) && checkStr($("#lastname").val())) {
                            alert("Missing First Name.");
                            $("#firstname").focus();
                            return false;
                        }
                        // lastname missing
                        else if (!checkStr($("#lastname").val()) && checkStr($("#firstname").val())) {
                            alert("Missing Last Name.");
                            $("#lastname").focus();
                            return false;
                        }
                        // something else missing
                        else
                            alert("Missing Card or Manual Input Data.");
                    }
                    // card input and manual input are empty
                    else
                        alert("Missing Card or Manual Input Data.");
                    setFocus();
                    return false;
                }
            }
            catch (err) {
                alert(err.message);
                setFocus();
            }

            return false;
        }
        // set focus based on saved cookies
        function setFocus() {
            var us = $.cookie("use-swipe");
            if (us != null && us != undefined && us === "true") {
                $("#cardtxt").focus();
            }
            else {
                $("#firstname").focus();
            }
        }
        // clear the entries in the form
        function clearForm() {
            $("input.clearable").each(function (index, element) {
                try {
                    $(this).val("");
                }
                catch (err) {

                }
            });
        }
    </script>
</asp:Content>
