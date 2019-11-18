function preacceptscreenMTurk() {

    var pls = $("<div id='preloadingscreen'></div>");
    pls.append("<table><tr><td><b>Please read before you accept the task</b><br>" +
    "<br><b>Task : </b>In this task, we ask you to <b>track various traffic entities</b> such as pedestrians, bicycles, cars etc. and label them appropriately. You have to draw a tightly fitting box around each of these objects and move the boxes with the objects as they move in the video.<br>" +
    "<b>Training Video : </b> The video below shows you a typical task and it also serves as a training video. Alternatively, you can watch it <a href= https://youtu.be/IcQIAdZ55fA target='_blank'> here </a></b>. (opens in new tab)<br>" +
    "<br><b> Payment </b> : You will be paid 15 cents for each moving object and 2 cents for each stationary object that you correctly track. <note style='color: red;'>You will be paid only if you do at least 3 moving objects.</note> For example if you track 5 moving objects and 2 stationary objects correctly you earn 79 cents ie. 45 cents base price + 34 cents bonus.<br> " +
  //  "<br><note style='color: red;'> <b>Note :</b> Your work will be manually evaluated and will effect your rating.</note>" +
    "<br>Please carefully review the training video before you start." +
    "<br>Your task will start once you accept.</note></td><br>");    
//    pls.append('<iframe title="YouTube video player" width="560" height="349" src="https://youtu.be/4GZmjv5N7hg?rel=0&autoplay=1&loop=1" frameborder="0" allowfullscreen></iframe>');
    //pls.append('<iframe width="560" height="315" src="https://www.youtube.com/embed/4GZmjv5N7hg?rel=0&autoplay=1&loop=1" frameborder="0" allowfullscreen></iframe>');
    pls.append('<iframe width="560" height="315" src="https://www.youtube.com/embed/IcQIAdZ55fA?rel=0&autoplay=1&loop=1" frameborder="0" allowfullscreen></iframe>');
    $("#AcceptancePageDivision").append(pls);   
}