<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" 
    Inherits="OfficeClip.OpenSource.Example._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="color:red">
        <asp:Literal ID="litError" runat="server" />
    </div>
        <div>
            Access Token (from Refresh Token):
            <asp:Literal ID="litAccessToken" runat="server" />
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
        <div>--------------------------------------------------------------------------------</div>
        <div>
            <asp:Literal ID="litDirectoryString" runat="server" />
        </div>
    </form>
</body>
</html>
