<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="auth.aspx.cs" 
    Inherits="OfficeClip.OpenSource.OAuth2.Example.MS365.Auth" %>

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
            Mode:
            <asp:Literal ID="litMode" runat="server" />
            Client Id:
            <asp:Literal ID="litClientId" runat="server" />
            <br /><br />
            Access Token:
            <asp:Literal ID="litAccessToken" runat="server" />
            <br /><br />
           Refresh Token:
            <asp:Literal ID="litRefreshToken" runat="server" />
            <br /><br />
            State Value Returned:
            <asp:Literal ID="litState" runat="server" />
            <br /><br />
            User Info String:
            <asp:Literal ID="litResponseString" runat="server" />
             <br /><br />
            Imap Test Output
            <asp:Literal ID="litImapTest" runat="server" />
             <br /><br />
            Exchange Access Token
            <asp:Literal ID="litExchangeAccessToken" runat="server" />
             <br /><br />
            Exchange Refresh Token
            <asp:Literal ID="litExchangeRefreshToken" runat="server" />
        </div>
   </form>
</body>
</html>
