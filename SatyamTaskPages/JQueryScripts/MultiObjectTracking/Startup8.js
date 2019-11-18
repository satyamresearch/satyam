var annotationsJob;

$(document).ready(function () {

    startTaskMTurk();
    
});


function startTaskMTurk() {
    if ($("#FinalDone_Hidden")[0].value == "true") {
        loadthankyoupage();
    }
    else if ($("#LabelingTaskStarted_Hidden")[0].value == "false") {
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

//function startTask() {
//    if ($("#FinalDone_Hidden")[0].value == "true") {
//        loadthankyoupage();
//    }
//    else if ($("#LabelingTaskStarted_Hidden")[0].value == "false") {
//        preloadingscreen();
//    }
//    else {
//        /*the server passes on the file list to the 
//        client using the hidden field, this function parses it 
//        and populates frameURLList*/
//        var job = job_import_new();
//        annotationJob = new loadingscreen(job);
//    }
//}

