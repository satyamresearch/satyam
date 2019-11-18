var annotationsJob;

$(document).ready(function () {

    if ($("#FinalDone_Hidden")[0].value == "false") {
        var document_ready_time = getDateTime();
        $("#DocumentReadyTime_Hidden").val(document_ready_time);
        /*the server passes on the file list to the 
        client using the hidden field, this function parses it 
        and populates frameURLList*/
        var job = job_import();
        annotationJob = new loadingscreen(job);
    }
    else {
        loadthankyoupage();
    }
});

