$(document).ready(function () {
    prepare();
});

var ImageContainer = null;
var Image = null;
//var existingObjects = new Array(); // each object might have multiple polygons with boolean sign
var currentObjectCardinalPoints = new Array();
var car_axis_points = new Array();
var x_axis_points = new Array();
var y_axis_points = new Array();
var z_axis_points = new Array();


var existingObjectIDs = new Array();
var existingParts = new Array();
var existingCategoryIndexes = new Array();
var polygonDivisions = new Array();
var selectedPolygons = new Array();
var svgDivisions = new Array();

var PolygonsInDrawnSequence = null;

var PNG = new Array();

var button_type = 0
var car_anno_draw_type = 0
var drawBoundary = "false";
var canvas = null;

var boundaryDrawer = null;
var axisDrawer = null;


var categoryIndex = 0;
var category;
var colors = [["256,0,0"], ["0,256,0"], ["0,0,256"], ["256,0,256"], ["256,256,0"], ["0,256,256"], ["0,0,0"], ["256,256,256"],
["128,0,0"], ["0,128,0"], ["0,0,128"], ["128,0,128"], ["128,128,0"], ["0,128,128"], ["128,128,128"],
["192,0,0"], ["0,192,0"], ["0,0,192"], ["192,0,192"], ["192,192,0"], ["0,192,192"], ["192,192,192"]];
var currentColor = ["256,0,0"];
//var colors = ["red", "blue", "green", "purple", "yellow", "violet"];

var ObjCounter = 0;
var PolygonCounter = 0;


var displayWidth;
var displayHeight;

var imageWidth;
var imageHeight;

var scalex;
var scaley;


var DeleteRegionFlag = false;

function prepare() {
    boundaries = new Array();
    ImageContainer = $("#ImageDivision")[0];
    Image = $("#TheImage");
    hide_axis_draw()

    updateDisplayParams();

    document.getElementById('canvas').height = displayHeight;

    //$("#CategorySelection_RadioButtonList")[0].disabled = true;
    //$("#CategorySelection_RadioButtonList_0")[0].checked = true;

    $("#attentionDiv").hide();
    $("#QuestionDiv").hide();
    // button hide show
    $("#addDropTable").hide();
    $("#editTable").hide();
    $("#buttonTable").hide();
    $("#caraxiseditTable").hide();

    //Instruciton hide show
    showStartingInstruction();

    var checked_radio = $("[id*=CategorySelection_RadioButtonList] input:checked");
    category = checked_radio.val();
    //$("#CategoryMentionLabel")[0].innerHTML = category;

    for (var i = 0; i < displayWidth * displayHeight; i++) {
        PNG.push(0);
    }
    ui_showinstructions()
}



function updateDisplayParams() {
    displayWidth = Image[0].clientWidth;
    displayHeight = Image[0].clientHeight;
    imageWidth = Image[0].naturalWidth;
    imageHeight = Image[0].naturalHeight;
    scalex = imageWidth / displayWidth;
    scaley = imageHeight / displayHeight;
}


function showStartingInstruction() {
    $("#StartingInstruction").show();
    $("#CarAxisInstruction").hide();
    $("#CarAxisInstruction0").hide();
    $("#CarAxisInstruction1").hide();
    $("#CarAxisInstruction2").hide();
    $("#CarAxisInstruction3").hide();

    $("#XAxisInstruction").hide();
    $("#YAxisInstruction").hide();
    $("#ZAxisInstruction").hide();
    $("#DrawingInstruction").hide();
    $("#caraxiseditTable").hide();

}
function showCarInstruction() {
    $("#StartingInstruction").hide();
    $("#CarAxisInstruction").show();
    $("#CarAxisInstruction0").hide();
    $("#CarAxisInstruction1").hide();
    $("#CarAxisInstruction2").hide();
    $("#CarAxisInstruction3").hide();

    $("#XAxisInstruction").hide();
    $("#YAxisInstruction").hide();
    $("#ZAxisInstruction").hide();
    $("#DrawingInstruction").hide();
    $("#caraxiseditTable").show();
    $("#editTable").hide();
}

function showCarInstruction_2() {
    var button = $("#next_step")[0];
    $("#CarAxisInstruction").hide();
    $("#CarAxisInstruction0").hide();
    $("#CarAxisInstruction1").hide();
    $("#CarAxisInstruction2").hide();
    $("#CarAxisInstruction3").hide();
    if (car_anno_draw_type == 0) {
        button.value = "Draw Car Origin";
        $("#CarAxisInstruction").show();

    }
    else if (car_anno_draw_type == 1) {
        button.value = "Origin Done; Draw Car Length";
        $("#CarAxisInstruction0").show();

    }
    else if (car_anno_draw_type == 2) {
        button.value = "Length Done; Draw Car Width";
        $("#CarAxisInstruction1").show();

    }
    else if (car_anno_draw_type == 3) {
        button.value = "Width Done; Draw Car Height";
        $("#CarAxisInstruction2").show();

    }
    else if (car_anno_draw_type == 4) {
        $("#CarAxisInstruction3").show();
    }
    $("#StartingInstruction").hide();
    $("#XAxisInstruction").hide();
    $("#YAxisInstruction").hide();
    $("#ZAxisInstruction").hide();
    $("#DrawingInstruction").hide();
    if (car_anno_draw_type <= 3) {
        $("#caraxiseditTable").show();
        $("#editTable").hide();
    }
    else {
        $("#caraxiseditTable").hide();
        $("#editTable").show();
    }


}


function showXparallelInstruction() {
    $("#StartingInstruction").hide();
    $("#CarAxisInstruction").hide();
    $("#CarAxisInstruction0").hide();
    $("#CarAxisInstruction1").hide();
    $("#CarAxisInstruction2").hide();
    $("#CarAxisInstruction3").hide();

    $("#XAxisInstruction").show();
    $("#YAxisInstruction").hide();
    $("#ZAxisInstruction").hide();
    $("#DrawingInstruction").hide();
    $("#caraxiseditTable").hide();

}
function showYparallelInstruction() {
    $("#StartingInstruction").hide();
    $("#CarAxisInstruction").hide();
    $("#CarAxisInstruction0").hide();
    $("#CarAxisInstruction1").hide();
    $("#CarAxisInstruction2").hide();
    $("#CarAxisInstruction3").hide();
    $("#XAxisInstruction").hide();
    $("#YAxisInstruction").show();
    $("#ZAxisInstruction").hide();
    $("#DrawingInstruction").hide();
    $("#caraxiseditTable").hide();

}
function showZparallelInstruction() {
    $("#StartingInstruction").hide();
    $("#CarAxisInstruction").hide();
    $("#CarAxisInstruction0").hide();
    $("#CarAxisInstruction1").hide();
    $("#CarAxisInstruction2").hide();
    $("#CarAxisInstruction3").hide();
    $("#XAxisInstruction").hide();
    $("#YAxisInstruction").hide();
    $("#ZAxisInstruction").show();
    $("#DrawingInstruction").hide();
    $("#caraxiseditTable").hide();

}



function showDrawingInstruction() {
    $("#StartingInstruction").hide();
    $("#CarAxisInstruction").hide();
    $("#CarAxisInstruction0").hide();
    $("#CarAxisInstruction1").hide();
    $("#CarAxisInstruction2").hide();
    $("#CarAxisInstruction3").hide();
    $("#XAxisInstruction").hide();
    $("#YAxisInstruction").hide();
    $("#ZAxisInstruction").hide();
    $("#DrawingInstruction").show();
}

function show_axis_draw() {
    $("#NewXAxisButton").show();
    $("#NewYAxisButton").show();
    $("#NewZAxisButton").show();
}
function hide_axis_draw() {
    $("#NewXAxisButton").hide();
    $("#NewYAxisButton").hide();
    $("#NewZAxisButton").hide();
}


function NewObjectOnClick() {
    $("#QuestionDiv").hide();
    var button = $("#NewObjectButton")[0];
    button_type = 1
    if (button.value == "Draw the Car Axis") {
        $("#attentionDiv").show();
        $("#CategorySelectionPanel").hide();
        $("#addDropTable").show();
        $("#buttonTable").hide();
        $("#NewObjectButtonTable").hide();

        button.value = "Re-Draw the Car Axis";

        showCarInstruction_2();

        draw_the_axis();

    } else {
        $("#attentionDiv").hide();
        $("#CategorySelectionPanel").show();
        $("#addDropTable").hide();
        $("#buttonTable").show();

        PolygonCounter = 0;
        ObjCounter++;

        showCarInstruction_2();
        draw_the_axis();

    }
    return false;

}

function NewParallelOnClickx() {
    $("#QuestionDiv").hide();
    button_type = 2
    var button = $("#NewXAxisButton")[0];
    if (button.value == "Draw Parallels to Length") {
        $("#attentionDiv").show();
        $("#CategorySelectionPanel").hide();
        $("#addDropTable").show();
        $("#buttonTable").hide();
        $("#NewObjectButtonTable").hide();

        button.value = "Re-Draw Parallels to Length";


        showXparallelInstruction();

        PolygonsInDrawnSequence = new Array();
        draw_parallel_lines();

    } else {
        $("#attentionDiv").hide();
        $("#CategorySelectionPanel").show();
        $("#addDropTable").hide();
        $("#buttonTable").show();

        PolygonCounter = 0;
        ObjCounter++;

        showXparallelInstruction();
        draw_parallel_lines();

    }
    return false;

}

function NewParallelOnClicky() {
    $("#QuestionDiv").hide();
    button_type = 3
    var button = $("#NewYAxisButton")[0];
    if (button.value == "Draw Parallels to Width") {
        $("#attentionDiv").show();
        $("#CategorySelectionPanel").hide();
        $("#addDropTable").show();
        $("#buttonTable").hide();
        $("#NewObjectButtonTable").hide();

        button.value = "Re-Draw Parallels to Width";
        //existingObjects.push(new Array());

        showYparallelInstruction();

        PolygonsInDrawnSequence = new Array();
        draw_parallel_lines();

    } else {
        $("#attentionDiv").hide();
        $("#CategorySelectionPanel").show();
        $("#addDropTable").hide();
        $("#buttonTable").show();

        PolygonCounter = 0;
        ObjCounter++;

        showYparallelInstruction();
        draw_parallel_lines();

    }
    return false;

}
function NewParallelOnClickz() {
    $("#QuestionDiv").hide();
    button_type = 4
    var button = $("#NewZAxisButton")[0];
    if (button.value == "Draw Parallels to Height") {
        $("#attentionDiv").show();
        $("#CategorySelectionPanel").hide();
        $("#addDropTable").show();
        $("#buttonTable").hide();
        $("#NewObjectButtonTable").hide();

        button.value = "Re-Draw Parallels to Height";
        //existingObjects.push(new Array());


        showZparallelInstruction();

        PolygonsInDrawnSequence = new Array();
        draw_parallel_lines();

    } else {
        $("#attentionDiv").hide();
        $("#CategorySelectionPanel").show();
        $("#addDropTable").hide();
        $("#buttonTable").show();

        PolygonCounter = 0;
        ObjCounter++;

        showZparallelInstruction();
        draw_parallel_lines();

    }
    return false;

}

function draw_the_axis() {
    DeleteRegionFlag = false;
    $("#NewObjectButtonTable").hide();
    $("#addDropTable").hide();
    $("#QuestionDiv").hide();
    $("#editTable").show();


    for (var i = 0; i < polygonDivisions.length - 1; i++) {
        polygonDivisions[i].unbind('click');
        selectedPolygons[i] = false;
    }
    drawBoundary = true;

    boundaryDrawer = new BoundaryDrawer(colors[0], car_axis_points, x_axis_points, y_axis_points, z_axis_points, displayWidth, displayHeight, PNG);
    boundaryDrawer.init();

    showCarInstruction_2();
    return false;
}
function draw_parallel_lines() {
    DeleteRegionFlag = false;
    $("#NewObjectButtonTable").hide();
    $("#addDropTable").hide();
    $("#QuestionDiv").hide();
    $("#editTable").show();


    for (var i = 0; i < polygonDivisions.length - 1; i++) {
        polygonDivisions[i].unbind('click');
        selectedPolygons[i] = false;
    }
    // drawBoundary = true;

    axisDrawer = new AxisDrawer(colors[button_type], button_type, car_axis_points, x_axis_points, y_axis_points, z_axis_points, displayWidth, displayHeight, PNG);
    axisDrawer.init();

    return false;
}


function globalUpdate() {
    PolygonCounter = boundaryDrawer.getNoPolygonOfCurrentObject();

    PolygonCounter++;

    var lastIndex = existingObjectIDs.length - 1;
    var cardinalPoints = boundaryDrawer.getCardinalPoints();


    if (cardinalPoints != null) {

        currentObjectCardinalPoints = cardinalPoints;

        existingObjectIDs[lastIndex] = boundaryDrawer.getObjID();
        existingParts = boundaryDrawer.getExistingParts();
        existingCategoryIndexes[lastIndex] = categoryIndex;
        polygonDivisions[lastIndex] = boundaryDrawer.getPolygonDivision();
        selectedPolygons[lastIndex] = false;
        svgDivisions[lastIndex] = boundaryDrawer.getSVGDivision();

    }
}


function FinishPolygonOnClick() {
    $("#attentionDiv").hide();
    $("#CategorySelectionPanel").show();
    $("#addDropTable").hide();
    $("#QuestionDiv").show();

    $("#buttonTable").show();
    $("#editTable").hide();
    $("#NewObjectButtonTable").show();

    if (button_type == 1) {
        var fin = boundaryDrawer.finish();
        car_axis_points = boundaryDrawer.getCardinalPoints();
        car_anno_draw_type = 0
        delete boundaryDrawer;
        drawBoundary = false;
        if (car_axis_points.length == 4) {
            show_axis_draw()
        }
    }
    else if (button_type == 2) {
        var fin = axisDrawer.finish();

        x_axis_points = axisDrawer.getCardinalPoints();
        // console.log(x_axis_points);

        delete axisDrawer;
    }
    else if (button_type == 3) {
        var fin = axisDrawer.finish();

        y_axis_points = axisDrawer.getCardinalPoints();
        delete axisDrawer;
    }
    else if (button_type == 4) {
        var fin = axisDrawer.finish();

        z_axis_points = axisDrawer.getCardinalPoints();
        delete axisDrawer;
    }
    button_type = 0
    // globalUpdate();

    //if (!fin) {
    //    PolygonsInDrawnSequence.push(boundaryDrawer.getCurrentPolygonCardinalPoints());
    //}


    // $("#canvas")[0].style.zIndex = 0;
    // for (var i = 0; i < polygonDivisions.length; i++) {
    //     polygonDivisions[i][0].style.zIndex = 10;
    //     polygonDivisions[i].bind('click', polygonSelected);
    // }




    // if (PolygonCounter == 0) {
    //     showFirstPolygonInstruction();
    // } else {
    //     showXparallelInstruction();
    // }


    showDrawingInstruction()
    return false;
}


function DeleteObjectClick() {
    var deleted = false;
    var objIdsToBeRemoved = new Array();
    var ObjIndexesToBeRemoved = new Array();
    for (var i = 0; i < polygonDivisions.length; i++) {
        if (selectedPolygons[i] == true) //remove this division
        {
            svgDivisions[i].remove();
            objIdsToBeRemoved.push(existingObjectIDs[i]);
            ObjIndexesToBeRemoved.push(i);
            deleted = true;
        }
    }
    if (deleted) {
        var partsIndexToBeRemoved = new Array();
        for (var j = 0; j < existingParts.length; j++) {
            for (var i = 0; i < objIdsToBeRemoved.length; i++) {
                if (existingParts[j].getObjID() == objIdsToBeRemoved[i]) {
                    partsIndexToBeRemoved.push(j);
                    break;
                }
            }
        }


        for (var i = 0; i < partsIndexToBeRemoved.length; i++) {
            existingParts.splice(i, 1);
        }

        for (var i = 0; i < ObjIndexesToBeRemoved.length; i++) {
            var index = i;
            //existingObjects.splice(index, 1);
            existingObjectIDs.splice(index, 1);
            existingCategoryIndexes.splice(index, 1);
            svgDivisions.splice(index, 1);
            polygonDivisions.splice(index, 1);
            selectedPolygons.splice(index, 1);
        }

        boundaryDrawer = new BoundaryDrawer(currentColor, category, new Array(),
            existingObjectIDs, existingParts, existingCategoryIndexes, ObjCounter, PolygonCounter, displayWidth, displayHeight, PNG);

        boundaryDrawer.init();
        delete boundaryDrawer;
        $("#canvas")[0].style.zIndex = 0;
        $("#canvas").unbind('click');
    }
    return false;
}
function FinishCarAxisAnno() {


    var button = $("#next_step")[0];

    car_axis_points = boundaryDrawer.getCardinalPoints();

    if (car_anno_draw_type == 0) {
        if (car_axis_points.length == 0) {
            car_anno_draw_type++;
        }
        else
            alert("You selected too many point. Undo all points")
    }
    else if (car_anno_draw_type == 1) {
        if (car_axis_points.length == 1) {
            car_anno_draw_type++;
        }
        else
            alert("You MUST have only ONE points selected")
    }
    else if (car_anno_draw_type == 2) {
        if (car_axis_points.length == 2) {
            car_anno_draw_type++;
        }
        else
            alert("You MUST have only TWO points selected")
    }
    else if (car_anno_draw_type == 3) {
        if (car_axis_points.length == 3) {

            $("#caraxiseditTable").hide();
            $("#editTable").show();
            car_anno_draw_type++;
        }
        else
            alert("You MUST have only THREE points selected")

    }
    showCarInstruction_2()

    return false;
}
// function undoCarAxisAnno() {



//     car_axis_points = boundaryDrawer.getCardinalPoints();

//     if (car_anno_draw_type == 0){
//         if (car_axis_points.length == 0) {
//             car_anno_draw_type ++;
//         }
//     }
//     else if (car_anno_draw_type == 1){
//         if (car_axis_points.length == 1) {
//             car_anno_draw_type ++;
//         }
//     }
//     else if (car_anno_draw_type == 2){
//         if (car_axis_points.length == 2) {
//             car_anno_draw_type ++;
//         }
//     }
//     else if (car_anno_draw_type == 3)
//     {
//         if (car_axis_points.length == 3) {

//             $("#caraxiseditTable").hide();
//             $("#editTable").show();
//             car_anno_draw_type ++;
//         }

//     }
//     if (car_anno_draw_type < 0)
//         car_anno_draw_type = 0
//     showCarInstruction_2()

//     return false;
// }
function UndoButtonOnClick_2() {


    car_axis_points = boundaryDrawer.getCardinalPoints();

    if (car_axis_points.length < car_anno_draw_type)
        car_anno_draw_type--;
    boundaryDrawer.undo();

    if (car_anno_draw_type < 0)
        car_anno_draw_type = 0
    showCarInstruction_2()

    return false;
}



function UndoButtonOnClick() {
    if (button_type == 1) {
        UndoButtonOnClick_2()
    }
    else {
        axisDrawer.undo()
    }
    return false;
}

function polygonSelected(e) {
    var poly = this.getAttribute('points');
    var id = this.id;
    var index = -1;
    for (var i = 0; i < polygonDivisions.length; i++) {
        if (polygonDivisions[i][0].id == id) {
            index = i;
            break;
        }
    }
    if (index >= 0) {
        if (selectedPolygons[index] == false) {
            this.style.fill = "rgba(" + colors[existingCategoryIndexes[index]] + ",0.7)"
            selectedPolygons[index] = true;
        }
        else {
            this.style.fill = "rgba(" + colors[existingCategoryIndexes[index]] + ",0.3)"
            selectedPolygons[index] = false;
        }
    }
}


function categoryChanged() {
    //var radioButtonList = $("#CategoryRadioButtonList")[0];
    var checked_radio = $("[id*=CategorySelection_RadioButtonList] input:checked");
    var items = $("[id*=CategorySelection_RadioButtonList] input:radio");
    category = checked_radio.val();
    for (var i = 0; i < items.length; i++) {
        if (items[i].checked == true) {
            categoryIndex = i;
            break;
        }
    }
    BoundaryDrawer.color = colors[categoryIndex];
    currentColor = colors[categoryIndex];

    //$("#CategoryMentionLabel")[0].innerHTML = category;
}

function jsonStringifyResult() {

    var ret = "{\"objects\":{\"caxis\":{\"vertices\":";

    var car_axis_points_new = new Array();
    for (var i = 0; i < car_axis_points.length; i++) {
        var newPoint = new Array();
        newPoint.push((car_axis_points[i][0] * scalex) | 0);// make it integer
        newPoint.push((car_axis_points[i][1] * scaley) | 0);
        car_axis_points_new.push(newPoint);
    }
    var boundary = JSON.stringify(car_axis_points_new);
    ret += boundary;
    ret += "},";

    ret += "\"xvppoints\":{\"vertices\":";

    var x_axis_points_new = new Array();
    for (var i = 0; i < x_axis_points.length; i++) {
        var newPoint = new Array();
        newPoint.push((x_axis_points[i][0] * scalex) | 0);// make it integer
        newPoint.push((x_axis_points[i][1] * scaley) | 0);
        x_axis_points_new.push(newPoint);
    }
    var boundary = JSON.stringify(x_axis_points_new);
    ret += boundary;
    ret += "},";

    ret += "\"yvppoints\":{\"vertices\":";

    var y_axis_points_new = new Array();
    for (var i = 0; i < y_axis_points.length; i++) {
        var newPoint = new Array();
        newPoint.push((y_axis_points[i][0] * scalex) | 0);// make it integer
        newPoint.push((y_axis_points[i][1] * scaley) | 0);
        y_axis_points_new.push(newPoint);
    }

    var boundary = JSON.stringify(y_axis_points_new);
    ret += boundary;
    ret += "},";

    ret += "\"zvppoints\":{\"vertices\":";

    var z_axis_points_new = new Array();
    for (var i = 0; i < z_axis_points.length; i++) {
        var newPoint = new Array();
        newPoint.push((z_axis_points[i][0] * scalex) | 0);// make it integer
        newPoint.push((z_axis_points[i][1] * scaley) | 0);
        z_axis_points_new.push(newPoint);
    }
    var boundary = JSON.stringify(z_axis_points_new);
    ret += boundary;
    ret += "}";

    ret += "},\"displayScaleReductionX\":" + scalex + ",";
    ret += "\"displayScaleReductionY\":" + scaley + ",";
    ret += "\"imageWidth\":" + imageWidth + ",";
    ret += "\"imageHeight\":" + imageHeight + "}";
    console.log(ret)
    return ret;
}

function submit_button_on_click() {

    updateDisplayParams();

    //$("#buttonTable").hide();
    //$("#addDropTable").hide();
    //$("#editTable").hide();
    $("#buttonDiv").hide();
    $("#CategorySelectionPanel").hide();
    if (car_axis_points.length == 0) {
        if (confirm("You haven't selected the car axis , are you sure you want to submit?")) {
            var ans = jsonStringifyResult();
            $("#Hidden_Result")[0].value = ans;
            // alert("All Done")
            return true;
        }
    }
    else if (x_axis_points.length < 3) {
        if (confirm("You haven't selected atleast 3 parallel line to the car length , are you sure you want to submit?")) {
            var ans = jsonStringifyResult();
            $("#Hidden_Result")[0].value = ans;
            // alert("All Done")
            return true;
        }
    }
    else if (y_axis_points.length < 3) {
        if (confirm("You haven't selected atleast 3 parallel line to the car width , are you sure you want to submit?")) {
            var ans = jsonStringifyResult();
            $("#Hidden_Result")[0].value = ans;
            // alert("All Done")
            return true;
        }
    }
    else if (z_axis_points.length < 3) {
        if (confirm("You haven't selected atleast 3 parallel line to the car height , are you sure you want to submit?")) {
            var ans = jsonStringifyResult();
            $("#Hidden_Result")[0].value = ans;
            // alert("All Done")
            return true;
        }
    }
    else {
        if (confirm("Are you sure you want to submit?")) {
            var ans = jsonStringifyResult();
            $("#Hidden_Result")[0].value = ans;
            // alert("All Done")
            return true;
        }
    }
    $("#buttonDiv").show();

    return false;
}