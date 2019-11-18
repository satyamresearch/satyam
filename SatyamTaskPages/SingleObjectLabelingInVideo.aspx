<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SingleObjectLabelingInVideo.aspx.cs" Inherits="SatyamTaskPages.SingleObjectLabelingInVideo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery-ui-1.11.4.custom/external/jquery/jquery.js"></script>
    <script src="jquery-ui-1.11.4.custom/jquery-ui.js"></script>
    <link href="jquery-ui-1.11.4.custom/jquery-ui.min.css" rel="stylesheet" />
    <script src="JQueryScripts/SingleObjectLabelingInVideo/StartUp1.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Select the Right Category for the Video Clip Below</h2>
        </div>
        <table>
            <tr>
                <td class="auto-style1">
                    <table>
                        <tr>
                            <td>
                                <div id="VideoDivision"></div>
                            </td>
                         </tr>
                        <tr>
                            <td>
                                <table style="vertical-align:top">
                                    <tr>
                                        <td>   
                                            
                                            <asp:RadioButtonList ID="CategorySelection_RadioButtonList" runat="server" AutoPostBack="False">
                                            </asp:RadioButtonList>
                                            
                                        </td>
                                        <td>
                                            <asp:Button ID="SubmitButton" runat="server" OnClick="SubmitButton_Click" Text="Submit" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                 </td>
                 <<td>
                     <asp:Panel ID="PreacceptancePanel" runat="server" Visible="true">
                         <strong>Note : This is preview only. You will be able to submit only after you have accepted the task</strong>
                     </asp:Panel>
                     <br />
                     In this task you have labeled 
                     <asp:Label ID="NoLabeled" runat="server" Font-Bold="True" Text="0"></asp:Label>
&nbsp;videos so far.<h3>Instructions</h3>
                     
                         <ul  id="InstructionsBulletedList">
                             <li>You will be presented between 1 to 10 videos.<span style="color:red"><strong> You will only paid after you finish categorizing them all correctly</strong> </span>.</li>
                            <li>Select <span style="color:red"> the most appropriate (only one) </span> category that video belongs to by using the radio buttons below and then enter submit.</li>
                            <li>After you enter submit the next video will come to you.</li>
                            <li><span style="color:red"><strong>Note :</strong> Your performace is being monitored in this task.</span></li>
                        </ul>
                        <div id="descriptionDiv">

                            <asp:Panel ID="DescriptionPanel" runat="server" Visible="false">
                                <h3>Detailed Description of Categories</h3>
                                <asp:Panel ID="DescriptionTextPanel" runat="server">

                                </asp:Panel>
                            </asp:Panel>

                        </div>
                 </td>

           </tr>
        </table> 
        <div>
         <asp:HiddenField ID="Hidden_VideoURL" runat="server" />  
         <asp:HiddenField ID="Hidden_TaskEntryString" runat="server" />  
         <asp:HiddenField ID="Hidden_PageLoadTime" runat="server" />   
        </div>
    </form>

</body>
</html>
