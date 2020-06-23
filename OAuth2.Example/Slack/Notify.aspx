<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Notify.aspx.cs" 
    Inherits="OfficeClip.OpenSource.OAuth2.Example.Slack.Notify" %>

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
        <br />
        <div>--------------------------------------------------------------------------------</div>
        <div style="background-color:cornsilk">
            Select Channel:
            <asp:DropDownList ID="ddlChannels" runat="server" Width="200px"></asp:DropDownList>
        </div>
        <br />
        <div>--------------------------------------------------------------------------------</div>
        <br />
        <div>
            Message/Blocks:
            <br />
            <asp:TextBox ID="txtMessage" TextMode="MultiLine" runat="server" Width="400px" Rows="6"></asp:TextBox>
        </div>
        <br />
        <div>
            <asp:Button ID="btnSend" Text="Send" runat="server" Width="100px" OnClick="btnSend_Click"/>
        </div>

         <div>--------------------------------------------------------------------------------</div>

   </form>
</body>
</html>
