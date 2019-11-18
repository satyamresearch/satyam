function loadingscreen(job) {
    var ui;
    var ls = $("<div id='loadingscreen'></div>");
    ls.append("<div id='loadingscreeninstructions' class='button'>Show " +
        "Instructions</div>");
    ls.append("<div id='loadingscreentext'>Downloading the video...</div>");
    ls.append("<div id='loadingscreenslider'></div>");

    //ls.append("<div class='loadingscreentip'>You are welcome to work on " +
     //   "other tasks/HITs while you wait for the download to complete. When the " +
     //   "download finishes, we'll play a gentle musical tune to notify " +
     //   "you.</div>");

    $("#LoadingPageDivision").append(ls);
    //$("#container").html(ls);
//    container.html(ls);

//    if (!development && !mturk_isoffline()) {
//        ui_showinstructions(job);
    //    }

//    ui_showinstructions(job);

    $("#loadingscreeninstructions").button({
        icons: {
            primary: "ui-icon-newwin"
        }
    }).click(function () {
        ui_showinstructions(job);
    });

  //  eventlog("preload", "Start preloading");

    //    preloadvideo(job.start, job.stop, job.frameurl,
    preloadvideo(job.start, job.stop, job.frameURLList,
        preloadslider($("#loadingscreenslider"), function (progress) {
            if (progress == 1) {
                //if (!development && !mturk_isoffline()) {
                $("body").append('<div id="music"><embed src="JQueryScripts/small_step.mp3">' +
                        '<noembed><bgsound src="JQueryScripts/Music/small_step.mp3"></noembed></div>');

                    window.setTimeout(function () {
                        $("#music").remove();
                    }, 2000);
                //}

                ls.remove()
                var images_loaded_time = getDateTime();
                $("#ImagesLoadedTime_Hidden").val(images_loaded_time);

                ui = new ui_build(job);

               // mturk_enabletimer();

                //eventlog("preload", "Done preloading");
            }
        })
    );

    this.ui_post_track = function () {
        ui.ui_post_track();
    }
}