$(document).ready(function () {
    prepare();
});

var ImageContainer = null;
var Image = null;
var existingParts = new Array();
var existingCategoryIndexes = new Array();
var polygonDivisions = new Array();
var selectedPolygons = new Array();
var svgDivisions = new Array();


var drawBoundary = "false";
var canvas = null;

var boundaryDrawer = null;


var categoryIndex = 0;
var category;
var colors = [["256,0,0"], ["0,256,0"], ["0,0,256"], ["256,0,256"], ["256,256,0"], ["0,256,256"], ["0,0,0"], ["256,256,256"],
    ["128,0,0"], ["0,128,0"], ["0,0,128"], ["128,0,128"], ["128,128,0"], ["0,128,128"], ["0,0,0"], ["128,128,128"]];
var currentColor = ["256,0,0"];
//var colors = ["red", "blue", "green", "purple", "yellow", "violet"];

var ObjCounter = 1;


var displayWidth;
var displayHeight;

var imageWidth;
var imageHeight;

var scalex;
var scaley;


function prepare() {
    boundaries = new Array();
    ImageContainer = $("#ImageDivision")[0];
    Image = $("#TheImage");

    updateDisplayParams();

    //$("#CategorySelection_RadioButtonList")[0].disabled = true;
    $("#CategorySelection_RadioButtonList_0")[0].checked = true;
    $("#attentionDiv").hide();
    var checked_radio = $("[id*=CategorySelection_RadioButtonList] input:checked");
    category = checked_radio.val();
    $("#CategoryMentionLabel")[0].innerHTML = category;
}


function updateDisplayParams() {
    displayWidth = Image[0].clientWidth;
    displayHeight = Image[0].clientHeight;
    imageWidth = Image[0].naturalWidth;
    imageHeight = Image[0].naturalHeight;
    scalex = imageWidth / displayWidth;
    scaley = imageHeight / displayHeight;
}

function DrawButtonOnClick()
{


    var button = $("#DrawButton")[0];
    if(button.value == "Start Drawing New Boundary")
    {
        $("#attentionDiv").show();
        $("#CategorySelectionPanel").hide();
        for (var i = 0; i < polygonDivisions.length; i++) {
            polygonDivisions[i].unbind('click');
            //polygonDivisions[i][0].style.fill = "rgba(255,0,0,0.3)"
            selectedPolygons[i] = false;
        }
        button.value = "Finish Drawing New Boundary";
        drawBoundary = true;
        $("#UndoButton")[0].value = "Undo";
        boundaryDrawer = new BoundaryDrawer(currentColor, category, new Array(), existingParts, existingCategoryIndexes, ObjCounter);
        boundaryDrawer.init();
        ObjCounter = ObjCounter + 1;
    } else {
        $("#attentionDiv").hide();
        $("#CategorySelectionPanel").show();
        boundaryDrawer.finish();
        var cardinalPoints = boundaryDrawer.getCardinalPoints();
        if (cardinalPoints != null) {
            existingParts.push(cardinalPoints);
            existingCategoryIndexes.push(categoryIndex);

            var polygonDiv = boundaryDrawer.getPolygonDivision();
            polygonDivisions.push(polygonDiv);
            selectedPolygons.push(false);
            svgDivisions.push(boundaryDrawer.getSVGDivision());
        }
        delete boundaryDrawer;
        button.value = "Start Drawing New Boundary";
        drawBoundary = false;
        $("#UndoButton")[0].value = "Delete Boundaries";
        $("#canvas")[0].style.zIndex = 0;
        for(var i=0;i<polygonDivisions.length;i++)
        {
            polygonDivisions[i][0].style.zIndex = 10;
            polygonDivisions[i].bind('click',polygonSelected);
        }
    }
    return false;
}

function UndoButtonOnClick()
{
    if($("#UndoButton")[0].value == "Undo")
    {
        boundaryDrawer.undo();
    }
    else {
        var deleted = false;
        var cardinalsToBeRemoved = new Array();
        for(var i=0;i<polygonDivisions.length;i++)
        {
            if(selectedPolygons[i] == true) //remove this division
            {
                //remove the svg division
                svgDivisions[i].remove();      
                cardinalsToBeRemoved.push(existingParts[i]);
                deleted = true;
            }
        }
        if (deleted) {
            for (var i = 0; i < cardinalsToBeRemoved.length; i++)
            {
                var index = existingParts.indexOf(cardinalsToBeRemoved[i]);
                existingParts.splice(index, 1);
                existingCategoryIndexes.splice(index, 1);
                svgDivisions.splice(index, 1);
                polygonDivisions.splice(index, 1);
                selectedPolygons.splice(index, 1);
            }
            boundaryDrawer = new BoundaryDrawer(currentColor, category,new Array(), existingParts, existingCategoryIndexes,ObjCounter);
            boundaryDrawer.init(); //this will redraw the image
            delete boundaryDrawer;
            $("#canvas")[0].style.zIndex = 0;
            $("#canvas").unbind('click');
        }
    }
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

    var items = $("[id*=CategorySelection_RadioButtonList] input:radio");

    var ret = "{\"objects\":[";
    for (var i = 0; i < existingParts.length; i++) {
        ret += "{\"polygon\":{\"vertices\":";
        // convert back to the original image scale
        for (var j = 0; j < existingParts[i].length; j++) {
            existingParts[i][j][0] = (existingParts[i][j][0] * scalex) | 0;// make it integer
            existingParts[i][j][1] = (existingParts[i][j][1]* scaley) | 0;
        }

        var boundary = JSON.stringify(existingParts[i]);
        ret += boundary;

        ret += "},";
        //var colorIndex = colors.indexOf(existingCategoryIndexes[i]);
        //var categoryIndex = colorIndex;
        ret += "\"Category\":\"" + items[existingCategoryIndexes[i]].value + "\"}";
        if (i != existingParts.length - 1) {
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
    //$("#SubmitButton")[0].disabled =true;

    updateDisplayParams();

    $("#buttonTable").hide();
    if (existingParts.length == 0) {
        if (confirm("You have segmented nothing, are you sure you want to submit?")) {
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
    $("#buttonTable").show();
    return false;
}