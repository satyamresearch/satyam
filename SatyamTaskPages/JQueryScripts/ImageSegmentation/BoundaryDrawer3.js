colors = [["256,0,0"], ["0,256,0"], ["0,0,256"], ["256,0,256"], ["256,256,0"], ["0,256,256"], ["0,0,0"], ["256,256,256"],
    ["128,0,0"], ["0,128,0"], ["0,0,128"], ["128,0,128"], ["128,128,0"], ["0,128,128"], ["128,128,128"],
    ["192,0,0"], ["0,192,0"], ["0,0,192"], ["192,0,192"], ["192,192,0"], ["0,192,192"], ["192,192,192"]];



function BoundaryDrawer(_color, _categoryIndex, cPoints, _existingObjectIDs, _existingParts, _existingCategoryIndexes, cntr, polycntr, displayWidth, displayHeight, png)
{
    var canvas = $("#canvas");
    var color = _color;
    var categoryIndex = _categoryIndex;
    var colorString = "rgb(" + color + ")";
    var colorFadeString = "rgba(" + color + ",0.5)";
    var currentPolygonCardinalPoints = new Array();
    var cardinalPointsOfCurrentObject = cPoints; // a list of polygons now
    var existingObjectIDs = _existingObjectIDs;
    var existingParts = _existingParts;  // a list of parts, which is a list of polygons
    var existingCategoryIndexes = _existingCategoryIndexes;
    var svgPolygonDivision = null;
    var svgDivision = null;
    var ObjID = cntr;
    var PolygonID = polycntr;
    var width = displayWidth;
    var height = displayHeight;
    var PNG = png;
    var me = this;

    this.init = function()
    {
        if (PolygonID != cardinalPointsOfCurrentObject.length) {
            console.log("PolygonID error");
        } 

        PolygonID = cardinalPointsOfCurrentObject.length;

        cardinalPointsOfCurrentObject.push(new Array());

        canvas[0].style.zIndex = 100;
        canvas.bind('click', onclickDrawing);
        me.clearAndRedraw();
    }

    

    this.getObjID = function() {
        return ObjID;
    }

    this.getNoPolygonOfCurrentObject = function () {
        return cardinalPointsOfCurrentObject.length;
    }

    this.getCurrentPolygonCardinalPoints = function () {
        return currentPolygonCardinalPoints;
    }

    function onclickDrawing(e)
    {
        var rect = canvas[0].getBoundingClientRect();
        var x = Math.floor(event.clientX - rect.left);
        var y = Math.floor(event.clientY - rect.top);
        var xy = new Array();
        xy.push(x);
        xy.push(y);
        cardinalPointsOfCurrentObject[PolygonID].push(xy);
        currentPolygonCardinalPoints.push(xy);
        me.clearAndRedraw();
    }


    this.putPoint = function(xy, color)
    {
        var pointSize = 4; // Change according to the size of the point.
        var ctx = canvas[0].getContext("2d");
        ctx.fillStyle = "rgb(" + color + ")";
        ctx.beginPath(); //Start path
        ctx.arc(xy[0], xy[1], pointSize, 0, Math.PI * 2, true); // Draw a point using the arc function of the canvas with a point structure.
        ctx.fill(); // 
    }

    this.putFadePoint = function (xy, color) {
        var pointSize = 4; // Change according to the size of the point.
        var ctx = canvas[0].getContext("2d");
        ctx.fillStyle = "rgba(" + color + ",0.5)"; 
        ctx.beginPath(); //Start path
        ctx.arc(xy[0], xy[1], pointSize, 0, Math.PI * 2, true); // Draw a point using the arc function of the canvas with a point structure.
        ctx.fill(); // 
        ctx.closePath();
    }

    this.drawLine = function(startxy,endxy, color)
    {
        var ctx = canvas[0].getContext("2d");
        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.strokeStyle = "rgb(" + color + ")";
        ctx.moveTo(startxy[0], startxy[1]);
        ctx.lineTo(endxy[0], endxy[1]);
        ctx.stroke();
        ctx.closePath();
    }

    this.drawFadeLine = function (startxy, endxy, color) {
        var ctx = canvas[0].getContext("2d");
        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.strokeStyle = "rgba(" + color + ",0.5)";
        ctx.moveTo(startxy[0], startxy[1]);
        ctx.lineTo(endxy[0], endxy[1]);
        ctx.stroke();
        ctx.closePath();
    }


    this.clearAndRedraw = function(xy)
    {
        var ctx = canvas[0].getContext("2d");
        ctx.clearRect(0, 0, canvas[0].width, canvas[0].height);
        drawAllExistingBoundariesFaded();
        me.draw();
    }


    function drawExistingBoundaryFaded(points, colorIndexes)
    {
        
            if (points.length > 0) {
                me.putFadePoint(points[0], colors[colorIndexes]);

                for (var i = 1; i < points.length; i++) {
                    me.putFadePoint(points[i], colors[colorIndexes]);
                    me.drawFadeLine(points[i - 1], points[i], colors[colorIndexes]);
                }

                if (points.length > 2) {
                    me.drawFadeLine(points[points.length - 1], points[0], colors[colorIndexes]);
                }
            }
        
        
    }

    function drawAllExistingBoundariesFaded()
    {
        // the last one is current
        for (var i = 0; i < existingParts.length-1;i++)
        {
            var objid = existingParts[i].getObjID();
            for (var j = 0; j < existingParts[i].length; j++) {
                drawExistingBoundaryFaded(existingParts[i][j], existingCategoryIndexes[objid]);
            }
        }
    }

    this.draw = function()
    {
        for (var j = 0; j < cardinalPointsOfCurrentObject.length; j++) {
            if (cardinalPointsOfCurrentObject[j].length > 0) {
                me.putPoint(cardinalPointsOfCurrentObject[j][0], color);
            }
            for (var i = 1; i < cardinalPointsOfCurrentObject[j].length; i++) {
                me.putPoint(cardinalPointsOfCurrentObject[j][i], color);
                me.drawLine(cardinalPointsOfCurrentObject[j][i - 1], cardinalPointsOfCurrentObject[j][i], color);
            }
            if (j != cardinalPointsOfCurrentObject.length-1 &&  cardinalPointsOfCurrentObject[j].length > 2) {
                me.drawLine(cardinalPointsOfCurrentObject[j][cardinalPointsOfCurrentObject[j].length - 1],
                    cardinalPointsOfCurrentObject[j][0], color);
            }
        }
    }

    this.drawFinal = function () {
        me.draw();
        if (cardinalPointsOfCurrentObject.length == 0) return;
        var j = cardinalPointsOfCurrentObject.length-1;
            if (cardinalPointsOfCurrentObject[j].length > 2) {
                me.drawLine(cardinalPointsOfCurrentObject[j][cardinalPointsOfCurrentObject[j].length - 1],
                    cardinalPointsOfCurrentObject[j][0], color);
            }
        
        
    }


    this.drawFade = function () {
        for (var j = 0; j < cardinalPointsOfCurrentObject.length; j++) {
            if (cardinalPointsOfCurrentObject[j].length > 0) {
                me.putFadePoint(cardinalPointsOfCurrentObject[j][0], color);
            }
            for (var i = 1; i < cardinalPointsOfCurrentObject[j].length; i++) {
                me.putFadePoint(cardinalPointsOfCurrentObject[j][i], color);
                me.drawFadeLine(cardinalPointsOfCurrentObject[j][i - 1], cardinalPointsOfCurrentObject[j][i], color);
            }
            if (j != cardinalPointsOfCurrentObject.length - 1 && cardinalPointsOfCurrentObject[j].length > 2) {
                me.drawFadeLine(cardinalPointsOfCurrentObject[j][cardinalPointsOfCurrentObject[j].length - 1],
                    cardinalPointsOfCurrentObject[j][0], color);
            }
        }
        
    }

    this.drawFadeFinal = function () {
        me.drawFade();
        var j = cardinalPointsOfCurrentObject.length - 1;
            if (cardinalPointsOfCurrentObject[j].length > 2) {
                me.drawFadeLine(cardinalPointsOfCurrentObject[j][cardinalPointsOfCurrentObject[j].length - 1],
                    cardinalPointsOfCurrentObject[j][0], color);
            }
        
        
    }



    

    this.undo = function()
    {
        if (cardinalPointsOfCurrentObject[PolygonID].length == 0)
        {
            return;
        }
        cardinalPointsOfCurrentObject[PolygonID].pop();
        currentPolygonCardinalPoints.pop();
        me.clearAndRedraw();
    }

    this.getCardinalPoints = function () {
        var results = new Array();
        for (var j = 0; j < cardinalPointsOfCurrentObject.length; j++) {
            if (cardinalPointsOfCurrentObject[j].length < 3) {
                results.push(null);
            }
            var res = new Array();
            for (var i = 0; i < cardinalPointsOfCurrentObject[j].length; i++) {
                var xy = new Array();
                xy.push(cardinalPointsOfCurrentObject[j][i][0]);
                xy.push(cardinalPointsOfCurrentObject[j][i][1]);
                res.push(xy);
            }
            results.push(res);
        }
        
        return results;
    }

    this.getExistingParts = function () {
        return existingParts;
    }


    this.getColor = function(){
        return color;
    }

    this.getPNG = function () {
        return PNG;
    }


    //function updateAndDrawPNG() {
    //    var poly = new Polygon(cardinalPointsOfCurrentObject[PolygonID]);
    //    for (var i = 0; i < displayWidth; i++) {
    //        for (var j = 0; j < displayHeight; j++){
    //            var p = new Array();
    //            p.push(i);
    //            p.push(j);
    //            if (poly.Interior(p)) {
    //                PNG[j * displayWidth + i] = categoryIndex;
    //            }

    //            if (PNG[j * displayWidth + i] != 0) {
    //                var c = PNG[j * displayWidth + i];
    //                putFadePoint(p, c);
    //            }
    //        }
    //    }
    //}
    
    
    //function createSVGPolygonDivision() {

    //    var minx = 100000;
    //    var maxx = 0;
    //    var miny = 100000;
    //    var maxy = 0;
    //    var id = "obj_" + ObjID;

    //    for (var j = 0; j < cardinalPointsOfCurrentObject.length; j++) {
    //        for (var i = 0; i < cardinalPointsOfCurrentObject[j].length; i++) {
    //            if (cardinalPointsOfCurrentObject[j][i][0] > maxx) {
    //                maxx = cardinalPointsOfCurrentObject[j][i][0];
    //            }
    //            if (cardinalPointsOfCurrentObject[j][i][1] > maxy) {
    //                maxy = cardinalPointsOfCurrentObject[j][i][1];
    //            }
    //            if (cardinalPointsOfCurrentObject[j][i][0] < minx) {
    //                minx = cardinalPointsOfCurrentObject[j][i][0];
    //            }
    //            if (cardinalPointsOfCurrentObject[j][i][1] < miny) {
    //                miny = cardinalPointsOfCurrentObject[j][i][1];
    //            }
    //        }
    //    }
        

    //    var width = maxx - minx;
    //    var height = maxy - miny;

    //    // remove previous svg of this obj, if any
    //    var element = document.getElementById(id);
    //    if (element != null) {
    //        element.parentNode.removeChild(element);
    //    }
        


    //    //var divString = "<svg viewBox = '" + minx + " " + miny + " " + maxx + " " + maxy + "' width= " + width + " height= " + height + " style='position:absolute'>";
    //    //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute'>";
    //    //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block'>";
    //    //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block;padding:" + miny + "px " + minx + "px'>";
    //    var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block;margin-left:" + minx + "px;margin-top:" + miny + "px'>";

    //    // polygon pointstring
    //    //var pointString = "'";
    //    //for (var i = 0; i < cardinalPoints.length; i++) {
    //    //    var x = cardinalPoints[i][0] - minx;
    //    //    var y = cardinalPoints[i][1] - miny;
    //    //    pointString += x + "," + y + " ";
    //    //}
    //    //pointString += "'";
    //    //divString += "<polygon id='" + id + "' points=" + pointString + " style='fill:rgba(" + color + ", 0.3)'>";

    //    var pointString = "'";
    //    for (var j = 0; j < cardinalPointsOfCurrentObject.length; j++) {
    //        if (cardinalPointsOfCurrentObject[j].length > 0) {
    //            var x0 = cardinalPointsOfCurrentObject[j][0][0] - minx;
    //            var y0 = cardinalPointsOfCurrentObject[j][0][1] - miny;
    //            pointString += "M" + x0 + ',' + y0 + " "; // move to
    //            for (var i = 1; i < cardinalPointsOfCurrentObject[j].length; i++) {
    //                var x = cardinalPointsOfCurrentObject[j][i][0] - minx;
    //                var y = cardinalPointsOfCurrentObject[j][i][1] - miny;
    //                pointString += "L" + x + "," + y + " "; // line to
    //            }
    //            // close path
    //            pointString += "Z ";
    //        }
    //    }
        
        
    //    pointString += "'";
    //    //var transparency = (ObjID % 5 + 1) / 5;
    //    var transparency = 0.5;
    //    divString += "<path id='" + id + "' fill-rule='evenodd' d=" + pointString + " style='fill:rgba(" + color + ", "+ transparency +")'>";

        
    //    divString += "</svg>";
    //    var div = $(divString).insertBefore($("#TheImage"));
    //    div[0].style.zIndex = 0;
    //    svgDivision = div;
    //    return $("#" + id);
    //}


    function updateSVGPolygonDivision(ObjectPoints, objid, color) {

        var id = "obj_" + objid;
        // remove previous svg of this obj, if any
        var element = document.getElementById(id);
        if (element != null) {
            element.parentNode.removeChild(element);
        }

        var CurrentObj = new ObjectInstance(ObjectPoints, objid, "", color);

        var divString = CurrentObj.getSVGDivisionString();

        var div = $(divString).insertBefore($("#TheImage"));
        div[0].style.zIndex = 0;
        svgDivision = div;
        return $("#" + id);
    }

    this.MergeAndReorganizeAllParts = function(){

        // If fully contained by another part (1-1 relationship)
        // If part  objID is same, add polygon to existing part, delete new part
        // If part objID is different,
        //    Exclude new part from the existing part, but not delete the new part

        var newPartID = existingParts.length - 1;
        var newPart = existingParts[newPartID];

        //var noAdditionalPolygonsToCurrentObject = 0;

        // check if the new object is containing any other object, if so deduct those object.

        // there could be multiple fully containING relationship
        // must treat each independently, with respect to the new drawn polygon
        var oldPartsContainedIndex = new Array();
        for (var i = 0; i < existingParts.length - 1; i++) {
            var oldPart = existingParts[i];
            if (oldPart.IsFullyContained(newPart)) {
                oldPartsContainedIndex.push(i);
            }
        }

        var oldPartsToBeRemovedIndex = new Array();
        for (var i = 0; i < oldPartsContainedIndex.length; i++) {
            var oldPart = existingParts[oldPartsContainedIndex[i]];
            var oldPartPolys = oldPart.getPolys();
            for (var k = 0; k < oldPartPolys.length; k++) {
                existingParts[newPartID].insertPoly(oldPartPolys[k]);
                //if (newPart.getObjID() != oldPart.getObjID()) {
                //    // adding polygons from other objects, which was not counted.
                //    // to the cardinal points of this part
                //    // update the cardinal points at last
                //    //cardinalPointsOfCurrentObject.push(oldPartPolys[k]);
                //    // to the counter
                //    noAdditionalPolygonsToCurrentObject = noAdditionalPolygonsToCurrentObject + 1;
                //}
            }
            if (newPart.getObjID() == oldPart.getObjID()) {
                oldPartsToBeRemovedIndex.push(oldPartsContainedIndex[i]);
            } 
        }

        // remove old fully contained parts of same obj
        var newExistingPart = new Array();
        for (var i = 0; i < existingParts.length - 1; i++) {
            var found = false;
            for (var j = 0; j < oldPartsToBeRemovedIndex.length; j++) {
                if (oldPartsToBeRemovedIndex[j] == i) {
                    found = true; break;
                }
            }
            if (!found) {
                newExistingPart.push(existingParts[i]);
            }
            //if (!oldPartsToBeRemovedIndex.includes(i)) {
            //    newExistingPart.push(existingParts[i]);
            //}
            
        }

        newExistingPart.push(existingParts[existingParts.length - 1]);
        existingParts = newExistingPart;
        

        // new part is still at the end of the array, but might be updated,
        // so still use newPart (the new drawn polygon) for the following judgement
        var newPartUpdated = existingParts[existingParts.length - 1]; // actually doesn't matter, this is the same as new part
        // changed interior rule to closed interval rule
        for (var i = 0; i < existingParts.length - 1; i++) {
            var oldPart = existingParts[i];
            if (newPart.IsFullyContained(oldPart)) {

                // when adding the newPart to the existing part, we should use the updated one.
                var newObjPolys = newPartUpdated.getPolys();
                for (var k = 0; k < newObjPolys.length; k++) {
                    existingParts[i].insertPoly(newObjPolys[k]);
                }

                if (newPart.getObjID() == oldPart.getObjID()) {
                    existingParts.pop();
                } else {
                    // TODO:::need to go back and change svg division  / data and all associtaed with that object
                    updateSVGDivision(oldPart.getObjID());
                    
                }

                break;// there should be only one fully containED relationship
            }
        }

        me.updateCardinalPointsOfCurrentObject();

        //return noAdditionalPolygonsToCurrentObject;
    }


    function updateSVGDivision(objid) {
        var index = existingObjectIDs.indexOf(objid);
        var c = colors[existingCategoryIndexes[index]];
        var allpolys = new Array();
        for (var p = 0; p < existingParts.length; p++) {
            if (existingParts[p].getObjID() == objid) {
                var polys = existingParts[p].getPolys();
                for (var q = 0; q < polys.length; q++) {
                    allpolys.push(polys[q]);
                }
                
            }
        }
        updateSVGPolygonDivision(allpolys,objid, c);
        
    }

    this.updateCardinalPointsOfCurrentObject = function () {
        var updatedCardinalPoints = new Array();
        for (var i = 0; i < existingParts.length; i++) {
            if (existingParts[i].getObjID() == ObjID) {
                var polys = existingParts[i].getPolys();
                for (var j = 0; j < polys.length; j++) {
                    updatedCardinalPoints.push(polys[j]);
                }                
            }
        }
        cardinalPointsOfCurrentObject = updatedCardinalPoints;
    }


    this.DeleteLastDrawPolygon = function (LastPolygonPoints) {
        // current obj is empty
        if (cardinalPointsOfCurrentObject.length == 0) return;

        me.init();
        
        // as if the same polygon was redrawn again to cancel the last drawn
        
        var newPolyID = cardinalPointsOfCurrentObject.length - 1;
        //for (var i = 0; i < cardinalPointsOfCurrentObject[newPolyID - 1].length; i++) {
        //    var xy = new Array(); 
        //    for (var j = 0; j < cardinalPointsOfCurrentObject[newPolyID - 1][i].length; j++) {
        //        var coord = cardinalPointsOfCurrentObject[newPolyID - 1][i][j];
        //        xy.push(coord);
        //    }
            
        //    cardinalPointsOfCurrentObject[newPolyID].push(xy);
        //}
        cardinalPointsOfCurrentObject[newPolyID] = LastPolygonPoints; 

        me.finish();
    }

    this.finish = function()
    {
        canvas.unbind('click');

        // PromoteNewDrawnPolygonToAPart
        var newPolyID = cardinalPointsOfCurrentObject.length - 1;
        var ret = true;
        if (cardinalPointsOfCurrentObject[newPolyID].length <= 2) {
            // not a valid polygon
            cardinalPointsOfCurrentObject.pop();
            ret = false;
        } else {
            var partPolys = new Array();
            partPolys.push(cardinalPointsOfCurrentObject[newPolyID]);
            var newPart = new Part(partPolys, ObjID);
            existingParts.push(newPart);
            me.MergeAndReorganizeAllParts();
        }
        var fin = me.finalize();
        
        svgPolygonDivision = updateSVGPolygonDivision(cardinalPointsOfCurrentObject, ObjID, color);
        return ret;
    }


    this.finalize = function () {
        var ctx = canvas[0].getContext("2d");
        ctx.clearRect(0, 0, canvas[0].width, canvas[0].height);
        drawAllExistingBoundariesFaded();

        //if (cardinalPointsOfCurrentObject[PolygonID].length > 2) {
        me.drawFinal();
        return true;
        //}
        /*else {
            alert("You have to mark atleast 3 points to draw a boundary! Click Start Drawing New Boundary to start over again.");
            return false;
        }*/
    }
    //this.getPolygons = function () {
    //    return polygons;
    //}

    this.getPolygonDivision = function()
    {
        return svgPolygonDivision;
    }

    this.getSVGDivision = function () {
        return svgDivision;
    }

    return me;
}