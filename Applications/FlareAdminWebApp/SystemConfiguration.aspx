<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="SystemConfiguration.aspx.cs" Inherits="FlareAdminWebApp.SystemConfigurationPage" Title="Flare System Configuration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <p>Flare System Configuration</p>
    <br />
    <asp:ListBox ID="lbEntryNames" runat="server" AutoPostBack="True" Width="200px" 
                        onselectedindexchanged="lbEntryNames_SelectedIndexChanged"></asp:ListBox>
    <br />
    <br />
    <p>Details of Selected Entry</p>
    <p>ID: <asp:TextBox ID="tbID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Name: <asp:TextBox ID="tbName" runat="server"></asp:TextBox></p>
    <p>Value: <asp:TextBox ID="tbValue" runat="server"></asp:TextBox></p>
    <br />
    <p>
        <asp:Button ID="btnNew" runat="server" Text="New" onclick="btnNew_Click" />
        <asp:Button ID="btnSave" runat="server" Text="Save" onclick="btnSave_Click" />
        <asp:Button ID="btnDelete" runat="server" Text="Delete" 
            onclick="btnDelete_Click" />
    </p>
</asp:Content>
