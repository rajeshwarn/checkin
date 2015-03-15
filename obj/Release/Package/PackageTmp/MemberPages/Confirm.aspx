<%@ Page Title="Confirm" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Confirm.aspx.cs" Inherits="IEEECheckin.ASP.Confirm" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h1 class="post-header">Confirmation</h1>            
    <p>Entry is confirmed.</p>
    <p>You should be redirected back in a few seconds.</p>
</asp:Content>
<asp:Content runat="server" ID="JavaScriptContent" ContentPlaceHolderID="JavaScripts">
    <script type="text/javascript">
        setTimeout(goBack, 750);
        function goBack() {
            window.history.back();
        }
    </script>
</asp:Content>