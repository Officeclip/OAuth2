<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Auth.aspx.cs" 
    Inherits="OfficeClip.OpenSource.OAuth2.Example.Slack.Auth" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="color: red">
            <asp:Literal ID="litError" runat="server" />
        </div>
        <div>
            Access Token (from Authorization Code):
            <asp:Literal ID="litAccessToken" runat="server" />
        </div>
        <div>
            <strong>Connected to: </strong>
            <asp:Literal ID="litTeamName" runat="server" /> &nbsp;
            <label> -- </label>
            <asp:Literal ID="litChannelName" runat="server"></asp:Literal>
        </div>
       <div>
            <strong>Webhook url: </strong>
            <asp:Literal ID="litWebhook" runat="server" /> &nbsp;
        </div>
        <br />
        <div>
            <asp:Button ID="btnSend" Text="Send Message" runat="server" Width="100px" OnClick="btnSend_Click"/>
        </div>
   </form>
</body>
</html>
