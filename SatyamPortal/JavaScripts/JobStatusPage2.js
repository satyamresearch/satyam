function DisplayJobStatus()
{
    var userId = "Admin";
    var request = userId;
    $.ajax({
        type: "POST",
        url: "WebServiceHelpers.aspx/getJobStatus",
        data: "{'request': '" + request + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: displayJobStatus,
        failure: function (response) {
            alert(response.d);
        }
    });
    return false;
}


function displayJobStatus(response)
{
    var obj = JSON.parse(response.d);
    //var tablediv = document.getElementById("jobstatusdivision");
    var tablediv = $("#jobstatusdivision");
    var tableStr = '<table border=1 style="width:100%"><tr><td>GIUD</td><td>Template Type</td><td>Submit Time</td><td>Status</td></tr>';

    tablediv.append();    
    for (var i = 0; i < obj.length; i++)
    {
        tableStr += ('<tr><td>' + obj[i].JobGUID + '</td><td>' + obj[i].JobTemplateType + '</td></td>' + obj[i].JobSubmitTime + '</td><td>' + obj[i].JobStatus + '</td></tr>');
    }
    tableStr += '</table>';
    tablediv.append(tableStr);
    return false;
}