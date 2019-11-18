$(document).ready(function () {
    prepare();
});

var boxDrawer = null;
var boxes = null;
var ImageContainer = null;
var Image = null;

var categoryIndex = 0;
var category;
var currentColor = "red";
var colors = ["red", "blue", "green", "purple", "yellow", "violet"];



var displayWidth; 
var displayHeight;

var imageWidth; 
var imageHeight;

var scalex;
var scaley;

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
    boxDrawer.color = colors[categoryIndex];
    currentColor = colors[categoryIndex];
    boxDrawer.vcrosshair[0].style.backgroundColor = currentColor;
    boxDrawer.hcrosshair[0].style.backgroundColor = currentColor;
    if (boxDrawer.handle != null) {
        boxDrawer.handle[0].style.borderColor = currentColor;
    }
    $("#CategoryMentionLabel")[0].innerHTML = category;
}

function DrawBoundary() {
    boundaryString = $("#Hidden_BoundaryLines")[0].value;

    boundary = JSON.parse(boundaryString);
    if (boundary.length > 0) {

        for (var i = 0; i < boundary.length; i++) {
            boundary[i].x1 = Math.floor(boundary[i].x1 / scalex);
            boundary[i].y1 = Math.floor(boundary[i].y1 / scaley);
            boundary[i].x2 = Math.floor(boundary[i].x2 / scalex);
            boundary[i].y2 = Math.floor(boundary[i].y2 / scaley);
        }
        drawLines(ImageContainer, boundary, 'red');
    }
    else {
        drawImageBoundary(ImageContainer, 'red');
    }

}

function prepare()
{
    boxes = new Array();
    ImageContainer = $("#ImageDivision")[0];    
    Image = $("#DisplayImage");
    //sleep(500);
    displayWidth = Image[0].clientWidth;
    displayHeight = Image[0].clientHeight;
    imageWidth = Image[0].naturalWidth;
    imageHeight = Image[0].naturalHeight;
    scalex = imageWidth / displayWidth;
    scaley = imageHeight / displayHeight;
    DrawBoundary();
    
    $("#buttonTable").show();
    //Image.load(function () { DrawBoundary(); });
    

    $("#CategorySelection_RadioButtonList")[0].disabled = true;
    $("#CategorySelection_RadioButtonList_0")[0].checked = true;
   // $("#CategorySelectionPanel").hide();    
    $("#attentionDiv").hide();
    var checked_radio = $("[id*=CategorySelection_RadioButtonList] input:checked");
    category = checked_radio.val();
    $("#CategoryMentionLabel")[0].innerHTML = category;
}

function NextBoxOnClick()
{

    displayWidth = Image[0].clientWidth;
    displayHeight = Image[0].clientHeight;
    imageWidth = Image[0].naturalWidth;
    imageHeight = Image[0].naturalHeight;
    scalex = imageWidth / displayWidth;
    scaley = imageHeight / displayHeight;
    DrawBoundary();

    var text = $("#DrawNextBoxButton")[0].value;
    if(text == "Draw New Boxes")
    {
        $("#DrawNextBoxButton")[0].value = "Edit/Delete Boxes";
        //$("#CategorySelectionPanel").show();
        //$("#CategorySelectionPanel").enabled = true;
        $("#CategorySelection_RadioButtonList")[0].disabled = false;
//        $("#CategorySelection_RadioButtonList").enable();
        $("#attentionDiv").show();
        boxDrawer = new BoxDrawer(ImageContainer, Image, boxes,currentColor,category);
        boxDrawer.enable();
        for (var i = 0; i < boxes.length; i++) {
            if (boxes[i].handle != null) {
                boxes[i].disallowEdit();
            }
            else {
                boxes.splice(i,1);
            }
        }

    }
    else {
        $("#DrawNextBoxButton")[0].value = "Draw New Boxes";
        //$("#CategorySelectionPanel").hide();
        //$("#CategorySelectionPanel").disable();
        //$("#CategorySelection_RadioButtonList").diaable();
        $("#CategorySelection_RadioButtonList")[0].disabled = true;
        $("#attentionDiv").hide();
        boxDrawer.disable();
        boxDrawer.clear();
        boxDrawer = null;
        for(var i=0;i<boxes.length;i++)
        {
            boxes[i].allowEdit();
        }
    }
    return false;
}




function jsonStringifyResult() {
    var displayWidth = Image[0].clientWidth;
    var displayHeight = Image[0].clientHeight;

    var imageWidth = Image[0].naturalWidth;
    var imageHeight = Image[0].naturalHeight;

    var scalex = imageWidth / displayWidth;
    var scaley = imageHeight / displayHeight;


    var ret = "{\"objects\":[";
    for (var i = 0; i < boxes.length; i++) {
        ret += "{\"boundingBox\":{";
        var tlx = Math.floor(scalex*boxes[i].bounds[0][0]);
        var tly = Math.floor(scaley *boxes[i].bounds[0][1]);
        var brx = Math.floor(scalex *boxes[i].bounds[1][0]);
        var bry = Math.floor(scaley *boxes[i].bounds[1][1]);
        var centerx = Math.floor((tlx + brx) / 2);
        var centery = Math.floor((tly + bry) / 2);
        ret += "\"tlx\":" + tlx + ",";
        ret += "\"tly\":" + tly + ",";
        ret += "\"brx\":" + brx + ",";
        ret += "\"bry\":" + bry + ",";
        ret += "\"centerx\":" + centerx + ",";
        ret += "\"centery\":" + centery + "},";
        ret += "\"Category\":\"" + boxes[i].category + "\"}";
        if (i != boxes.length - 1) {
            ret += ",";
        }
    }

    ret += "],\"displayScaleReductionX\":" + scalex + ",";
    ret += "\"displayScaleReductionY\":" + scaley + ",";
    ret += "\"imageWidth\":" + imageWidth + ",";
    ret += "\"imageHeight\":" + imageHeight + "}";
    return ret;
}

function load_previous_results() {
    //// the data here should be in json format
    data = [{
        "boundingBox": { "tlx": 931, "tly": 356, "brx": 1036, "bry": 462, "centerx": 983, "centery": 409 },
        "Category": "car"
    }];
        
    //prevBoxes = $("#Hidden_PrevBoxes")[0].value;
    //data = JSON.parse(prevBoxes);
    
    boxDrawer = new BoxDrawer(ImageContainer, Image, boxes, currentColor, category);
    boxDrawer.enable();
    //for (var i = 0; i < boxes.length; i++) {
    //    if (boxes[i].handle != null) {
    //        boxes[i].disallowEdit();
    //    }
    //    else {
    //        boxes.splice(i, 1);
    //    }
    //}

    var items = $("[id*=CategorySelection_RadioButtonList] input:radio");
    for (var j = 0; j < data.length; j++) {
        var ci = 0;
        
        for (var k = 0; k < items.length; k++) {
            if (items[k].val == data[j]["Category"]) {
                ci = k;
                break;
            }
        }
        boxDrawer.color = colors[ci];

        var bbox = data[j]["boundingBox"];
        boxDrawer.startdrawing(bbox["tlx"], bbox["tly"]);
        boxDrawer.finishdrawing(bbox["brx"], bbox["bry"]);
    }
    //reset color
    categoryChanged();

    
}

function submit_button_on_click() {
    //$("#SubmitButton")[0].disabled =true;
    $("#buttonTable").hide();
    if (boxes.length == 0) {
        if (confirm("You have marked no box, are you sure you want to submit?")) {
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
    return false;
}