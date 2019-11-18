<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ObjectCountingInVideoMTurk.aspx.cs" Inherits="SatyamTaskPages.ObjectCountingInVideoMTurk" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery-ui-1.11.4.custom/external/jquery/jquery.js"></script>
    <script src="jquery-ui-1.11.4.custom/jquery-ui.js"></script>
    <link href="jquery-ui-1.11.4.custom/jquery-ui.min.css" rel="stylesheet" />
    <script src="JQueryScripts/SingleObjectCountingInVideo/StartUp1.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Count the number of 
                <asp:Label ID="ObjectNameLabel" runat="server" Text="object"></asp:Label>
                (s) in the video clip below</h2>
        </div>
         <div>

            <asp:Label ID="ErrorLabel" runat="server" Text=""></asp:Label>

        </div>
        <table>
            <tr>
                <td class="auto-style1">
                    <table>
                        <tr>
                            <td class="auto-style2">
                                <div id="VideoDivision"></div>
                            </td>
                         </tr>
                        <tr>
                            <td class="auto-style2">
                                <table style="vertical-align:top">
                                    <tr>
                                        <td>   
                                            
                                            
                                            
                                            <strong>Count =</strong>
                                            <asp:TextBox ID="CountTextBox" runat="server"></asp:TextBox>
                                            
                                            
                                            
                                        </td>
                                        <td>
                                            <asp:Button ID="SubmitButton" runat="server" OnClick="SubmitButton_Click" Text="Submit and Get Next" Enabled="False"/>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                 </td> 
                 <td>
                     <asp:Panel ID="PreacceptancePanel" runat="server" Visible="true">
                         <strong>Note : This is preview only. You will be able to submit only after you have accepted the task</strong>
                     </asp:Panel>
                     <br />
                     In this task you have finished 
                     <asp:Label ID="NoLabeled" runat="server" Font-Bold="True" Text="0"></asp:Label>
&nbsp;images so far.<h3>Instructions</h3>
                     
                         <ul  id="InstructionsBulletedList">
                             <li>You will be presented between 1 to 20 video clips. You have to count the number of  
                                 <asp:Label ID="ObjectNameLabel1" runat="server" Text="object"></asp:Label>
                                 (s) in the video clip.<span style="color:red"><strong> You will only paid after you finish counting in them all correctly</strong> </span>.</li>
                            <li>After you enter submit the next video clip will come to you.</li>
                            <li><span style="color:red"><strong>Note :</strong> Your performance is being monitored in this task.</span></li>
                        </ul>
                        <div id="descriptionDiv">

                            <asp:Panel ID="DescriptionPanel" runat="server" Visible="false">
                                <h3>Detailed Description of
                                    <asp:Label ID="ObjectNameLabel2" runat="server" Text="Object"></asp:Label>
                                </h3>
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
         <asp:HiddenField ID="Hidden_Price" runat="server" />  
         <asp:HiddenField ID="Hidden_TasksPerJob" runat="server" />
         <asp:HiddenField ID="Hidden_PageLoadTime" runat="server" />  
         <asp:HiddenField ID="Hidden_NoImagesDone" runat="server" Value="0" /> 
         <asp:HiddenField ID="Hidden_AmazonAssignmentID" runat="server" /> 
         <asp:HiddenField ID="Hidden_AmazonWorkerID" runat="server" /> 
         <asp:HiddenField ID="Hidden_HITID" runat="server" /> 
    </div>
    </form>
</body>
</html>
