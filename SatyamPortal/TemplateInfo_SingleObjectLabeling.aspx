<%@ Page Title="" Language="C#" MasterPageFile="~/JobTemplate.master" AutoEventWireup="true" CodeBehind="TemplateInfo_SingleObjectLabeling.aspx.cs" Inherits="SatyamPortal.TemplateInfo_SingleObjectLabeling" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table>
                        <tr>
                            <td>
                                 <h3>Template : Single Object Labeling</h3> 
                             </td>
                            <td class="auto-style1" style="width: 263px">
                                 <a href="Template_SingleObjectLabeling.aspx">Create a Job</a>
                             </td>                            
                        </tr>
                    </table>
                    <p>In this template, images are presented one by one and they asked to select the relevant category.</p>
    
                    <h3>Demo Example : (<a href="http://satyamresearchtaskpages.azurewebsites.net/SingleObjectLabelingDemoPage.aspx" target="_blank">click here </a>(opens in a new tab)</h3>
                     <p>Two categories "Car" and "Bus".</p> 
                     <p>The Description string can be HTML5 code or simply text to describe the categories. For example - &quot;Car: Includes SUV's, Vans with upto 6 passengers, Jeeps and Pickcups. Click <a href="http://satyamtaskpages.azurewebsites.net/CategoryExamplesPageForDemo.aspx"> here</a> to see examples of the various categories.&quot; </p>
    <p>&nbsp;Note: You will have to put an escape \ before the \"s in your HTML5 code.</p>
</asp:Content>
