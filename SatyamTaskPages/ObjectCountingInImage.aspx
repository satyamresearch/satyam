<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ObjectCountingInImage.aspx.cs" Inherits="SatyamTaskPages.ObjectCountingInImage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Count the number of 
                <asp:Label ID="ObjectNameLabel" runat="server" Text="object"></asp:Label>
                (s)&nbsp;in the image below</h2>
        </div>
        <div>

            <asp:Label ID="ErrorLabel" runat="server" Text=""></asp:Label>

        </div>
        <table>
            <tr>
                <td class="auto-style1">
                    <table>
                        <tr>
                            <td>
                                <asp:Image ID="DisplayImage" runat="server" Height="200px" />
                            </td>
                         </tr>
                        <tr>
                            <td>
                                <table style="vertical-align:top">
                                    <tr>
                                        <td>   
                                            
                                            
                                            
                                            <strong>Count =</strong>
                                            <asp:TextBox ID="CountTextBox" runat="server"></asp:TextBox>
                                            
                                            
                                            
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
                 <td>
                    <h3>Instructions</h3>
                     
                         <ul  id="InstructionsBulletedList">
                            <li>Count the number of&nbsp;
                                <asp:Label ID="ObjectNameLabel1" runat="server" Text="object "></asp:Label>
                                (s)
                                in this image and enter the number in the text box below and then enter submit.</li>
                        </ul>
                        <div id="descriptionDiv">

                            <asp:Panel ID="DescriptionPanel" runat="server" Visible="false">
                                <h3>Detailed Description of
                                    <asp:Label ID="Label1" runat="server" Text="object"></asp:Label>
                                </h3>
                                <asp:Panel ID="DescriptionTextPanel" runat="server">

                                </asp:Panel>
                            </asp:Panel>

                        </div>
                 </td>

           </tr>
        </table>
        <div>
         <asp:HiddenField ID="Hidden_TaskEntryString" runat="server" />  
         <asp:HiddenField ID="Hidden_PageLoadTime" runat="server" />   
    </div>
    </form>
</body>
</html>
