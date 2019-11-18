function instructionsSatyam(job, h) {
    h.append("<h3>Instructions and Tips</h3>");
    // h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/IcQIAdZ55fA target='_blank' > here </a></b>. (opens in new tab)");
    //h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/k6eqBXkudF4 target='_blank' > here </a></b>. (opens in new tab)");
    h.append("<br><b>You can watch the instruction video <a href=  https://youtu.be/CFMXdLS1_0I target='_blank' > here </a></b>. (opens in new tab)");
    h.append('<iframe width="700" height="400" src="image/turker.mp4" frameborder="0" allow="autoplay; encrypted-media" allowfullscreen></iframe>');

    h.append("<br><b>Task :</b> Your job</strong> is to draw four set of lines follwing the proper instructions </b>.");
    //h.append("</div>");
    // h.append("<h3>Remember</h3>");
    // h.append("<ul style='list-style-type:circle'>" +
    //     "<li><strong style="color:red"> First, draw the three axis of the car within the blue box. These lines should aligns with three dimensions of the car (length, width, height).</strong></li>" +
    //     "<li><strong style="color:red"> Second, draw 3-4 lines that you think are parallel to the length axis you've drawn.</strong></li>" +
    //     "<li><strong style="color:red"> Third, draw 3-4 lines that you think are parallel to the width axis you've drawn.</strong></li>" +
    //     "<li><strong style="color:red"> Fourth, draw 3-4 lines that you think are parallel to the height axis you've drawn.</strong></li>" +
    //     "</ul>");
    h.append("<h3>Steps to Complete</h3>");
    h.append("<ul style='list-style-type:circle'>" +
        "<li> First, draw the three axis of the car within the blue box. These lines should aligns with three dimensions of the car (length, width, height). </li>" +
        "<li> Second, draw 3-4 lines that you think are parallel to the length axis you've drawn.</li>" +
        "<li> Third, draw 3-4 lines that you think are parallel to the width axis you've drawn. </li>" +
        "<li>Fourth, draw 3-4 lines that you think are parallel to the height axis you've drawn.</li>" +
        "</ul>");

    h.append("<h3>Car Axis Drawing Instruction</h3>");
    h.append("<ul style='list-style-type:circle'>" +
        "<li> Your task is to draw the lines that lies along the length, width, and height of the car and has one of the car bottom corner as the intersection Point. In order to do that, please carefully follow the instructions below.</li>" + 
        "<li> <strong> The order below is of utmost importance. Do not deviate from the order.</strong> </li>" + 
        
        "<li> <strong>To start </strong>, first click the &quot;Draw the Car Axis&quot Button </li>" +
        "<li> <strong>Click on</strong> the bottom Corner on the Car which is closest from the Bottom of the Image. We will call this point Origin. </li>" + 
        "<li><strong>Click on</strong> the other bottom corner of the Car along the length of the Car. </li>" +
        "<li><strong>Click on</strong> the other bottom corner of the Car along the Width of the Car. </li>" +
        "<li><strong>Click on</strong> a point that you think is along the car height if you join this point and the origin. </li>" +
        "<li>If you think you made a mistake, you can undo the last step by clicking on &quot;Undo Last Point&quot</li>" +
       "</ul>");

    // h.append("<table><tr><td><img width="400" src='image/step1.jpg'></td><td><img width="400" src='image/step2.jpg'></td></tr><tr><th>Step1</th><th>Step2</th></tr></table>");
    // h.append("<table><tr><td><img width="400" src='image/step3.jpg'></td><td><img width="400" src='image/step4.jpg'></td></tr><tr><th>Step3</th><th>Step4</th></tr></table>");

    h.append("<h3>Lines Parallel to the Car Length Drawing Instruction</h3>");
    h.append("<ul style='list-style-type:circle'>" +
        "<li> Your task is to draw 3-4 lines that you think are parallel to the line along the length of the car.</li>" +
        "<li> These lines are typically, the road edges, lane dividers, pedestrian path parallel to the road.</li>" +
        "<li> <strong>To start </strong>, first click the &quot;Draw Parallels to Length&quot Button </li>" +
        "<li><strong>To draw each line, Click on</strong> images pixels with the two endpoints of the line</li>" +
        "<li><strong>To delete</strong> the last point, click on &quot;Undo Last Point&quot;</li>" +
        "<li><strong>To delete</strong> the last line, click on &quot;Undo Last Point&quot; twice</li>" +
        "<li><strong>Any length of the line segments are accepted</strong> </li>" +
        "<li>When finished</strong>, click on &quot;Finish Task&quot;</li>" +
        "<li><strong>Example Good Work</strong>:</li>" +
        "</ul>");
    // h.append("<table><tr><td><img src='image/stepx.jpg'></td></tr><tr><th>Good</th></tr></table>");

    h.append("<h3>Lines Parallel to the Car Width Drawing Instruction</h3>");
    h.append("<ul style='list-style-type:circle'>" +
        "<li> Your task is to draw 3-4 lines that you think are parallel to the line along the width of the car.</li>" +
        "<li> These lines are typically, the pedestrian crossing, edges of the building that are perpendicular to the road, cross roads etc</li>" +
        "<li> <strong>To start </strong>, first click the &quot;Draw Parallels to Width&quot Button </li>" +
        "<li><strong>To draw each line, Click on</strong> images pixels with the two endpoints of the line</li>" +
        "<li><strong>To delete</strong> the last point, click on &quot;Undo Last Point&quot;</li>" +
        "<li><strong>To delete</strong> the last line, click on &quot;Undo Last Point&quot; twice</li>" +
        "<li><strong>Any length of the line segments are accepted</strong> </li>" +
        "<li>When finished</strong>, click on &quot;Finish Task&quot;</li>" +
        "<li><strong>Example Good Work</strong>:</li>" +
        "</ul>");

    h.append("<h3>Lines Parallel to the Car Height Drawing Instruction</h3>");
    h.append("<ul style='list-style-type:circle'>" +
        "<li> Your task is to draw 3-4 lines that you think are parallel to the line along the height of the car.</li>" +
        "<li> These lines are typically edges of the street lamps, parking meters, verticle edges of buildings</li>" +
        "<li> <strong>To start </strong>, first click the &quot;Draw Parallels to Height&quot Button </li>" +
        "<li><strong>To draw each line, Click on</strong> images pixels with the two endpoints of the line</li>" +
        "<li><strong>To delete</strong> the last point, click on &quot;Undo Last Point&quot;</li>" +
        "<li><strong>To delete</strong> the last line, click on &quot;Undo Last Point&quot; twice</li>" +
        "<li><strong>Any length of the line segments are accepted</strong> </li>" +
        "<li>When finished</strong>, click on &quot;Finish Task&quot;</li>" +
        "<li><strong>Example Good Work</strong>:</li>" +
        "</ul>");

    h.append("<h3>Tools and Tips</h3>");
    h.append("<ul>" +
        "<li> <strong>You MUST Finish the Car Axis Drawing at the very beginning before you CAN proceed to the other three tasks</strong></li>" +
        "<li><strong>You can redo any of the steps by clicking the respective buttons</strong></li>" +
        "<li>When finished</strong>, click on &quot;Submit&quot; to submit the result.</li>" +
        "<li><strong>Example Good Work</strong>:</li>" +
        "</ul>");
    h.append("<table><tr><td><img src='image/goodwork.jpg'></td><td><img src='Images/tight-bad.jpg'></td></tr><tr><th>Good</th><th>Bad</th></tr></table>");

    // h.append("<h3>Acknowledgements</h3>");
    // h.append("We thanks the developers of <a href = http://web.mit.edu/vondrick/vatic/ > VATIC</a> as this tool was developed by extending and modifying its source code.");
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
