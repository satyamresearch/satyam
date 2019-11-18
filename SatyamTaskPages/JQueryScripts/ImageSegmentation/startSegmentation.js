$(document).ready(function () {
    prepare();
});

var ImageContainer = null;
var Image = null;
//var existingObjects = new Array(); // each object might have multiple polygons with boolean sign
var currentObjectCardinalPoints = new Array();
var existingObjectIDs = new Array();
var existingParts = new Array();
var existingCategoryIndexes = new Array();
var polygonDivisions = new Array();
var selectedPolygons = new Array();
var svgDivisions = new Array();

var PolygonsInDrawnSequence = null;

var PNG = new Array();


var drawBoundary = "false";
var canvas = null;

var boundaryDrawer = null;


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

    //Instruciton hide show
    showStartingInstruction();

    var checked_radio = $("[id*=CategorySelection_RadioButtonList] input:checked");
    category = checked_radio.val();
    $("#CategoryMentionLabel")[0].innerHTML = category;

    for (var i = 0; i < displayWidth * displayHeight; i++) {
        PNG.push(0);
    }
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
    $("#FirstPolyInstruction").hide();
    $("#SecondPolyInstruction").hide();
    $("#DrawingInstruction").hide();
}
function showFirstPolygonInstruction() {
    $("#StartingInstruction").hide();
    $("#FirstPolyInstruction").show();
    $("#SecondPolyInstruction").hide();
    $("#DrawingInstruction").hide();
}
function showSecondPolygonInstruction() {
    $("#StartingInstruction").hide();
    $("#FirstPolyInstruction").hide();
    $("#SecondPolyInstruction").show();
    $("#DrawingInstruction").hide();
}

function showDrawingInstruction() {
    $("#StartingInstruction").hide();
    $("#FirstPolyInstruction").hide();
    $("#SecondPolyInstruction").hide();
    $("#DrawingInstruction").show();
}

function NewObjectOnClick() {
    $("#QuestionDiv").hide();
    var button = $("#NewObjectButton")[0];
    if (button.value == "Start A New Object") {
        $("#attentionDiv").show();
        $("#CategorySelectionPanel").hide();
        $("#addDropTable").show();
        $("#buttonTable").hide();
        $("#NewObjectButtonTable").hide();
        
        button.value = "Yes. Finish Object";
        //existingObjects.push(new Array());
        currentObjectCardinalPoints = new Array();
        existingObjectIDs.push(-1);
        existingCategoryIndexes.push(-1);
        polygonDivisions.push(null);
        selectedPolygons.push(false);
        svgDivisions.push(null);

        showFirstPolygonInstruction();

        PolygonsInDrawnSequence =new Array();

    } else {
        $("#attentionDiv").hide();
        $("#CategorySelectionPanel").show();
        $("#addDropTable").hide();
        $("#buttonTable").show();
        button.value = "Start A New Object";

        var button2 = $("#AddPolygon")[0];

        if (button2.value == "No. Draw Another Boundary") {
            button2.value = "Draw A New Boundary"
        }

        PolygonCounter = 0;
        ObjCounter++;

        showStartingInstruction();
    }
    return false;
}




function AddPolygonButtonOnClick()
{
    DeleteRegionFlag = false;
    $("#NewObjectButtonTable").hide();
    $("#addDropTable").hide();
    $("#QuestionDiv").hide();
    $("#editTable").show();

    var button = $("#AddPolygon")[0];

    if (button.value == "Draw A New Boundary") {
        button.value = "No. Draw Another Boundary"
    }

    for (var i = 0; i < polygonDivisions.length-1; i++) {
        polygonDivisions[i].unbind('click');
        selectedPolygons[i] = false;
    }
    drawBoundary = true;

    boundaryDrawer = new BoundaryDrawer(currentColor, category, currentObjectCardinalPoints,
        existingObjectIDs, existingParts, existingCategoryIndexes, ObjCounter, PolygonCounter, displayWidth, displayHeight, PNG);
    boundaryDrawer.init();

    showDrawingInstruction();
    return false;
}

function DeletePolygonButtonOnClick() {
    
    if (PolygonsInDrawnSequence.length == 0) return false;
    boundaryDrawer = new BoundaryDrawer(currentColor, category, currentObjectCardinalPoints,
        existingObjectIDs, existingParts, existingCategoryIndexes, ObjCounter, PolygonCounter, displayWidth, displayHeight,PNG);

    var lastPolygonDrawn = PolygonsInDrawnSequence[PolygonsInDrawnSequence.length - 1];

    boundaryDrawer.DeleteLastDrawPolygon(lastPolygonDrawn);

    PolygonsInDrawnSequence.pop();

    globalUpdate();

    delete boundaryDrawer;

    return false;
}

function globalUpdate() {
    PolygonCounter = boundaryDrawer.getNoPolygonOfCurrentObject();

    PolygonCounter++;

    var lastIndex = existingObjectIDs.length-1;
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
    $("#NewObjectButtonTable").show();
    $("#addDropTable").show();
    $("#QuestionDiv").show();
    $("#editTable").hide();
    $("#NewObjectButtonTable").show();

    var fin = boundaryDrawer.finish();
    

    globalUpdate();

    if (!fin) {
        PolygonsInDrawnSequence.push(boundaryDrawer.getCurrentPolygonCardinalPoints());
    }
    

    $("#canvas")[0].style.zIndex = 0;
    for (var i = 0; i < polygonDivisions.length; i++) {
        polygonDivisions[i][0].style.zIndex = 10;
        polygonDivisions[i].bind('click', polygonSelected);
    }
    

    

    if (PolygonCounter == 0) {
        showFirstPolygonInstruction();
    } else {
        showSecondPolygonInstruction();
    }

    
    delete boundaryDrawer;
    drawBoundary = false;
    
    return false;
}


function DeleteObjectClick(){
    var deleted = false;
    var objIdsToBeRemoved = new Array();
    var ObjIndexesToBeRemoved = new Array();
    for(var i=0;i<polygonDivisions.length;i++)
    {
        if(selectedPolygons[i] == true) //remove this division
        {   
            svgDivisions[i].remove();
            objIdsToBeRemoved.push(existingObjectIDs[i]);
            ObjIndexesToBeRemoved.push(i);
            deleted = true;
        }
    }
    if (deleted) {
        var partsIndexToBeRemoved = new Array();
        for (var j = 0; j < existingParts.length; j++) 
        {
            for (var i = 0; i < objIdsToBeRemoved.length; i++)
            {
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

function UndoButtonOnClick()
{
    boundaryDrawer.undo();
    return false;
}

function polygonSelected(e)
{
    var poly = this.getAttribute('points');
    var id = this.id;
    var index = -1;
    for (var i = 0; i < polygonDivisions.length; i++)
    {
        if(polygonDivisions[i][0].id == id)
        {
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
            this.style.fill = "rgba(" + colors[existingCategoryIndexes[index]]+",0.3)"
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
    
    $("#CategoryMentionLabel")[0].innerHTML = category;
}

function jsonStringifyResult() {
    var existingObjects = new Array();
    for (var p = 0; p < existingParts.length; p++) {
        var objid = existingParts[p].getObjID();
        var index = existingObjectIDs.indexOf(objid);
        if (existingObjects[index] == null) {
            existingObjects[index] = new Array();
        }

        var polys = existingParts[p].getPolys();
        for (var q = 0; q < polys.length; q++) {
            existingObjects[index].push(polys[q]);
        }
    }

    var items = $("[id*=CategorySelection_RadioButtonList] input:radio");

    var ret = "{\"objects\":[";
    for (var i = 0; i < existingObjects.length; i++) {
        ret += "{\"segment\":{\"polygons\":[";
        
        // convert back to the original image scale
        for (var j = 0; j < existingObjects[i].length; j++) {
            var newPoly = new Array();
            ret += "{\"vertices\":";
            for (var k = 0; k < existingObjects[i][j].length; k++) {
                var newPoint = new Array();
                newPoint.push((existingObjects[i][j][k][0] * scalex) | 0);// make it integer
                newPoint.push((existingObjects[i][j][k][1] * scaley) | 0);
                newPoly.push(newPoint);
            }
            var boundary = JSON.stringify(newPoly);
            ret += boundary;

            ret += "}";

            if (j != existingObjects[i].length - 1) {
                ret += ",";
            }
            
        }
        ret+="]},"
        
        //var colorIndex = colors.indexOf(existingCategoryIndexes[i]);
        //var categoryIndex = colorIndex;
        ret += "\"Category\":\"" + items[existingCategoryIndexes[i]].value + "\"}";
        if (i != existingObjects.length - 1) {
            ret += ",";
        }
    }

    ret += "],\"displayScaleReductionX\":" + scalex + ",";
    ret += "\"displayScaleReductionY\":" + scaley + ",";
    ret += "\"imageWidth\":" + imageWidth + ",";
    ret += "\"imageHeight\":" + imageHeight + "}";
    return ret;
}

function submit_button_on_click() {

    updateDisplayParams();

    //$("#buttonTable").hide();
    //$("#addDropTable").hide();
    //$("#editTable").hide();
    $("#buttonDiv").hide();
    $("#CategorySelectionPanel").hide();

    if (existingParts.length == 0) {
        if (confirm("Nothing has been segmented , are you sure you want to submit?")) {
            var ans = jsonStringifyResult();
            $("#Hidden_Result")[0].value = ans;
            return true;
        }
    }
    else {
        if (confirm("Are you sure you want to submit?")) {
            var ans = jsonStringifyResult();
            $("#Hidden_Result")[0].value = ans;
            return true;
        }
    }
    $("#buttonDiv").show();
    return false;
}