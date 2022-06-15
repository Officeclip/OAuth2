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
            Client Id:
            <asp:Literal ID="litClientId" runat="server" />
            Access Token (from Authorization Code):
            <asp:Literal ID="litAccessToken" runat="server" />
            <br /><br />
            State Value Returned:
            <asp:Literal ID="litState" runat="server" />
            <br /><br />
            Refresh Token:
            <asp:Literal ID="litRefreshToken" runat="server" />
        </div>
   </form>
</body>
</html>
