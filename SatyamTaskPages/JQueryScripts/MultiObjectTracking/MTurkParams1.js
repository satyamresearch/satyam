function getMTurkParams(param, url) {
    var regexS = "[\?&]" + param + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var tmpURL = url;
    var results = regex.exec(tmpURL);
    if (results == null) {
        return "";
    } else {
        return results[1];
    }
}

