var ui_disabled = 0;

function ui_build(job)
{
    var screen = ui_setup(job);
    var videoframe = $("#videoframe");
    var player = new VideoPlayer(videoframe, job);
    var tracks = new TrackCollection(player, job);
    var objectui = new TrackObjectUI($("#newobjectbutton"), $("#objectcontainer"), videoframe, job, player, tracks);

    ui_setupbuttons(job, player, tracks);
    ui_setupslider(player);
    ui_setupsubmit(job, tracks);
    ui_setupskipjob();
    ui_setupsubmitanddonejob(job, tracks);
    ui_setupexitjob();
    ui_setupclickskip(job, player, tracks, objectui);
    ui_setupkeyboardshortcuts(job, player);
    // added back the load previous function
    ui_loadprevious(job, objectui);

    /*$("#newobjectbutton").click(function() {
        if (!mturk_submitallowed())
        {
            $("#turkic_acceptfirst").effect("pulsate");
        }
    });*/

    this.ui_post_track = function() {
        $("TracksOutput_Hidden").val(tracks.serialize());
    }

}


function ui_setup(job)
{
    var container = $("#AnnotationPageDivision");
    var screen = $("<div id='annotatescreen'></div>").appendTo(container);

    $("<table>" +
        "<tr>" +
            //"<td><div id='skipbar'></div><div id='submitbar'></div><div id='submitanddonebar'></div><div id='exitbar'></div><div id='instructionsbutton' class='button'>Instructions</div><div id='instructions'>Annotate all objects of interest, moving, stationary, or obstructed, for the entire video.</td>" +
        "<td><div id='skipbar'></div>" +
        "<div id='submitbar'></div>" +
        "<div id='submitanddonebar'></div>" +
        "<div id='exitbar'></div>" +
        "<div id='instructionsbutton' class='button'>Instructions</div>" +
        "<div id='instructions'></td>" +
            "<td><div id='topbar'></div></td>" +
        "</tr>" +
        "<tr>" +
              "<td><div id='videoframe'></div></td>" + 
              "<td rowspan='2'><div id='sidebar'></div></td>" +
          "</tr>" + 
          "<tr>" +
              "<td><div id='bottombar'></div></td>" + 
          "</tr>" +
          "<tr>" +
              "<td><div id='advancedoptions'></div></td>" +
//              "<td><div id='submitbar'></div></td>" +
          "</tr>" +
      "</table>").appendTo(screen).css("width", "100%");


    //var playerwidth = Math.max(720, job.width);
    var playerwidth = job.width;


   // $("#videoframe").css({"width": job.width + "px",
      //                    "height": job.height + "px",
      //                    "margin": "0 auto"})
       //             .parent().css("width", playerwidth + "px");

      $("#videoframe").css({"width": job.width + "px",
                        "height": job.height + "px",
                        "margin": "0 auto", 
                        "position" : "relative"})
                                       .parent().css("width", job.width + "px");

      var videoFrame = $("#videoframe")[0];

    $("#sidebar").css({"height": job.height + "px",
                       "width": "205px"});

    //$("#annotatescreen").css("width", (playerwidth + 205) + "px");
    $("#annotatescreen").css("width", (playerwidth) + "px");

    $("#bottombar").append("<div id='playerslider'></div>");
    $("#bottombar").append("<div class='button' id='rewindbutton'>Rewind</div> ");
    $("#bottombar").append("<div class='button' id='playbutton'>Play</div> ");
    $("#topbar").append("<div id='newobjectcontainer'>" +
        //"<div class='button' id='newobjectbutton'>New Object</div>"+
        "</div > ");

    $("<div id='objectcontainer'></div>").appendTo("#sidebar");

/*    $("<div class='button' id='openadvancedoptions'>Options</div>")
        .button({
            icons: {
                primary: "ui-icon-wrench"
            }
        }).appendTo($("#advancedoptions").parent()).click(function() {
                //eventlog("options", "Show advanced options");
                //$(this).remove();
                $("#advancedoptions").show();
            });*/

    //$("#advancedoptions").hide();



    //$("#advancedoptions").show();

    $("#advancedoptions").append(
    "<input type='checkbox' id='annotateoptionsresize'>" +
    "<label for='annotateoptionsresize'>Disable Resize</label> " +
    "<input type='checkbox' id='annotateoptionshideboxes'>" +
    "<label for='annotateoptionshideboxes'>Hide Boxes</label> " +
    "<input type='checkbox' id='annotateoptionshideboxtext'>" +
    "<label for='annotateoptionshideboxtext'>Hide Labels</label> ");

    $("#advancedoptions").append(
    "<div id='speedcontrol'>" +
    "<input type='radio' name='speedcontrol' " +
        "value='5,1' id='speedcontrolslower'>" +
    "<label for='speedcontrolslower'>Slower</label>" +
    "<input type='radio' name='speedcontrol' " +
        "value='15,1' id='speedcontrolslow'>" +
    "<label for='speedcontrolslow'>Slow</label>" +
    "<input type='radio' name='speedcontrol' " +
        "value='30,1' id='speedcontrolnorm' checked='checked'>" +
    "<label for='speedcontrolnorm'>Normal</label>" +
    "<input type='radio' name='speedcontrol' " +
        "value='90,1' id='speedcontrolfast'>" +
    "<label for='speedcontrolfast'>Fast</label>" +
    "</div>");

    //$("#skipbar").append("<div id='skipbutton' class='button'>Skip and Get Next Task</div>");
    $("#submitbar").append("<div id='submitbutton' class='button'>Submit and Get Next Task</div>");
    $("#submitanddonebar").append("<div id='submitanddonebutton' class='button'>Submit and Exit</div>");
    $("#exitbar").append("<div id='exitbutton' class='button'>Exit</div>");

    /*if (mturk_isoffline())
    {
        $("#submitbutton").html("Save Work");
    }*/

    drawLines(videoFrame, job.lines, 'red');

    return screen;
}

function ui_setupbuttons(job, player, tracks)
{
    $("#instructionsbutton").click(function() {
        player.pause();
        ui_showinstructions(job); 
    }).button({
        icons: {
            primary: "ui-icon-newwin"
        }
    });

    $("#playbutton").click(function() {
        if (!$(this).button("option", "disabled"))
        {
            player.toggle();

            if (player.paused)
            {
                //eventlog("playpause", "Paused video");
            }
            else
            {
                //eventlog("playpause", "Play video");
            }
        }
    }).button({
        disabled: false,
        icons: {
            primary: "ui-icon-play"
        }
    });

    $("#rewindbutton").click(function() {
        if (ui_disabled) return;
        player.pause();
        player.seek(player.job.start);
        //eventlog("rewind", "Rewind to start");
    }).button({
        disabled: true,
        icons: {
            primary: "ui-icon-seek-first"
        }
    });

    player.onplay.push(function() {
        $("#playbutton").button("option", {
            label: "Pause",
            icons: {
                primary: "ui-icon-pause"
            }
        });
    });

    player.onpause.push(function() {
        $("#playbutton").button("option", {
            label: "Play",
            icons: {
                primary: "ui-icon-play"
            }
        });
    });

    player.onupdate.push(function() {
        if (player.frame == player.job.stop)
        {
            $("#playbutton").button("option", "disabled", true);
        }
        else if ($("#playbutton").button("option", "disabled"))
        {
            $("#playbutton").button("option", "disabled", false);
        }

        if (player.frame == player.job.start)
        {
            $("#rewindbutton").button("option", "disabled", true);
        }
        else if ($("#rewindbutton").button("option", "disabled"))
        {
            $("#rewindbutton").button("option", "disabled", false);
        }
    });

    $("#speedcontrol").buttonset();
    $("input[name='speedcontrol']").click(function() {
        player.fps = parseInt($(this).val().split(",")[0]);
        player.playdelta = parseInt($(this).val().split(",")[1]);
        //console.log("Change FPS to " + player.fps);
        //console.log("Change play delta to " + player.playdelta);
        if (!player.paused)
        {
            player.pause();
            player.play();
        }
        //eventlog("speedcontrol", "FPS = " + player.fps + " and delta = " + player.playdelta);
    });

    $("#annotateoptionsresize").button().click(function() {
        var resizable = $(this).attr("checked") ? false : true;
        tracks.resizable(resizable);

        if (resizable)
        {
            //eventlog("disableresize", "Objects can be resized");
        }
        else
        {
            //eventlog("disableresize", "Objects can not be resized");
        }
    });

    $("#annotateoptionshideboxes").button().click(function() {
        //var visible = !$(this).attr("checked");
        var v = $(this)[0];
        var visible = v.checked;
        visible = !visible;
 
        tracks.visible(visible);

        if (visible)
        {
            //eventlog("hideboxes", "Boxes are visible");
        }
        else
        {
            //eventlog("hideboxes", "Boxes are invisible");
        }
    });

    $("#annotateoptionshideboxtext").button().click(function () {

        var v = $(this)[0];
        var visible = v.checked;
        visible = !visible;


        if (visible)
        {
            $(".boundingboxtext").show();
        }
        else
        {
            $(".boundingboxtext").hide();
        }
    });
}

function ui_setupkeyboardshortcuts(job, player)
{
    $(window).keypress(function(e) {
        //console.log("Key press: " + e.keyCode);

        if (ui_disabled)
        {
            //console.log("Key press ignored because UI is disabled.");
            return;
        }

        var keycode = e.keyCode ? e.keyCode : e.which;
        //eventlog("keyboard", "Key press: " + keycode);
        
        if (keycode == 32 || keycode == 112 || keycode == 116 || keycode == 98)
        {
            $("#playbutton").click();
        }
        if (keycode == 114)
        {
            $("#rewindbutton").click();
        }
        else if (keycode == 110)
        {
            $("#newobjectbutton").click();
        }
        else if (keycode == 104)
        {
            $("#annotateoptionshideboxes").click();
        }
        else 
        {
            var skip = 0;
            if (keycode == 44 || keycode == 100)
            {
                skip = job.skip > 0 ? -job.skip : -10;
            }
            else if (keycode == 46 || keycode == 102)
            {
                skip = job.skip > 0 ? job.skip : 10;
            }
            else if (keycode == 62 || keycode == 118)
            {
                skip = job.skip > 0 ? job.skip : 1;
            }
            else if (keycode == 60 || keycode == 99)
            {
                skip = job.skip > 0 ? -job.skip : -1;
            }

            if (skip != 0)
            {
                player.pause();
                player.displace(skip);

                ui_snaptokeyframe(job, player);
            }
        }
    });

}

function ui_canresize()
{
    return !$("#annotateoptionsresize").attr("checked"); 
}

function ui_areboxeshidden()
{
    return $("#annotateoptionshideboxes").attr("checked");
}

function ui_setupslider(player)
{
    var slider = $("#playerslider");
    slider.slider({
        range: "min",
        value: player.job.start,
        min: player.job.start,
        max: player.job.stop,
        slide: function(event, ui) {
            player.pause();
            player.seek(ui.value);
            // probably too much bandwidth
            //eventlog("slider", "Seek to " + ui.value);
        }
    });

    /*slider.children(".ui-slider-handle").hide();*/
    slider.children(".ui-slider-range").css({
        "background-color": "#868686",
        "background-image": "none"});

    slider.css({
        marginTop: "6px",
        width: parseInt(slider.parent().css("width")) - 200 + "px", 
        float: "right"
    });

    player.onupdate.push(function() {
        slider.slider({value: player.frame});
    });
}

function ui_iskeyframe(frame, job)
{
    return frame == job.stop || (frame - job.start) % job.skip == 0;
}

function ui_snaptokeyframe(job, player)
{
    if (job.skip > 0 && !ui_iskeyframe(player.frame, job))
    {
        console.log("Fixing slider to key frame");
        var remainder = (player.frame - job.start) % job.skip;
        if (remainder > job.skip / 2)
        {
            player.seek(player.frame + (job.skip - remainder));
        }
        else
        {
            player.seek(player.frame - remainder);
        }
    }
}

function ui_setupclickskip(job, player, tracks, objectui)
{
    if (job.skip <= 0)
    {
        return;
    }

    player.onupdate.push(function() {
        if (ui_iskeyframe(player.frame, job))
        {
            console.log("Key frame hit");
            player.pause();
            $("#newobjectbutton").button("option", "disabled", false);
            $("#playbutton").button("option", "disabled", false);
            tracks.draggable(true);
            tracks.resizable(ui_canresize());
            tracks.recordposition();
            objectui.enable();
        }
        else
        {
            $("#newobjectbutton").button("option", "disabled", true);
            $("#playbutton").button("option", "disabled", true);
            tracks.draggable(false);
            tracks.resizable(false);
            objectui.disable();
        }
    });

    $("#playerslider").bind("slidestop", function() {
        ui_snaptokeyframe(job, player);
    });
}

function ui_loadprevious(job, objectui)
{
    //var overlay = $('<div id="turkic_overlay"></div>').appendTo("#container");
    //var note = $("<div id='submitdialog'>One moment...</div>").appendTo("#container");


    //// the data here should be in json format
    //// {"label": path.labelid, 
    ////  "boxes": [tuple(x) for x in path.getboxes()],
    ////  "attributes": [(x.attributeid, x.frame, x.value) for x in path.attributes]}

    ////server_request("getboxesforjob", [job.jobid], function(data) {
    ////    overlay.remove();
    ////    note.remove();

    ////    for (var i in data)
    ////    {
    ////        objectui.injectnewobject(data[i]["label"],
    ////                                 data[i]["boxes"],
    ////                                 data[i]["attributes"]);
    ////    }
    ////});

    //overlay.remove();
    //note.remove();

    //data = [{
    //    "label": '0',
    //    "boxes": [[0, 0, 20, 20, 0, false, false], [50, 50, 80, 80, 10, false, false],[100, 100, 120, 120, 20, true, false]], //[(xtl, ytl, xbr, ybr, frame, outside, occluded)]
    //    "attributes": []// [(x.attributeid, x.frame, x.value)] 
    //}]; 

    //data = [{
    //    "label": 0,
    //    "boxes": [[126, 38, 131, 49, 0, false, false], [1261, 381, 1314, 497, 1, false, false], [1260, 381, 1313, 497, 2, false, false], [1259, 381, 1312, 497, 3, false, false], [1258, 381, 1311, 497, 4, false, false], [1257, 381, 1310, 497, 5, false, false], [1256, 381, 1309, 497, 6, false, false], [1255, 381, 1308, 497, 7, false, false], [1254, 381, 1307, 497, 8, false, false], [1253, 381, 1306, 497, 9, false, false], [1252, 381, 1305, 497, 10, false, false], [1251, 381, 1304, 497, 11, false, false], [1250, 381, 1303, 497, 12, false, false], [1249, 381, 1302, 497, 13, false, false], [1248, 381, 1301, 497, 14, false, false], [1247, 381, 1300, 497, 15, false, false], [1246, 381, 1299, 497, 16, false, false], [1245, 381, 1298, 497, 17, false, false], [1244, 381, 1297, 497, 18, false, false], [1243, 381, 1296, 497, 19, false, false], [1242, 381, 1295, 497, 20, false, false]],
    //    "attributes": []
    //}];

    //objectui.injectnewobject(0,
    //    [[0, 0, 20, 20, 0, false, false], [50, 50, 80, 80, 10, false, false],[100, 100, 120, 120, 20, true, false]],
    //    []);

    data = JSON.parse(job.prevTrace);

    frameCount = job.frameURLList.length;

    for (var i = 0; i < data.length;i++)
    {
        for (var j = 0; j < data[i]["boxes"].length;j++) {
            for (var k = 0; k < 4; k++) {
                data[i]["boxes"][j][k] = Math.floor(data[i]["boxes"][j][k] * job.scalingFactor);
            }
        }

        objectui.injectnewobject(data[i]["label"],
                                    data[i]["boxes"],
                                    data[i]["attributes"]);
    }
}

function ui_setupskipjob() {
    $("#skipbutton").button({
        icons: {
            primary: 'ui-icon-check'
        }
    }).click(function () {
        if (ui_disabled) return;
        ui_skip();
    });
}


function ui_skip() {
    if (!window.confirm("Are you sure you want to <b>skip</b> this job?" +
                    "If you sip, you will get another new video segment.")) {
        return;
    }
    else {
        $("#Skipped_Hidden").val("true");
        __doPostBack("<%= Skip_Button.ClientID%>", "OnClick");
    }
}


function ui_setupsubmit(job, tracks)
{
    $("#submitbutton").button({
        icons: {
            primary: 'ui-icon-check'
        }
    }).click(function() {
        if (ui_disabled) return;
        ui_submit(job, tracks);
    });
}


function ui_setupsubmitanddonejob(job, tracks) {
    $("#submitanddonebutton").button({
        icons: {
            primary: 'ui-icon-check'
        }
    }).click(function () {
        if (ui_disabled) return;
        $("#FinalDone_Hidden").val("true");
        ui_submit(job, tracks);
    });
}

function ui_setupexitjob() {
    $("#exitbutton").button({
        icons: {
            primary: 'ui-icon-check'
        }
    }).click(function () {
        if (ui_disabled) return;
        ui_exitjob();
    });
}


function ui_exitjob() {
    if (!window.confirm("Are you sure you want to finally exit?")) {
        return;
    }
    else {
        $("#Skipped_Hidden").val("true");
        $("#FinalDone_Hidden").val("true");
        __doPostBack("<%= Done_Button.ClientID%>", "OnClick");
    }
}


function ui_submit(job, tracks)
{
    //console.dir(tracks);
    //console.log("Start submit - status: " + tracks.serialize());

    /*if (!mturk_submitallowed())
    {
        alert("Please accept the task before you submit.");
        return;
    }*/

    //if (mturk_isassigned() && !mturk_isoffline())
    //{
    for (var j = 0; j < tracks.tracks.length; j++) {
        var tck = tracks.tracks[j];
        for (var i = 0; i < job.attributes[tck.label].length; i++) {
            var count = 0;
            for (var f in tck.attributejournals[i].annotations) {
                count++;
            }
            if (count==0) {
                var attr = job.attributes[tck.label][i];
                window.confirm("Please annotate " + attr + " for object " + tck.label);
                return;
            }
        }
    }
    
    

    if (!window.confirm("Are you sure you are ready to submit? Please " +
                        "make sure that the entire video is labeled and " +
                        "your annotations are correct.\n\nTo submit, " +
                        "press OK. Otherwise, press Cancel to keep " +
                        "working.")) {
        return;
    }
    else {
        var retString = tracks.serialize();
        $("#TracksOutput_Hidden").val(retString);
        __doPostBack("<%= Submit_Button.ClientID%>", "OnClick");
    }

    var overlay = $('<div id="turkic_overlay"></div>').appendTo("#container");
    ui_disable();

    var note = $("<div id='submitdialog'></div>").appendTo("#container");

    function validatejob(callback)
    {
      /*  server_post("validatejob", [job.jobid], tracks.serialize(),
            function(valid) {
                if (valid)
                {
                    console.log("Validation was successful");
                    callback();
                }
                else
                {
                    note.remove();
                    overlay.remove();
                    ui_enable();
                    console.log("Validation failed!");
                    ui_submit_failedvalidation();
                }
            });*/
    }

    function respawnjob(callback)
    {
        /*server_request("respawnjob", [job.jobid], function() {
            callback();
        });*/
    }
    
    function savejob(callback)
    {
       /* server_post("savejob", [job.jobid],
            tracks.serialize(), function(data) {
                callback()
            });*/
    }

    function finishsubmit(redirect)
    {
        if (mturk_isoffline())
        {
            window.setTimeout(function() {
                note.remove();
                overlay.remove();
                ui_enable();
            }, 1000);
        }
        else
        {
            window.setTimeout(function() {
                redirect();
            }, 1000);
        }
    }

    if (job.training)
    {
        console.log("Submit redirect to train validate");

        note.html("Checking...");
        validatejob(function() {
            savejob(function() {
                mturk_submit(function(redirect) {
                    respawnjob(function() {
                        note.html("Good work!");
                        finishsubmit(redirect);
                    });
                });
            });
        });
    }
    else
    {
        note.html("Saving...");
        savejob(function() {
            mturk_submit(function(redirect) {
                note.html("Saved!");
                finishsubmit(redirect);
            });
        });
    }
}

function ui_submit_failedvalidation()
{
    $('<div id="turkic_overlay"></div>').appendTo("#container");
    var h = $('<div id="failedverificationdialog"></div>')
    h.appendTo("#container");

    h.append("<h1>Low Quality Work</h1>");
    h.append("<p>Sorry, but your work is low quality. We would normally <strong>reject this assignment</strong>, but we are giving you the opportunity to correct your mistakes since you are a new user.</p>");
    
    h.append("<p>Please review the instructions, double check your annotations, and submit again. Remember:</p>");

    var str = "<ul>";
    str += "<li>You must label every object.</li>";
    str += "<li>You must draw your boxes as tightly as possible.</li>";
    str += "</ul>";

    h.append(str);

    h.append("<p>When you are ready to continue, press the button below.</p>");

    $('<div class="button" id="failedverificationbutton">Try Again</div>').appendTo(h).button({
        icons: {
            primary: "ui-icon-refresh"
        }
    }).click(function() {
        $("#turkic_overlay").remove();
        h.remove();
    }).wrap("<div style='text-align:center;padding:5x 0;' />");
}

function ui_showinstructions(job)
{
    //console.log("Popup instructions");

    if ($("#instructionsdialog").size() > 0)
    {
        return;
    }

    //eventlog("instructions", "Popup instructions");

//    $('<div id="turkic_overlay"></div>').appendTo("#container");
    $('<div id="turkic_overlay"></div>').appendTo("#LoadingPageDivision");

//    var h = $('<div id="instructionsdialog"></div>').appendTo("#container");
    var h = $('<div id="instructionsdialog"></div>').appendTo("#LoadingPageDivision");

    $('<div class="button" id="instructionsclosetop">Dismiss Instructions</div>').appendTo(h).button({
        icons: {
            primary: "ui-icon-circle-close"
        }
    }).click(ui_closeinstructions);

    //instructionsVisionZero(job, h)
    instructionsSatyam(job, h)

    ui_disable();
}

function ui_closeinstructions()
{
    //console.log("Popdown instructions");
    $("#turkic_overlay").remove();
    $("#instructionsdialog").remove();
    //eventlog("instructions", "Popdown instructions");

    ui_enable();
}

function ui_disable()
{
    if (ui_disabled++ == 0)
    {
        $("#newobjectbutton").button("option", "disabled", true);
        $("#playbutton").button("option", "disabled", true);
        $("#rewindbutton").button("option", "disabled", true);
        $("#submitbutton").button("option", "disabled", true);
        $("#playerslider").slider("option", "disabled", true);

        //console.log("Disengaged UI");
    }

    //console.log("UI disabled with count = " + ui_disabled);
}

function ui_enable()
{
    if (--ui_disabled == 0)
    {
        $("#newobjectbutton").button("option", "disabled", false);
        $("#playbutton").button("option", "disabled", false);
        $("#rewindbutton").button("option", "disabled", false);
        $("#submitbutton").button("option", "disabled", false);
        $("#playerslider").slider("option", "disabled", false);

        //console.log("Engaged UI");
    }

    ui_disabled = Math.max(0, ui_disabled);

    //console.log("UI disabled with count = " + ui_disabled);
}