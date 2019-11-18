<%@ Page Title="" Language="C#" MasterPageFile="~/JobTemplate.master" AutoEventWireup="true" CodeBehind="Template_MultipleObjectsTracking.aspx.cs" Inherits="SatyamPortal.Template_MultipleObjectsTracking" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table>
        <tr>
            <td>
                <h3>Template : Multiple Objects Tracking</h3> 
            </td>
            <td class="auto-style1" style="width: 263px">
                <a href="TemplateInfo_MultipleObjectsTracking.aspx">Learn About this Template</a>
            </td> 
            <td class="auto-style2" style="width: 180px">
                <strong>Job GUID</strong> :<asp:Label ID="NewJobGUID" runat="server" Text="0"></asp:Label>
            </td>
            <td>
                <asp:Button ID="JobSubmitButton" runat="server" Text="Submit" OnClick="JobSubmitButton_Click"/>
            </td>
        </tr>
    </table>

    <div id="ErrorField"> 
            <asp:Label ID="Template_ErrorMessageLabel" runat="server" Text=""></asp:Label>
                    </div>
                    
                <h3>Image Location Information</h3> (Note : All the images in this location will be launched for labeling automatically. The results file will be stored in the same location.)
                   <table>
                   <tr><td>
                   <table>
                       <tr>
                           <td>
                               Azure Blob Storage Connection String :
                           </td>
                           <td>
                               
                               <asp:TextBox id="AzureBlobStorageConnectionStringTextBox" runat="server" Width="284px"></asp:TextBox>
                               
                           </td>
                           
                      </tr>
                       <tr>
                           <td>
                               Azure Blob Storage Container Name :
                           </td>
                           <td>
                               
                               <asp:TextBox id="AzureBlobStorageContainerNameTextBox" runat="server" Width="284px"></asp:TextBox>
                               
                           </td>
                           
                      </tr>
                       <tr>
                           <td>
                               Azure Blob Storage Container Directory Name (optional) :
                           </td>
                           <td>                               
                               <asp:TextBox id="AzureBlobStorageContainerDirectoryNameTextBox" runat="server" Width="284px"></asp:TextBox>                               
                           </td>
                           
                      </tr>
                       <tr>
                           <td>
                               Input Data Format:
                           </td>
                           <td>                               
                               <asp:RadioButton ID="InputFormatVideo" runat="server" Text="Video: a directory of video files." Width="284px" GroupName="InputFormatRadioButtonGroup" Checked="True">
                               </asp:RadioButton>
                               <br>
                               <asp:RadioButton ID="InputFormatImage" runat="server" Text="Image: a directory of folders of frames, where each folder represents one video." Width="284px" GroupName="InputFormatRadioButtonGroup">
                               </asp:RadioButton>
                           </td>
                           
                      </tr>

                       
                   </table>
                    </td>
                    <td>
                               <asp:Button ID="AzureStorageLocation_ValidateButton" runat="server" Text="Validate" Width="110px" OnClientClick="return Template_Azure_Location_Check();"/>

                    </td>
                    </tr>
                   </table>
                    <table>
                        <tr>
                            <td>
                                <h3>Using Amazon Mechanical Turk?</h3>
                            </td>
                            <td>

                                <asp:CheckBox ID="UseAmazonMTurkCheckBox" runat="server" onClientClick="return onAmazonMechanicalTurkCheckBoxChanged();" AutoPostBack="True" OnCheckedChanged="UseAmazonMTurkCheckBox_CheckedChanged"/>

                            </td>
                        </tr>
                    </table>
                     <asp:Panel id="AmazonInformationPanel" runat="server" Visible="False">   
                    <table><tr>
                        <td>
                   <table>
                       <tr>
                           <td>
                               Amazon Access Key ID :
                           </td>
                           <td>
                               
                               <asp:TextBox id="AmazonAccessKeyID" runat="server" Width="284px"></asp:TextBox>
                               
                           </td>
                      </tr>
                       <tr>
                           <td>
                               Amazon Secret Access Key ID :
                           </td>
                           <td>
                               
                               <asp:TextBox id="AmazonSecretAccessKeyID" runat="server" Width="284px"></asp:TextBox>
                               
                           </td>
                      </tr>                       
                   </table>
                   </td>
                   <td>
                      <asp:Button ID="AmazonAccount_ValidateButton" runat="server" Text="Validate" Width="110px" onClientClick="return Template_Amazon_Information_Check();"/>
                   </td>
                        <td>
                        (Validates and tells you how much money is left in the account!) 
                            </td>
                   </tr>
                  </table>
                    </asp:Panel>
                    <h3>Categories and Subcategories for Labeling</h3>
                   To add new category : Type the desired category into the text box and add new category.<br/>
                   To delete category : Select the desired category in the listbox and click delete.<br/>
                   To add new sub-category : Select the desired category and then type the desired category into the text box and add new sub-category.<br/>
                   To delete sub-category : Select the desired sub-category in the listbox and click delete.<br/>


                <table>
                   <tr>
                      <td>
                       <table align="center">
                        <tr>
                            <td class="auto-style2">
                                <asp:TextBox ID="AddCategoryTextBox" runat="server" Height="29px" Width="145px"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                           <td>
                            <asp:Button ID="AddCategoryButton" runat="server" Text="Add New Class" Width="102px" OnClick="AddCategoryButton_Click" />
                            </td>
                        </tr>
                        <tr>
                           <td>
                             <asp:Button ID="DeleteCategoryButton" runat="server" Text="Delete Class" OnClick="DeleteCategoryButton_Click" />
                            </td>
                        </tr>
                        </table>
                        </td>
                        <td class="auto-style3">
                            <table>
                                <tr>
                                    <td class="auto-style4">
                                        <p style="font-weight: bold; font-size: large; ">Categories</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="auto-style4">
                                        <asp:ListBox ID="CategoryListBox" runat="server" Width="151px" AutoPostBack="True"></asp:ListBox>
                                    </td>
                                </tr>
                            </table>
                        </td>
                     </tr>
                    </table>
                   <table>
                       <tr><td>
                           <strong>Description Content</strong> (You can write text to explain the various categories and subcategories. You can add html5 code if you feel compelled e.g. to provide example images etc.)
                        </td></tr>
                        <tr><td>                                            
                           <asp:TextBox ID="CategoryDescription" runat="server" Height="52px" Width="780px"></asp:TextBox>                                            
                        </td></tr>
                   </table>
                   
                    <table>
                        <tr>
                            <td>
                                <table>
                                    <tr><td>
                                        <strong>Boundary of Interest in Image (Optional) </strong> 
                                    </td></tr>
                                    <tr><td>
                                        <a href="Template_MultipleObjectTracking.aspx">See template description for information</a>
                                    </td></tr>
                                 </table>
                            </td>
                            <td>
                                <strong>:</strong>
                            </td>
                            <td>

                                <asp:TextBox ID="BoundaryString" runat="server" Height="30px" Width="761px"></asp:TextBox>

                            </td>
                        </tr>
                    </table>
                    
                    <table>
                        <tr>
                            <td>
                                <strong>Desired Ground Truth Frame Rate</strong> : (frames/sec) 
                            </td>
                            <td>

                                <asp:TextBox ID="TargetFrameRate" runat="server" Width="206px">10</asp:TextBox>

                            </td>
                       </tr>
                    </table>

                    <table>
                        <tr>
                            <td class="auto-style2" style="width: 204px">
                                <strong>Video Chunk Duration</strong> : (sec) 
                            </td>
                            <td>

                                <asp:TextBox ID="ChunkDuration" runat="server" Width="206px">3</asp:TextBox>

                            </td>
                            <td>
                                (Your video will be broken into small managable pieces. If there are lots of objects in the video and lot of movement e.g. traffic surveillance on a busy road then use 10sec chucks. It is recommended not to use more than 30 seconds.)
                            </td>
                       </tr>
                    </table>


                    <table>
                        <tr>
                            <td class="auto-style2" style="width: 204px">
                                <strong>Chunk Overlap Duration</strong> : (sec) 
                            </td>
                            <td>

                                <asp:TextBox ID="ChunkOverlap" runat="server" Width="206px">0.5</asp:TextBox>

                            </td>
                            <td>
                                (To stitch the results of multiple chunks, a longer overlap help at a higher total cost. It is recommended to use 0.5 seconds.)
                            </td>
                       </tr>
                    </table>

                   <asp:Panel id="PricePanel" runat="server" Visible="False">   
                      <table>
                      <tr>                          
                          <td>   
                              <strong>Average Number of Objects Per Chunk</strong>: 
                         </td>
                          <td>
                              <asp:TextBox id="NoObjectsPerChunkTextBox" runat="server">1</asp:TextBox>
                          </td>                        
                      </table>
                       <table>
                      <tr>
                          <td>   
                              <strong>Price per Object</strong>(cents/chunk): 
                         </td>
                          <td>

                              <asp:TextBox id="PriceTextBox" runat="server">1.5</asp:TextBox>

                          </td>
                          <td>
                               (recommended price is 1.5 cents/object/chunk)
                          </td>
                      </table>
                       <table>
                      <tr>
                          <td class="auto-style1" style="width: 203px">
                              <strong>MTurk Task Title</strong> (optional) : 
                          </td>
                          <td>

                              <asp:TextBox ID="AmazonTaskTitleTextBox" runat="server" Width="336px">Track Objects in a Video</asp:TextBox>

                          </td>
                          <td>
                              (A short title for your task, 2-3 words long, the turkers might search for this title if they like the job. Default is <strong>Image Classification Task</strong>)
                          </td>
                      </tr>
                      <tr>
                          <td class="auto-style1" style="width: 203px">
                              <strong>MTurk Task Description</strong> (optional): 
                          </td>
                          <td>
                              <asp:TextBox ID="AmazonTaskDescriptionTextBox" runat="server" Width="336px">Quickly earn money by tracking objects in Video! </asp:TextBox>
                          </td>
                          <td>
                            (A short catchy description to entice workers, typically a line)
                          </td>
                      </tr>
                      <tr>
                          <td class="auto-style1" style="width: 203px">
                              <strong>MTurk Keywords</strong> (optional): 
                          </td>
                          <td>
                              <asp:TextBox ID="AmazonTaskKeywordsTextBox" runat="server" Width="336px">video, tracking objects</asp:TextBox>
                          </td>
                          <td>
                            (Turkers might search on keywords - enter 3-5 keywords seperated by commas e.g. images, classify)
                          </td>
                      </tr>
                  </table>
                  </asp:Panel>
    <div>
         <asp:HiddenField ID="Hidden_SubCategories" runat="server" />   
         <asp:HiddenField ID="Hidden_MechanicalTurk" runat="server" value="false"/>   
    </div>
</asp:Content>
