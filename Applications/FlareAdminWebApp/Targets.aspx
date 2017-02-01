<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Targets.aspx.cs" Inherits="FlareAdminWebApp.TargetsPage" Title="Flare Targets" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <p>Flare Targets</p>
    <br />
    <asp:ListBox ID="lbTargetNames" runat="server" AutoPostBack="True" 
        Width="200px" onselectedindexchanged="lbTargetNames_SelectedIndexChanged">
    </asp:ListBox>
    <br />
    <br />
    <p>Details of Selected Target:</p>
    <p>Target ID: <asp:TextBox ID="tbTargetID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Account ID: <asp:TextBox ID="tbAccountID" runat="server"></asp:TextBox></p>
    <p>Enabled: <asp:CheckBox ID="cbEnabled" runat="server" /></p>
    <p>Name: <asp:TextBox ID="tbName" runat="server"></asp:TextBox></p>
    <p>URL: <asp:TextBox ID="tbURL" runat="server" Width="300px"></asp:TextBox></p>
    <p>Monitor interval (in seconds): <asp:TextBox ID="tbMonitorInterval" runat="server"></asp:TextBox></p>
    <p>Last monitored at: <asp:TextBox ID="tbLastMonitoredAt" runat="server" ReadOnly="True" Width="300px"></asp:TextBox></p>
    <p>Monitor type: <asp:DropDownList ID="ddlMonitorType" runat="server"></asp:DropDownList></p>
    <p>Last target log ID: <asp:TextBox ID="tbLastTargetLogID" runat="server"></asp:TextBox></p>
    <p>Target added at: <asp:TextBox ID="tbTargetAddedAt" runat="server" ReadOnly="True" Width="300px"></asp:TextBox></p>
    <p>Last failed at: <asp:TextBox ID="tbLastFailedAt" runat="server" ReadOnly="True" Width="300px"></asp:TextBox></p>
    <br />
    <p>Form Fields:</p>
    <asp:ListBox ID="lbTargetFormFieldNames" runat="server" AutoPostBack="True" 
        Width="200px" onselectedindexchanged="lbTargetFormFieldNames_SelectedIndexChanged">
    </asp:ListBox>
    <br />
    <p>Field ID: <asp:TextBox ID="tbFieldID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Field name: <asp:TextBox ID="tbFieldName" runat="server"></asp:TextBox></p>
    <p>Field value: <asp:TextBox ID="tbFieldValue" runat="server"></asp:TextBox></p>
    <br />
    <p>
        <asp:Button ID="btnNewField" runat="server" Text="New Field" onclick="btnNewField_Click" />
        <asp:Button ID="btnSaveField" runat="server" Text="Save Field" onclick="btnSaveField_Click" />
        <asp:Button ID="btnDeleteField" runat="server" Text="Delete Field" onclick="btnDeleteField_Click" />
    </p>
    <br />
    <p>
        <asp:Button ID="btnNewTarget" runat="server" Text="New Target" onclick="btnNewTarget_Click" />
        <asp:Button ID="btnSaveTarget" runat="server" Text="Save Target" onclick="btnSaveTarget_Click" />
        <asp:Button ID="btnDeleteTarget" runat="server" Text="Delete Target" onclick="btnDeleteTarget_Click" />
    </p>
</asp:Content>
