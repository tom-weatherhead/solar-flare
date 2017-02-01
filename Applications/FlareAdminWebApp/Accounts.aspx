<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Accounts.aspx.cs" Inherits="FlareAdminWebApp.AccountsPage" Title="Flare Client Accounts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <p>Flare Client Accounts</p>
    <br />
    <asp:ListBox ID="lbAccountNames" runat="server" AutoPostBack="True" 
        onselectedindexchanged="lbAccountNames_SelectedIndexChanged" Width="200px">
    </asp:ListBox>
    <br />
    <br />
    <p>Details of Selected Account</p>
    <p>Account ID: <asp:TextBox ID="tbAccountID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Enabled: <asp:CheckBox ID="cbEnabled" runat="server" /></p>
    <p>Name: <asp:TextBox ID="tbName" runat="server" Width="200px"></asp:TextBox></p>
    <br />
    <p>
        <asp:Button ID="btnNew" runat="server" onclick="btnNew_Click" Text="New" />
        <asp:Button ID="btnSave" runat="server" onclick="btnSave_Click" Text="Save" />
        <asp:Button ID="btnDelete" runat="server" onclick="btnDelete_Click" Text="Delete" />
    </p>
</asp:Content>
