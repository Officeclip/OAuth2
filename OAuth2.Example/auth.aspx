<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="auth.aspx.cs" 
    Inherits="OfficeClip.OpenSource.OAuth2.Example.Auth" %>

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
            <br />
            State Value Returned:
            <asp:Literal ID="litState" runat="server" />
            <br />
            Refresh Token:
            <asp:Literal ID="litRefreshToken" runat="server" />
        </div>
        <div>
            Full Name:
            <asp:Literal ID="litFullName" runat="server" />
            <br />
            Email:
            <asp:Literal ID="litEmail" runat="server" />
        </div>
        <div>
            Picture:
            <br />
            <%= ImageHtml %>
        </div>
        <div>
            Picture (200px):
            <br />
            <%=ImageResizedHtml %>
        </div>
        <div>--------------------------------------------------------------------------------</div>
        <div>
            <asp:Literal ID="litDirectoryString" runat="server" />
        </div>
        <div>--------------------------------------------------------------------------------</div>
        <div>
            <asp:Literal ID="litCalendarString" runat="server" />
        </div>
         <div>--------------------------------------------------------------------------------</div>
        <div>
            <asp:Literal ID="litContactString" runat="server" />
        </div>
   </form>
</body>
</html>
