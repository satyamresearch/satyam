$(document).ready(function () {
    prepare();
});

function prepare() {
    var video_uri = $("#Hidden_VideoURL")[0].value;
    var videoDiv = $("#VideoDivision");

    var splitFromExtension = video_uri.split(".");
    var ext = splitFromExtension[splitFromExtension.length-1];

    if (ext == 'mp4' || ext == "MP4") {
        videoDiv.append('<video width="720" controls> <source src="' + video_uri + '" type="video/mp4">');
    }
}