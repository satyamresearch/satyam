function instructionsSatyam(job, h) {
    h.append("<h3>Instructions and Tips</h3>");
    // h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/IcQIAdZ55fA target='_blank' > here </a></b>. (opens in new tab)");
    //h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/k6eqBXkudF4 target='_blank' > here </a></b>. (opens in new tab)");
    //h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/CFMXdLS1_0I target='_blank' > here </a></b>. (opens in new tab)");
    //h.append('<iframe width="700" height="400" src="https://www.youtube.com/embed/CFMXdLS1_0I?start=9" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen></iframe>');
    h.append("<h3>Task</h3>");
    h.append("<br>In this task, you will be given a short video clip in which an object will be performing different actions. " +
        "You will <b> label the attributes (e.g.walking, standing, talking) of the object tracked and idenified by a bounding box</b >. "+
        "<font color='red'>Select all</font> of the various attributes <font color='red'>that apply</font> to that particular object in the menu list <font color='red'>on the top right corner</font> of the screen. " + 
        "Drag the slider bar to play the video back and forth, and select as the attributes become true and deselect them when they become false. ");
    //h.append("</div>");
    h.append("<h3>Remember</h3>");
    h.append("<ul style='list-style-type:circle'>" +
        "<li>Select <font color='red'>ALL</font> actions <font color='red'>ONLY WHEN</font> the object in the bounding box perform those actions.</li> <li>Make sure to <font color='red'>DESELECT</font> as the actions disappear and <font color='red'>SELECT</font> new actions when they appear as the video plays forward. </li>"+
        "<li> If you cannot temporarily see an entity properly (more than 80%), due to shadows or being obstructed by something in the scene, check on <strong> Temporarily Occluded </strong>and uncheck as soon as the object becomes visible again </li>" +
        "<li> At the frame where you can see the entity for the last time, beacuse it is out of the view, or obstructed, or exits the frame, please check <strong> Out of View </strong></li>" +
        //"<li> Rewind and play back to make sure that the box tracks the object correctly and make corrections wherever neccessary. </li>" +
        //"<li><b>When you are ready to submit your work</b>, rewind the video and watch it through one more time. Does each rectangle follow the object it is tracking for the entire sequence? If you find a spot where it misses, press <strong>Pause</strong> and adjust the box. After you have checked your work, press the <strong>Submit </strong> button. We will pay you as soon as possible.</li>" +
        "</ul>");

    //h.append("<h3>Boxes should be tight</h3>");
    //h.append("<table><tr><td><img src='Images/tight-good.jpg'></td><td><img src='Images/tight-bad.jpg'></td></tr><tr><th>Good</th><th>Bad</th></tr></table>");

    h.append("<h3>Tools and Tips</h3>");
    h.append("<ul>" +
        "<li> The <strong>Slow</strong>, <strong>Normal</strong>, <strong>Fast</strong> buttons will change how fast the video plays back. If the video becomes confusing, slowing the play back speed may help.</li>" +
        "<li>Clicking <strong>Hide Boxes</strong> will temporarily hide the boxes on the screen. This is useful when the scene becomes too crowded. Remember to click it again to show the boxes.</li>" +
        //"<li>Remembering which box corresponds to which entity can be confusing. If you click on a box in the view screen, a tooltip will pop that will remind you of the box's identity by displaying its past track.</li>" +
        //"<li>Clicking <strong>Disable Resize</strong> will toggle between allowing you to resize the boxes. This option is helpful when the boxes become especially small.</li>" +
        "</ul>");

    h.append("<h3>Acknowledgements</h3>");
    h.append("We thanks the developers of <a href = http://web.mit.edu/vondrick/vatic/ > VATIC</a> as this tool was developed by extending and modifying its source code.");
}

function instructionsVisionZero(job, h) {
    h.append("<h3>Instructions and Tips</h3>");
   // h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/IcQIAdZ55fA target='_blank' > here </a></b>. (opens in new tab)");
    h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/k6eqBXkudF4 target='_blank' > here </a></b>. (opens in new tab)");

    h.append("<br><b>Task :</b> In this task, we ask you to <b>track various traffic entities at a traffic intersection</b>. You have to draw a tightly fitting box around each of these objects and move the boxes with the objects as they move in the video. Here are some reference examples of traffic objects");
    //h.append("<div style='vertical-align: middle' align='center'>");
    h.append("<table border=0>");
    h.append("<tr>");
    h.append("<td style='vertical-align: middle'><b>Car : </b></td>");
    h.append("<td><img src='InstructionImages/car1.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car3.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car_example_4.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car_example_3.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("<tr>");
    h.append("<td style='vertical-align: middle'><b>Bus : </b></td>");
    h.append("<td><img src='InstructionImages/bus_example_2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus1.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus3.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus5.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("<tr>");
    h.append("<td style='vertical-align: middle'><b>Truck : </b></td>");
    h.append("<td><img src='InstructionImages/truck1.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/truck2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/truck3.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/truck4.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("<tr>");
    h.append("<td style='vertical-align: middle'><b>Pedestrian : </b></td>");
    h.append("<td><img src='InstructionImages/ped1.PNG' height='80' width='120'></td>");
    h.append("<td style='vertical-align: middle'><b>Bicycle : </b></td>");
    h.append("<td><img src='InstructionImages/bicycle1.PNG' height='80' width='80'></td>");
    h.append("<td style='vertical-align: middle'><b>Motor Bike : </b></td>");
    h.append("<td><img src='InstructionImages/motorbike1.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("</table>");
    //h.append("</div>");
    h.append("<h3>Remember</h3>");
    h.append("<ul style='list-style-type:circle'>" +
    "<li> As soon as any part of the entity is on the road, <strong> check on Crossing Road </strong></li>" +
    "<li> As soon as the entity is off the road, <strong> check off Crossing Road </strong></li>" +
    "<li> If you cannot temporarily see an entity properly (more than 80%), due to shadows or being obstructed by something in the scene, check on <strong> Temporarily Occluded </strong>and uncheck as soon as the object becomes visible again </li>" +
    "<li> At the frame where you can see the entity for the last time, beacuse it is out of the view, or obstructed, or exits the frame, please check <strong> Out of View </strong></li>" +
    "<li> Rewind and play back to make sure that the box tracks the object correctly and make corrections wherever neccessary. </li>" +
    "<li><b>When you are ready to submit your work</b>, rewind the video and watch it through one more time. Does each rectangle follow the object it is tracking for the entire sequence? If you find a spot where it misses, press <strong>Pause</strong> and adjust the box. After you have checked your work, press the <strong>Submit </strong> button. We will pay you as soon as possible.</li>" +
    "</ul>");

    h.append("<h3>Boxes should be tight</h3>");
    h.append("<table><tr><td><img src='JQueryScripts/Images/tight-good.jpg'></td><td><img src='JQueryScripts/Images/tight-bad.jpg'></td></tr><tr><th>Good</th><th>Bad</th></tr></table>");

    h.append("<h3>Tools and Tips</h3>");
    h.append("<ul>" +
       "<li> The <strong>Slow</strong>, <strong>Normal</strong>, <strong>Fast</strong> buttons will change how fast the video plays back. If the video becomes confusing, slowing the play back speed may help.</li>" +
       "<li>Clicking <strong>Hide Boxes</strong> will temporarily hide the boxes on the screen. This is useful when the scene becomes too crowded. Remember to click it again to show the boxes.</li>" +
        "<li>Remembering which box corresponds to which entity can be confusing. If you click on a box in the view screen, a tooltip will pop that will remind you of the box's identity by displaying its past track.</li>" +
        "<li>Clicking <strong>Disable Resize</strong> will toggle between allowing you to resize the boxes. This option is helpful when the boxes become especially small.</li>" +
    "</ul>");

    h.append("<h3>Acknowledgements</h3>");
    h.append("We thanks the developers of <a href = http://web.mit.edu/vondrick/vatic/ > VATIC</a> as this tool was developed by extending and modifying its source code.");
}





function instructions(job, h)
{
    h.append("<h3>Instructions and Tips</h3>");
    h.append("<note style='color: red;'> <b>Note :</b> Your work will be manually evaluated and will effect your rating.</note>");
    h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/IcQIAdZ55fA target='_blank' > here </a></b>. (opens in new tab)");

    h.append("<br><b>Task :</b> In this task, we ask you to <b>track various traffic entities at a traffic intersection</b>. You have to draw a tightly fitting box around each of these objects and move the boxes with the objects as they move in the video. Here are some reference examples of traffic objects");
    h.append("<table>");
    h.append("<tr>");
    h.append("<td><b>Car : </b></td>");
    h.append("<td><img src='InstructionImages/car1.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car3.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car_example_4.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/car_example_3.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("<tr>");
    h.append("<td><b>Bus : </b></td>");
    h.append("<td><img src='InstructionImages/bus_example_2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus1.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus3.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/bus5.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("<tr>");
    h.append("<td><b>Truck : </b></td>");
    h.append("<td><img src='InstructionImages/truck1.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/truck2.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/truck3.PNG' height='80' width='80'></td>");
    h.append("<td><img src='InstructionImages/truck4.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("<tr>");
    h.append("<td><b>Pedestrian : </b></td>");
    h.append("<td><img src='InstructionImages/ped1.PNG' height='80' width='120'></td>");
    h.append("<td><b>Bicycle : </b></td>");
    h.append("<td><img src='InstructionImages/bicycle1.PNG' height='80' width='80'></td>");
    h.append("<td><b>Motor Bike : </b></td>");
    h.append("<td><img src='InstructionImages/motorbike1.PNG' height='80' width='80'></td>");
    h.append("</tr>");
    h.append("</table>");

    h.append("<h3>Remember</h3>");
    h.append("<ul style='list-style-type:circle'>" + 
    "<li> As soon as any part of the entity is on the road, <strong> check on Crossing Road </strong></li>" + 
    "<li> As soon as the entity is off the road, <strong> check off Crossing Road </strong></li>" + 
    "<li> If you cannot temporarily see an entity properly (more than 80%), due to shadows or being obstructed by something in the scene, check on <strong> Temporarily Occluded </strong>and uncheck as soon as the object becomes visible again </li>" + 
    "<li> At the frame where you can see the entity for the last time, beacuse it is out of the view, or obstructed, or exits the frame, please check <strong> Out of View </strong></li>" + 
    "<li> Rewind and play back to make sure that the box tracks the object correctly and make corrections wherever neccessary. </li>" + 
    "<li> You will be paid 15 cents for each moving object and 2 cents for each stationary object that you correctly track. <note style='color: red;'>You will be paid only if you do at least 3 moving objects.</note> For example if you track 5 moving objects and 2 stationary objects correctly you earn 79 cents ie. 45 cents base price + 34 cents bonus.</li> " +
    "<li><b>When you are ready to submit your work</b>, rewind the video and watch it through one more time. Does each rectangle follow the object it is tracking for the entire sequence? If you find a spot where it misses, press <strong>Pause</strong> and adjust the box. After you have checked your work, press the <strong>Submit </strong> button. We will pay you as soon as possible.</li>" + 
    "</ul>");
/*    h.append("<li> As soon as any part of the entity is on the road, <strong> check on Crossing Road </strong></li>");
    h.append("<li> As soon as the entity is off the road, <strong> check off Crossing Road </strong></li>");
    h.append("<li> If you cannot temporarily see an entity properly (more than 80%), due to shadows or being obstructed by something in the scene, check on <strong> Temporarily Occluded </strong>and uncheck as soon as the object becomes visible again </li>");
    h.append("<li> At the frame where you can see the entity for the last time, beacuse it is out of the view, or obstructed, or exits the frame, please check <strong> Out of View </strong></li>");
    h.append("<li> Rewind and play back to make sure that the box tracks the object correctly and make corrections wherever neccessary. </li>");
    h.append("<li>We will pay you <strong> 6 cents for each object</strong> you correctly annotate.</li>");
    h.append("<li> We will pay you <strong> 10 cents bonus if you annotate all the entities </strong> in the video.</li>");    
    h.append("<li><b>When you are ready to submit your work</b>, rewind the video and watch it through one more time. Does each rectangle follow the object it is tracking for the entire sequence? If you find a spot where it misses, press <strong>Pause</strong> and adjust the box. After you have checked your work, press the <strong>Submit </strong> button. We will pay you as soon as possible.</li>");*/
/*    if (job.perobject > 0)
    {
        var amount = Math.floor(job.perobject * 100);
        str += "<li>We will pay you <strong>" + amount + "&cent; for each object</strong> you annotate.</li>";
    }
    if (job.completion > 0)
    {
        var amount = Math.floor(job.completion * 100);
        str += "<li>We will award you a <strong>bonus of " + amount + "&cent; if you annotate every object</strong>.</li>";
    }
    if (job.skip > 0)
    {
        str += "<li>When the video pauses, adjust your annotations.</li>";
    }*/
    //str += "<li>We will hand review your work.</li>";
    /*h.append("</ul>");*/

    h.append("<h3>Boxes should be tight</h3>");
    h.append("<table><tr><td><img src='JQueryScripts/Images/tight-good.jpg'></td><td><img src='JQueryScripts/Images/tight-bad.jpg'></td></tr><tr><th>Good</th><th>Bad</th></tr></table>");

    h.append("<h3>Tools and Tips</h3>");
    h.append("<ul>" +
       "<li> The <strong>Slow</strong>, <strong>Normal</strong>, <strong>Fast</strong> buttons will change how fast the video plays back. If the video becomes confusing, slowing the play back speed may help.</li>" +
       "<li>Clicking <strong>Hide Boxes</strong> will temporarily hide the boxes on the screen. This is useful when the scene becomes too crowded. Remember to click it again to show the boxes.</li>" +
       // "<li>If there are many objects on the screen, it can become difficult to select the correct bounding box. By pressing the lock button <img src='JQueryScripts/Images/lock.jpg'> on an object's property bar, you can prevent changes to that track. Press the lock button again to renable modifications.</li>" + 
        "<li>Remembering which box corresponds to which entity can be confusing. If you click on a box in the view screen, a tooltip will pop that will remind you of the box's identity by displaying its past track.</li>" +     
        "<li>Clicking <strong>Disable Resize</strong> will toggle between allowing you to resize the boxes. This option is helpful when the boxes become especially small.</li>" +
    "</ul>");

  /*  h.append("<h3>Keyboard Shortcuts</h3>");
    h.append("<p>These keyboard shortcuts are available for your convenience:</p>");
    h.append('<ul class="keyboardshortcuts">' +
        '<li><code>n</code> creates a new object</li>' +
        '<li><code>t</code> toggles play/pause on the video</li>' +
        '<li><code>r</code> rewinds the video to the start</li>' +
        '<li><code>h</code> hides/shows the boxes (only after clicking Options button)</li>' +
        '<li><code>d</code> jump the video forward a bit</li>' +
        '<li><code>f</code> jump the video backward a bit</li>' +
        '<li><code>v</code> step the video forward a tiny bit</li>' +
        '<li><code>c</code> step the video backward a tiny bit</li>' +
        '</ul>');*/
}
