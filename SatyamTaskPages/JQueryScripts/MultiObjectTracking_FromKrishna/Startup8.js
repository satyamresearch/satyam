var annotationsJob;

$(document).ready(function () {

    var document_ready_time = getDateTime();
    $("#DocumentReadyTime_Hidden").val(document_ready_time);

    if ($("#AmazonTurk_Hidden")[0].value == "true") {
        var url = window.location.href;
        var assignmentID = getMTurkParams('assignmentId',url);
        var hitID = getMTurkParams('hitId', url);
        var workerID = getMTurkParams('workerId', url);

        //$("#DebugPageDivision")[0].textContent = "url = " + url + " assignmentID = " + assignmentID + " workerID = " + workerID;

        if (assignmentID == "" || assignmentID == "ASSIGNMENT_ID_NOT_AVAILABLE") //the turker has not yet accepted the task
        {
            preacceptscreenMTurk();
            //preloadingscreenMTurk();
        }
        else {
            $("#AmazonMTurkAssigmentID_Hidden").val(assignmentID);
            $("#AmazonMTurkWorkerID_Hidden").val(workerID);
            $("#AmazonMTurkHitID_Hidden").val(hitID);
            startTaskMTurk();
        }
        
    } else {
        startTask();
    }


    
});


function prepare()
{
    boxes = new Array();
    ImageContainer = $("#ImageDivision")[0];    
    Image = $("#DisplayImage");
    //sleep(500);
    displayWidth = Image[0].clientWidth;
    displayHeight = Image[0].clientHeight;
    imageWidth = Image[0].naturalWidth;
    imageHeight = Image[0].naturalHeight;
    scalex = imageWidth / displayWidth;
    scaley = imageHeight / displayHeight;
    DrawBoundary();
    
    $("#buttonTable").show();
    //Image.load(function () { DrawBoundary(); });
    

    $("#CategorySelection_RadioButtonList")[0].disabled = true;
    $("#CategorySelection_RadioButtonList_0")[0].checked = true;
   // $("#CategorySelectionPanel").hide();    
    $("#attentionDiv").hide();
    var checked_radio = $("[id*=CategorySelection_RadioButtonList] input:checked");
    category = checked_radio.val();
    $("#CategoryMentionLabel")[0].innerHTML = category;
}

function startTaskMTurk() {
    if ($("#FinalDone_Hidden")[0].value == "true") {
        loadthankyoupage();
    }
    else if ($("#LablingTaskStarted_Hidden")[0].value == "false") {
        preloadingscreenMTurk();
    }
    else {
        /*the server passes on the file list to the 
        client using the hidden field, this function parses it 
        and populates frameURLList*/
        var job = job_import_new();
        annotationJob = new loadingscreen(job);
    }
}

function startTask() {
    if ($("#FinalDone_Hidden")[0].value == "true") {
        loadthankyoupage();
    }
    else if ($("#LablingTaskStarted_Hidden")[0].value == "false") {
        preloadingscreen();
    }
    else {
        /*the server passes on the file list to the 
        client using the hidden field, this function parses it 
        and populates frameURLList*/
        var job = job_import_new();
        annotationJob = new loadingscreen(job);
    }
}

