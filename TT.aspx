<%@ Page Language="C#" AutoEventWireup="true" CodeFile="TT.aspx.cs" Inherits="TT" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    actionType
    <asp:TextBox ID="txtActionType" runat="server" Width="158px">0</asp:TextBox>
    <br />
    targetColumns<asp:TextBox ID="txtColumns" runat="server" Width="645px"></asp:TextBox>
&nbsp;
    <br />
    targetValues 
    <asp:TextBox ID="txtValues" runat="server" Width="655px"></asp:TextBox>
    <br />
    targetTable<asp:TextBox ID="txtTable" runat="server" Width="655px"></asp:TextBox>
    <br />
     Where <asp:TextBox ID="txtWhere" runat="server" Width="655px"></asp:TextBox>
    <br />
    SortBy <asp:TextBox ID="txtSortBy" runat="server" Width="655px"></asp:TextBox>
    <br />
    <br />
    <asp:Button ID="Button1" runat="server" onclick="Button1_Click" Text="Button" />
    <br />
    <br />
    <asp:Label ID="lbResult" runat="server" Text=""></asp:Label>
    <br />
    <br />
    <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
    </form>
</body>
</html>
