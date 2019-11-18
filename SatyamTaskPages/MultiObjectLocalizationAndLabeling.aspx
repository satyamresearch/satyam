<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MultiObjectLocalizationAndLabeling.aspx.cs" Inherits="SatyamTaskPages.MultiObjectLocalizationAndLabeling" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery-ui-1.11.4.custom/external/jquery/jquery.js"></script>
    <script src="jquery-ui-1.11.4.custom/jquery-ui.js"></script>
    <link href="jquery-ui-1.11.4.custom/jquery-ui.min.css" rel="stylesheet" />
        <link href="JQueryScripts/LocalizationAndLabeling/BoxDrawer.css" rel="stylesheet" />
    <script src="JQueryScripts/LocalizationAndLabeling/OnStart5.js"></script>
    <script src="JQueryScripts/LocalizationAndLabeling/BoxDrawer2.js"></script>
    <script src="JQueryScripts/LocalizationAndLabeling/DrawRegions2.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Draw Boxes Around All Objects of Interest and Select Their Categories</h2>
        <div id="attentionDiv">
        <h3>Attention : You are marking a 
            <asp:Label ID="CategoryMentionLabel" runat="server" Text="Category" ForeColor="Red"></asp:Label>
        </h3>
       </div>
        <table id="buttonTable">
            <tr>
                <td>
                    <asp:Button ID="DrawNextBoxButton" runat="server" Text="Draw New Boxes" OnClientClick="return NextBoxOnClick();" />
                </td>
                <td>
                    <asp:Button ID="SubmitButton" runat="server" Text="Submit and Get Next" onClientClick="return submit_button_on_click();" OnClick="SubmitButton_Click"/>
                </td>
            </tr>
        </table>
        <table>
            <tr>
                <td>
                    <div id="ImageDivision" style="position:relative;" > 
                         <asp:Image ID="DisplayImage" runat="server" ImageUrl="Images/4-cats-on-tree-fb-cover.jpg" Width="800" />
                    </div>
                </td>
                <td style="vertical-align:top">
                <table>
                    <tr><td>
                        <div id="CategorySelectionPanel" >
                            <table>
                                <tr>
                                    <td>
                                        <h3>Select Category</h3>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:RadioButtonList ID="CategorySelection_RadioButtonList" runat="server" Height="85px" Width="147px" onchange="categoryChanged()">
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                           </table>
                        </div>
                    </td></tr>
                    <tr><td>
                        <h3>Instructions</h3>                     
                         <ul  id="InstructionsBulletedList">
                             <li><strong>Your job</strong> is to draw tightly fitting boxes around <strong style="color:red">all</strong> the objects in the category list above.</li>
                             <li><strong style="color:red">The box must enclose the entire object but not any more.</strong></li>
                             <li><strong style="color:red">Draw a box only if you can see 50% or more of the object within the red dashed boundary.</strong></li>
                             <li><strong>To start </strong> drawing boxes Click &quot;Draw New Boxes&quot;.</li>
                             <li><strong>To draw a box </strong> Select the right category and then draw a box around the object by keeping the mouse pressed.</li>
                             <li><strong>You can draw multiple boxes</strong> at a single go. </li>
                             <li><strong>To Resize :</strong> If you want to delete/resize/move&nbsp; boxes that you already drew, click &quot;Edit/Delete Boxes&quot;.</li>
                             <li><strong>To Delete : </strong>click the cross on the top left corner.</li>
                             <li><strong>To submit and get next image </strong> Click &quot;Submit&quot; to submit your work</li>
                        </ul>
                        <div id="descriptionDiv">

                            <asp:Panel ID="DescriptionPanel" runat="server" Visible="false">
                                <h3>Detailed Description of Categories</h3>
                                <asp:Panel ID="DescriptionTextPanel" runat="server">
                                </asp:Panel>
                            </asp:Panel>

                        </div>
                    </td></tr>
                 </table>   
                </td>
            </tr>
        </table>   
        <div>
         <asp:HiddenField ID="Hidden_TaskEntryString" runat="server" />  
         <asp:HiddenField ID="Hidden_PageLoadTime" runat="server" />  
         <asp:HiddenField ID="Hidden_Result" runat="server" /> 
         <asp:HiddenField ID="Hidden_BoundaryLines" runat="server" /> 
    </div>
    </form>
</body>
</html>
