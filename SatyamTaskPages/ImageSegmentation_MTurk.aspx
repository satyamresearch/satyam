<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImageSegmentation_MTurk.aspx.cs" Inherits="SatyamTaskPages.ImageSegmentation_MTurk" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery-ui-1.11.4.custom/external/jquery/jquery.js"></script>
    <script src="jquery-ui-1.11.4.custom/jquery-ui.js"></script>
    <link href="jquery-ui-1.11.4.custom/jquery-ui.min.css" rel="stylesheet" />
    <script src="JQueryScripts/ImageSegmentation/startSegmentation.js"></script>
    <script src="JQueryScripts/ImageSegmentation/BoundaryDrawer3.js"></script>
    <script src="JQueryScripts/ImageSegmentation/Polygon.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Draw Polygons Around All Objects of Interest and Select Their Categories</h2>
        <div>
        <h3> Task:</h3>
        <div><ul>
        <li><strong>Your job</strong> is to draw one/multiple tightly fitting polygons along the boundaries (inner/outer) of <strong style="color:red">each and every instance/object</strong> (if any) of the classes listed in the category list.</li>
        <li><strong style="color:red">Draw boundaries so that only all of the visible parts of the object are enclosed but not any more.</strong></li>
        <%--<li><strong style="color:red">Draw a polygon only if you can see 50% or more of the object within the red dashed boundary.</strong></li>--%>
            </ul></div>
        
            <table>
               <tr>
                   <td style="vertical-align:top">
                       <div id="ImageDivision" style="position:relative;width=800px" > 
                         <canvas id="canvas" width="690" height="651" style="position:absolute"></canvas>
                         <%--<asp:Image ID="TheImage" runat="server" ImageUrl="Images/4-cats-on-tree-fb-cover.jpg" Width="690px" />--%>
                           <asp:Image ID="TheImage" runat="server" Width="690px" />
                       </div>
                    </td>
                    <td style="vertical-align:top">
                        <div id="attentionDiv">
                            <h3>Attention : You are marking a 
                                <asp:Label ID="CategoryMentionLabel" runat="server" Text="Category" ForeColor="Red"></asp:Label>
                            </h3>
                           </div>
                        <div id="CategorySelectionPanel" >
                             <table>
                               <tr>
                                  <td>
                                     <%--<h3 style="color:red;"><strong>Select a Category First</strong></h3>--%>
                                      <h3><strong>Select a Category:</strong></h3>
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
                        <div id="QuestionDiv">
                            <h3>Have you drawn all boundaries of all parts of this object, NOT including any different objects of same or different categories?
                            </h3>
                           </div>
                        

        <div id="buttonDiv">
        <table>           
            <tr><td>
                    <table id="NewObjectButtonTable">
                        <tr>
                            <td>
                        <asp:Button ID="NewObjectButton" runat="server" Text="Start A New Object" OnClientClick="return NewObjectOnClick();" />
                        </td>
                        </tr>
                    </table>
            </td></tr>
            <tr>
                 <td>
                    <table id="addDropTable">
                    <tr>
                    <td>
                        <asp:Button ID="AddPolygon" runat="server" Text="Draw A New Boundary" OnClientClick="return AddPolygonButtonOnClick();" />
                        <asp:Button ID="DropPolygon" runat="server" Text="Undo Last Boundary" OnClientClick="return DeletePolygonButtonOnClick();" />
                    </td>
                    </tr>
                    </table>
                  </td>
             </tr>
            <tr>
                 <td>
                    <table id="editTable">
                    <tr>
                    <td>
                        <asp:Button ID="Finish" runat="server" Text="Finish Boundary" OnClientClick="return FinishPolygonOnClick();"/>
                    </td>
                    <td>
                        <asp:Button ID="UndoButton" runat="server" Text="Undo Last Point" OnClientClick="return UndoButtonOnClick();"/>
                    </td>
                    
                    </tr>
                    </table>
                  </td>
             </tr>
            
            <tr>
                 <td>
                    <table id="buttonTable">
                    <tr>
                    <td>
                        <asp:Button ID="DeleteObjectButton" runat="server" Text="Delete Selected Object" OnClientClick="return DeleteObjectClick();" />
                        </td>
                        <td>
                       <asp:Button ID="SubmitButton" runat="server" Text="Submit" onClientClick="return submit_button_on_click();" OnClick="SubmitButton_Click"/>
                    </td>
                    </tr>
                    </table>
                  </td>
             </tr>
            </table>
            </div>

                <h3>Instructions And Examples:</h3>
                <div id="StartingInstruction"><%-- Need an example for all cases --%>
                <ul>
                    <li id="Start"><strong>To start </strong>, select a category of the object you are drawing, then click &quot;Start A New Object&quot;.</li>
                    <li><strong>When finished</strong>, check if <strong>each and every instance</strong> of the visible objects in the category list are segmented, click on &quot;Submit&quot; to submit the result.</li>
                    <li><strong>Example Good Work</strong> (when only cat and dog are on the category list):</li>
                    <li><img width="200" src="/Images/dog2.jpg"/></li>
                    </ul>
                    </div>
                <div id="FirstPolyInstruction">
                <ul>
                    <li id="FirstPoly"><strong>Click on</strong> &quot;Draw A New Boundary&quot; to start drawing a first polygon boundary of the object. </li>
                    <li><img width="200" src="/Images/cat1.jpg"/></li>
                    </ul>
                    </div>
                <div id="SecondPolyInstruction">
                <ul>
                    <li><strong>When you are done</strong>, check to see if every visible part of the target object is bounded (highlighted by category color).</li>
                    <li><strong>Click on</strong> &quot;Finish Object&quot; to finish the current object.</li>
                    <li id="SecondPoly">If there is <strong>another boundary of the SAME object</strong> to be drawn, click on &quot;Draw A New Boundary&quot; to add a new polygon boundary of the object. This could be a hole inside an existing boundary, or another part of the object that is <strong>not overlapping</strong> with <strong>any</strong> of the existing polygons drawn. </li>
                    <li><strong>DO NOT</strong> include different objects of the same class before you click on &quot;Finish Object&quot;.</li>
                    <li id="UndoPoly"><strong>To delete</strong> the last polygon drawn, click on &quot;Undo Last Boundary&quot;.</li>
                    <li>If the new part you want to draw is connected/overlapped with an existing part, you should consider delete the existing one and redrawn them together as one whole polygon. </li>
                    <%--<li id="SecondPolyExample"><img width="200" src="/Images/SecondPoly.jpg"/></li>--%>
                    <li> <strong>Example</strong> of Drawing a Second Part</li>
                    <li><img width="500" src="/Images/cat2_proc.jpg"/></li>
                    <%--<li> Example of Drawing a Inner Boundary</li>
                    <li><img width="500" src="/Images/cat2_proc.jpg"/></li>--%>
                    </ul>
                    </div>
                <div id="DrawingInstruction">
                <ul>
                    <li><strong>Click along</strong> one of the closing boundaries of the target object. </li>
                    <li><strong>If you can't draw on the image, try clearing your browser history/cache and restart your browser.</strong> </li>
                    <li><strong>When finished</strong>, click on &quot;Finish Boundary&quot;.</li>
                    <li><strong>To undo</strong> the last point clicked, click on &quot;Undo Last Point&quot;.</li>
                    <li>Boundaries <strong>shared by</strong> multiple objects, in some cases when objects are fully contained by another, will be automatically added so that you don't have to drawn them a second time.</li>
                    <li>Example Good Work:</li>
                    <li><img width="200" src="/Images/dog2.jpg"/></li>
                </ul>
                    </div>
                           
                                
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
         <asp:HiddenField ID="Hidden_Result" runat="server" /> 
         <asp:HiddenField ID="Hidden_BoundaryLines" runat="server" />             
            <asp:HiddenField ID="Hidden_Price" runat="server" /> 
            <asp:HiddenField ID="Hidden_TasksPerJob" runat="server" />
         <asp:HiddenField ID="Hidden_NoImagesDone" runat="server" Value="0" /> 
         <asp:HiddenField ID="Hidden_AmazonAssignmentID" runat="server" /> 
         <asp:HiddenField ID="Hidden_AmazonWorkerID" runat="server" /> 
         <asp:HiddenField ID="Hidden_HITID" runat="server" /> 
    </div>
    </form>
</body>
</html>

