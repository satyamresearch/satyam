<%@ Page Language="C#" MasterPageFile="~/JobTemplate.master" AutoEventWireup="true" CodeBehind="Template_OpenEndedQuestionsImage.aspx.cs" Inherits="SatyamPortal.Template_OpenEndedQuestionsImage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id ="Template_OpenEndedQuestionsImage">        
                    <table>
                        <tr>
                            <td>
                                 <h3>Template : Open-ended Questions for Image</h3> 
                             </td>
                            <td class="auto-style1" style="width: 263px">
                                <asp:Panel id="TemplateInfo" runat="server" Visible="True">
                                 <a href="TemplateInfo_OpenEndedQuestionsImage.aspx">Learn About this Template</a>
                                </asp:Panel>
                                <asp:Panel id="TemplateInfoMturk" runat="server" Visible="False">
                                 <a href="TemplateInfo_OpenEndedQuestionsImage.aspx">Learn About this Template</a>
                                </asp:Panel>
                             </td>
                            <td class="auto-style2" style="width: 180px">
                                <strong>Job GUID</strong> :<asp:Label ID="NewJobGUID" runat="server" Text="0"></asp:Label>
                            </td>
                            <td>
                                <asp:Button ID="JobSubmitButton" runat="server" Text="Submit Job" OnClick="OpenEndedQuestion_JobSubmitButton_Click"/>                            
                            </td>
                        </tr>
                    </table>
                   
&nbsp;
                   
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
                  
                   <table>
                       <tr><td>
                           <strong>Description Content</strong> (You can write text to explain the various categories and even add html5 code if you feel compelled e.g. to provide example images etc.)
                        </td></tr>
                        <tr><td>                                            
                           <asp:TextBox id="CategoryDescription" runat="server" Height="52px" Width="810px"></asp:TextBox>                                            
                        </td></tr>
                   </table>
                  <asp:Panel id="PricePanel" runat="server" Visible="False">   
                      <table>
                      <tr>
                          <td>   
                              <strong>Price per Image</strong>(cents/image): 
                         </td>
                          <td>

                              <asp:TextBox id="PriceTextBox" runat="server">1</asp:TextBox>

                          </td>
                          <td>
                               (recommended price is 0.1 cents/image. Jobs will be launched in sets of 20 images. Actual cost will be 3 times this on average since the same image is given to at least 3 people.)
                          </td>
                      </table>
                      <table>
                      <tr>
                          <td class="auto-style1" style="width: 203px">
                              <strong>MTurk Task Title</strong> (optional) : 
                          </td>
                          <td>

                              <asp:TextBox ID="AmazonTaskTitleTextBox" runat="server" Width="336px">Open-ended Questions for Image</asp:TextBox>

                          </td>
                          <td>
                              (A short title for your task, 2-3 words long, the turkers might search for this title if they like the job. Default is <strong>Open-ended Questions for Image</strong>)
                          </td>
                      </tr>
                      <tr>
                          <td class="auto-style1" style="width: 203px">
                              <strong>MTurk Task Description</strong> (optional): 
                          </td>
                          <td>
                              <asp:TextBox ID="AmazonTaskDescriptionTextBox" runat="server" Width="336px">Quickly earn money by answering questions about an image!</asp:TextBox>
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
                              <asp:TextBox ID="AmazonTaskKeywordsTextBox" runat="server" Width="336px">Pictures, Images, Captioning, Q&A</asp:TextBox>
                          </td>
                          <td>
                            (Turkers might search on keywords - enter 3-5 keywords seperated by commas e.g. images, classify)
                          </td>
                      </tr>
                  </table>
                  </asp:Panel>
                  </div>
     <div>
         <asp:HiddenField ID="Hidden_MechanicalTurk" runat="server" value="false"/>   
    </div>
</asp:Content>
