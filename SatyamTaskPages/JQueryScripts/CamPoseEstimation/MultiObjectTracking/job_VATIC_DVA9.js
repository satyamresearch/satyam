//var maxImageHeight = 720;
//var maxImageWidth = 1024;
//var maxImageHeight = 515;
//var maxImageWidth = 720;
var maxImageHeight = 600;
var maxImageWidth = 800;

var fixedYUIOffset = 85;


function Job() {
    var me = this;
    this.frameURLList = null;
    this.slug = null;
    this.start = null;
    this.stop = null;
    this.width = null;
    this.height = null;
    this.skip = null;
    this.perobject = null;
    this.completion = null;
    this.blowradius = null;
    this.thisid = null;
    this.labels = null;
    this.ImageList = null;
    this.fps = null;
    this.attributes = null;
    this.imageWidth = null;
    this.imageHeight = null;
    this.scalingFactor = null;
    this.lines = null;
}


function fill_attributes(attributes)
{
    var ret_array = new Array();
    var high_array = attributes.split(',');
    for (var i = 0; i < high_array.length; i++) {
        var atts = high_array[i].split(':');
        var details = atts[1].split('_');
        var detail_array = new Array();
        for(var j=0;j<details.length;j++)
        {
            if(details[j] != "")
            {
                detail_array.push(details[j]);
            }
        }
        ret_array.push(detail_array);                
    }
    return ret_array;
}

function fill_region(job, region)
{
    var ret_array = new Array();
    var lines = region.split(',');
    if (lines.length > 0)
    {
        for (var i = 0; i < lines.length; i++) {
            var lineCoords = lines[i].split('-');
            for (var j = 0; j < lineCoords.length - 3; j += 2) {
                var line = new Array();
                var val1 = lineCoords[j];
                var val1 = Math.floor(val1 * job.scalingFactor);
                //val1 = Math.floor(val1 );
                var val2 = lineCoords[j + 1] 
                var val2 = Math.floor(val2 * job.scalingFactor);
                //val2 = Math.floor(val2);
                var val3 = lineCoords[j+2];
                val3 = Math.floor(val3 * job.scalingFactor);
                //val3 = Math.floor(val3);
                var val4 = lineCoords[j + 3] 
                val4 = Math.floor(val4 * job.scalingFactor);
                //val4 = Math.floor(val4);
                line.push(val1);
                line.push(val2);
                line.push(val3);
                line.push(val4);
                ret_array.push(line);
            }
        }
        return ret_array;
    }
    else {
        return null;
    }
}

function job_import_new() {
    var job = new Job();

    var urlListString = $('#Hidden_ImageURLList').val();
    job.frameURLList = urlListString.split(',');
    job.slug = $('#Slug_Hidden').val();
    job.start = parseInt($('#Start_Hidden').val());
    job.stop = parseInt($('#Stop_Hidden').val());
    job.imageWidth = parseInt($('#ImageWidth_Hidden').val());
    job.imageHeight = parseInt($('#ImageHeight_Hidden').val());
    job.skip = parseInt($('#Skip_Hidden').val());
    job.perobject = parseFloat($('#PerObject_Hidden').val());
    job.completion = parseFloat($('#Completion_Hidden').val());
    job.blowradius = parseInt($('#BlowRadius_Hidden').val());
    job.jobid = parseInt($('#JobId_Hidden').val());
    job.labels = $('#LabelString_Hidden').val().split(',');
    job.training = $('#Training_Hidden').val();
    job.fps = $('#fps_Hidden').val();
    job.attributes = fill_attributes($('#Attributes_Hidden').val());
    job.width = maxImageWidth;
    job.height = maxImageHeight;

    //usually width is greater than height and hence
    if (job.imageWidth <= job.width) {
        job.scalingFactor = 1;
        job.width = job.imageWidth;
        job.height = job.imageHeight;
    }
    else {
        //imageWidth = maxImageWidth;
        job.scalingFactor = job.width / job.imageWidth;
        job.height = Math.ceil(job.imageHeight * job.scalingFactor);
    }

    job.lines = fill_region(job,$('#RegionString_Hidden').val())
    return job;
}


function job_import() {
    var job = new Job();

    var urlListString = $('#ImageURLList_Hidden').val();
    job.frameURLList = urlListString.split(',');

    var jobString = $('#JobParameters_Hidden').val();
    var jobParamsArray = jobString.split(',');

    var index = 0;
    job.slug = jobParamsArray[index + 1];
    index = index + 2;
    job.start = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    job.stop = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    job.imageWidth = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    job.imageHeight = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    job.skip = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    job.perobject = parseFloat(jobParamsArray[index + 1]);
    index = index + 2;
    job.completion = parseFloat(jobParamsArray[index + 1]);
    index = index + 2;
    job.blowradius = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    job.jobid = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    var labels_count = parseInt(jobParamsArray[index + 1]);
    var labels_array = new Array();
    for (var i = index + 2; i < index + 2 + labels_count; i++) {
        labels_array.push(jobParamsArray[i]);
    }
    index = index + 2 + labels_count + 1;
    job.labels = labels_array;
    job.training = parseFloat(jobParamsArray[index + 1]);
    index = index + 2;
    job.fps = parseInt(jobParamsArray[index + 1]);
    index = index + 2;
    var attribute_array = new Array();
    for (var i = 0; i < labels_count; i++) {
        index = index + 1;
        var details = new Array();
        var count = parseInt(jobParamsArray[index + 1]);
        index = index + 1;
        for (var j = 0; j < count; j++)
        {
            details.push(jobParamsArray[index + 1]);
            index = index + 1;
        }
        attribute_array.push(details);
    }
    job.attributes = attribute_array;
    index = index + 2;
    job.width = parseInt(jobParamsArray[index]);
    index = index + 2;
    job.height = parseInt(jobParamsArray[index]);
    job.scalingFactor = job.width/job.imageWidth;
    job.height = Math.ceil(job.imageHeight * job.scalingFactor);
    index = index + 2;
    var linesCnt = parseInt(jobParamsArray[index]);
    index = index +1;
    if (linesCnt > 0)
    {
        job.lines = new Array();
        for(var i=0;i<linesCnt;i++)
        {
            var line = new Array();
            for (var j = 0; j < 4; j++) {
                var val = jobParamsArray[index + j];
                val = Math.floor(val*job.scalingFactor)
                line.push(val);
            }
            job.lines.push(line);
            index = index + 4;
        }
    }
    return job;
}







