<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Contacts.aspx.cs" Inherits="FlareAdminWebApp.ContactsPage" Title="Flare Contacts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <p>Flare Contacts</p>
    <br />
    <asp:ListBox ID="lbContactNames" runat="server" AutoPostBack="True" 
        Width="200px" onselectedindexchanged="lbContactNames_SelectedIndexChanged"></asp:ListBox>
    <br />
    <br />
    <p>Details of Selected Contact</p>
    <p>Contact ID: <asp:TextBox ID="tbContactID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Account ID: <asp:TextBox ID="tbAccountID" runat="server"></asp:TextBox></p>
                    <p>
                        <asp:ListBox ID="lbAccountNames" runat="server" Width="200px"></asp:ListBox>
                    </p>
    <p>Enabled: <asp:CheckBox ID="cbEnabled" runat="server" /></p>
    <p>First Name: <asp:TextBox ID="tbFirstName" runat="server"></asp:TextBox></p>
    <p>Last Name: <asp:TextBox ID="tbLastName" runat="server"></asp:TextBox></p>
    <p>E-mail Address: <asp:TextBox ID="tbEmailAddress" runat="server" Width="256px"></asp:TextBox></p>
    <br />
    <p>
        <asp:Button ID="btnNew" runat="server" Text="New" onclick="btnNew_Click" />
        <asp:Button ID="btnSave" runat="server" Text="Save" onclick="btnSave_Click" />
        <asp:Button ID="btnDelete" runat="server" Text="Delete" 
            onclick="btnDelete_Click" />
    </p>
</asp:Content>
