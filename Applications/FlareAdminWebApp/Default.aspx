<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="FlareAdminWebApp.DefaultPage" Title="Flare Administration Web Application" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <p>Flare Administration Web Application</p>
    <br />
    <p>Connect to the Flare database:</p>
    <p>Server name: <asp:TextBox ID="tbServerName" runat="server"></asp:TextBox></p>
    <p>Database name: <asp:TextBox ID="tbDatabaseName" runat="server"></asp:TextBox></p>
    <p>User ID: <asp:TextBox ID="tbUserID" runat="server"></asp:TextBox></p>
    <p>Password: <asp:TextBox ID="tbPassword" runat="server" TextMode="Password"></asp:TextBox></p>
    <br />
    <asp:Button ID="btnConnect" runat="server" onclick="btnConnect_Click" Text="Connect" />
    <br />
    <p>Message: <asp:Label ID="lblMessage" runat="server" Text="None"></asp:Label></p>
    <p>Inner Exception Message: <asp:Label ID="lblInnerExceptionMessage" runat="server" Text="None"></asp:Label></p>
    <p>Test Label: <asp:Label ID="lblTest" runat="server" Text="Init"></asp:Label></p>
</asp:Content>
