<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TrackletLabeling_MTurk.aspx.cs" Inherits="SatyamTaskPages.TraceletLabeling_MTurk" %>

<!DOCTYPE html>


<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="jquery-ui-1.11.4.custom/external/jquery/jquery.js"></script>
    <script src="jquery-ui-1.11.4.custom/jquery-ui.js"></script>
    <link href="jquery-ui-1.11.4.custom/jquery-ui.min.css" rel="stylesheet" />
    <link href="JQueryScripts/TrackletLabeling/stylesheet5.css" rel="stylesheet" />
    <script src="JQueryScripts/TrackletLabeling/DateTime.js"></script>                            
    <script src="JQueryScripts/TrackletLabeling/Startup8.js"></script>
    <script src="JQueryScripts/TrackletLabeling/job_VATIC_DVA9.js"></script>
    <script src="JQueryScripts/TrackletLabeling/LoadingScreen_VATIC_DVA4.js"></script>
    <script src="JQueryScripts/TrackletLabeling/ui_VATIC_DVA27.js"></script>
    <script src="JQueryScripts/TrackletLabeling/instructions_VATIC_DVA8.js"></script>
    <script src="JQueryScripts/TrackletLabeling/preload_VATIC_DVA.js"></script>
    <script src="JQueryScripts/TrackletLabeling/videoplayer_VATIC_DVA1.js"></script>
    <script src="JQueryScripts/TrackletLabeling/tracks_VATIC_DVA12.js"></script>
    <script src="JQueryScripts/TrackletLabeling/objectui_VATIC_DVA10.js"></script>
    <script src="JQueryScripts/TrackletLabeling/ThankYouPage1.js"></script>
    <script src="JQueryScripts/TrackletLabeling/PreLoadingScreen_VATIC_DVA4.js"></script>
    <script src="JQueryScripts/TrackletLabeling/DrawRegions7.js"></script>
    <script src="JQueryScripts/TrackletLabeling/GetElementPosition.js"></script>
    <script src="JQueryScripts/TrackletLabeling/MTurkParams1.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="AcceptancePageDivision"></div>
        <div id="PreLoadingPageDivision"></div>
        <div id="LoadingPageDivision"></div>
        <div id="AnnotationPageDivision">
             <asp:Button ID="Submit_Button" runat="server" Text="Next Annotation" Visible="false" />
             <asp:Button ID="Skip_Button" runat="server" Text="SKip This Annotation" Visible="false" />
             <asp:Button ID="Done_Button" runat="server" Text="Exit" Visible="false" />
        </div>
        <div>
            <asp:HiddenField ID="Hidden_TaskEntryString" runat="server" />  
            <asp:HiddenField ID="Hidden_PageLoadTime" runat="server" />  
            <asp:HiddenField ID="TracksOutput_Hidden" runat="server" /> 
            <%--<asp:HiddenField ID="Hidden_BoundaryLines" runat="server" /> --%>
            <asp:HiddenField ID="Hidden_ImageURLList" runat="server" /> 
            <asp:HiddenField ID="Hidden_ChunkDuration" runat="server" /> 
            <asp:HiddenField ID="Hidden_AmazonAssignmentID" runat="server" /> 
            <asp:HiddenField ID="Hidden_AmazonWorkerID" runat="server" /> 
            <asp:HiddenField ID="Hidden_HITID" runat="server" /> 
            <asp:HiddenField ID="Hidden_Price" runat="server" /> 
          
            <asp:HiddenField ID="Slug_Hidden" runat="server" />
            <asp:HiddenField ID="Start_Hidden" runat="server" />
            <asp:HiddenField ID="Stop_Hidden" runat="server" />
            <asp:HiddenField ID="ImageWidth_Hidden" runat="server" />
            <asp:HiddenField ID="ImageHeight_Hidden" runat="server" />
            <asp:HiddenField ID="Skip_Hidden" runat="server" />
            <asp:HiddenField ID="PerObject_Hidden" runat="server" />
            <asp:HiddenField ID="Completion_Hidden" runat="server" />
            <asp:HiddenField ID="BlowRadius_Hidden" runat="server" />
            <asp:HiddenField ID="JobId_Hidden" runat="server" />
            <asp:HiddenField ID="LabelString_Hidden" runat="server" />
            <asp:HiddenField ID="Training_Hidden" runat="server" />
            <asp:HiddenField ID="fps_Hidden" runat="server" />
            <asp:HiddenField ID="Attributes_Hidden" runat="server" />
            <asp:HiddenField ID="RegionString_Hidden" runat="server" />
          
            <asp:HiddenField ID="Skipped_Hidden" runat="server" value="false"/>  
            <asp:HiddenField ID="FinalDone_Hidden" runat="server" value="false"/> 
            <asp:HiddenField ID="LabelingTaskStarted_Hidden" runat="server" value="true"/>
            
            <asp:HiddenField ID="AmazonTurk_Hidden" runat="server" value="false"/>


            <asp:HiddenField ID="PreviousTrackString_Hidden" runat="server" />
      </div>
    </form>
</body>
</html>
