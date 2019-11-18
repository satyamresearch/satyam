<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SingleObjectLabeling.aspx.cs" Inherits="SatyamTaskPages.SingleObjectLabeling1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style1 {
            width: 380px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Select the Right Category for the Image Below</h2>
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
                                            
                                            <asp:RadioButtonList ID="CategorySelection_RadioButtonList" runat="server" AutoPostBack="True">
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
                 <td>
                    <h3>Instructions</h3>
                     
                         <ul  id="InstructionsBulletedList">
                            <li>Select <span style="color:red"> <strong>the most appropriate (only one)</strong> </span> category that image belongs to by using the radio buttons below and then enter submit.</li>
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
         <asp:HiddenField ID="Hidden_TaskEntryString" runat="server" />  
         <asp:HiddenField ID="Hidden_PageLoadTime" runat="server" />   
    </div>
    </form>
</body>
</html>