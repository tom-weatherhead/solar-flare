<%@ Page Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="TargetLogSearch.aspx.cs" Inherits="FlareAdminWebApp.TargetLogSearchPage" Title="Flare Target Log Search Engine" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <p>Flare Target Log Search Engine</p>
    <hr />
    <p>Search Criteria:</p>
    <p>
        <asp:RadioButton ID="rbTarget" runat="server" AutoPostBack="True" 
            oncheckedchanged="rbTarget_CheckedChanged" />
        Log records associated with the target named:
    </p>
    <asp:ListBox ID="lbTargetNames" runat="server" Width="256px"></asp:ListBox>
    <p>
        <asp:RadioButton ID="rbAccount" runat="server" AutoPostBack="True" 
            oncheckedchanged="rbAccount_CheckedChanged" />
        Log records associated with targets associated with the account named:
    </p>
    <asp:ListBox ID="lbAccountNames" runat="server" Width="256px"></asp:ListBox>
    <p>
        <asp:RadioButton ID="rbAny" runat="server" AutoPostBack="True" 
            oncheckedchanged="rbAny_CheckedChanged" />
        Log records associated with any target
    </p>
    <p>
        <asp:CheckBox ID="cbFailuresOnly" runat="server" />
        Log records of failed transactions only
    </p>
    <p>
        <asp:CheckBox ID="cbStartTimeStamp" runat="server" AutoPostBack="True" 
            oncheckedchanged="cbStartTimeStamp_CheckedChanged" />
        Start Timestamp:
        <asp:TextBox ID="tbStartTimeStamp" runat="server" Height="22px" Width="256px"></asp:TextBox>
    </p>
    <p>
        <asp:CheckBox ID="cbEndTimeStamp" runat="server" AutoPostBack="True" 
            oncheckedchanged="cbEndTimeStamp_CheckedChanged" />
        End Timestamp:
        <asp:TextBox ID="tbEndTimeStamp" runat="server" Width="256px"></asp:TextBox>
    </p>
    <p><asp:Button ID="btnSearch" runat="server" Text="Search" onclick="btnSearch_Click" /></p>
    <hr />
    <p>Search Results:</p>
    <asp:ListBox ID="lbSearchResultSummaries" runat="server" Width="256px" 
        onselectedindexchanged="lbSearchResultSummaries_SelectedIndexChanged" AutoPostBack="True"></asp:ListBox>
    <p>Details of Selected Search Result</p>
    <p>Log record ID: <asp:TextBox ID="tbLogRecordID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Target ID: <asp:TextBox ID="tbTargetID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Timestamp: <asp:TextBox ID="tbTimeStamp" runat="server" ReadOnly="True" Width="256px"></asp:TextBox></p>
    <p>Transaction status: <asp:TextBox ID="tbStatus" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>
        Message:<br />
        <asp:TextBox ID="tbMessage" runat="server" Height="96px" ReadOnly="True" 
            TextMode="MultiLine" Width="512px"></asp:TextBox>
    </p>
    <p>Error code: <asp:TextBox ID="tbErrorCode" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Location ID: <asp:TextBox ID="tbLocationID" runat="server" ReadOnly="True"></asp:TextBox></p>
    <p>Response time: <asp:TextBox ID="tbResponseTime" runat="server" ReadOnly="True"></asp:TextBox>&nbsp;milliseconds</p>
</asp:Content>
