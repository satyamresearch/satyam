<%@ Page Title="" Language="C#" MasterPageFile="~/MainPage.Master" AutoEventWireup="true" CodeBehind="JobStatusPage.aspx.cs" Inherits="SatyamPortal.JobStatusPage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainTemplatePlaceHolder" runat="server">
    <h2>Job Status</h2>
    <p>
            <asp:Button ID="JobStatusRefreshButton" runat="server" Text="Refresh" style="align-content:center;" OnClick="JobStatusRefreshButton_Click"/>
    </p>
    <div id="jobstatusdivision">
        <asp:Repeater ID="rpt" runat="server">
            <%--<asp:Repeater ID="Repeater1" runat="server" OnItemDataBound="rpt_RowDataBound">--%>
        <HeaderTemplate>     
            <table runat="server" border="1" style="color: White; background-color: #3A4F63;" id="headerTable">                       
                <tr>
                    <td  style="width:300px;background-color: #3A4F63; color: White;">
                        Job GUID 
                    </td>
                    <td style="width:400px;background-color: #3A4F63; color: White;">
                       Job Template Type
                    </td>
                    <td style="width:200px;background-color: #3A4F63; color: White;">
                        Job Submit Time
                    </td>
                    <td style="width:100px;background-color: #3A4F63; color: White;">
                        Job Status
                    </td>
                    <td style="width:100px;background-color: #3A4F63; color: White;">
                        Job Progress
                    </td>
                    <td style="width:100px;background-color: #3A4F63; color: White;">
                        Task Pending
                    </td>
                    <td style="width:100px;background-color: #3A4F63; color: White;">
                        Total Results
                    </td>
                    <td style="width:100px;background-color: #3A4F63; color: White;">
                        Total Aggregated
                    </td>
                    <td style="width:100px;background-color: #3A4F63; color: White;">
                        Approval Rate
                    </td>
                </tr>
            </table>
            </HeaderTemplate>
            <ItemTemplate>
            <table border="1">
                <tr>
                    <td style="width:300px;">
                        <asp:Label ID="JobStatusPage_JobGUIDLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "JobGUID")%>'></asp:Label>
                    </td>
                    <td style="width:400px;">
                        <asp:Label ID="JobStatusPage_JobTemplateTypeLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "JobTemplateType")%>'></asp:Label>
                    </td>
                    <td style="width:200px;">
                        <asp:Label ID="JobStatusPage_JobSubmitTimeLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "JobSubmitTime")%>'></asp:Label>
                    </td>
                    <td style="width:100px;">
                        <asp:Label ID="JobStatusPage_JobStatusLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "JobStatus")%>'></asp:Label>
                    </td>
                    <td style="width:100px;">
                        <asp:Label ID="JobStatusPage_JobProgressLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "JobProgress")%>'></asp:Label>
                    </td>

                    <td style="width:100px;">
                        <asp:Label ID="JobStatusPage_TaskPendingLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "TaskPending")%>'></asp:Label>
                    </td>
                    <td style="width:100px;">
                        <asp:Label ID="TotalResults_TaskPendingLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "TotalResults")%>'></asp:Label>
                    </td>
                    <td style="width:100px;">
                        <asp:Label ID="TotalAggregated_TaskPendingLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "TotalAggregated")%>'></asp:Label>
                    </td>
                    <td style="width:100px;">
                        <asp:Label ID="ApprovalRate_TaskPendingLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "ApprovalRate")%>'></asp:Label>
                    </td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:Repeater>
    </div>
</asp:Content>
