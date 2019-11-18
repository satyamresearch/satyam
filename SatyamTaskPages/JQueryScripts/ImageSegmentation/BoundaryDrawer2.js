colors = [["256,0,0"], ["0,256,0"], ["0,0,256"], ["256,0,256"], ["256,256,0"], ["0,256,256"], ["0,0,0"], ["256,256,256"],
["128,0,0"], ["0,128,0"], ["0,0,128"], ["128,0,128"], ["128,128,0"], ["0,128,128"], ["0,0,0"], ["128,128,128"]];



function BoundaryDrawer(color, categoryIndex, existingCardinalBoundaryPoints, existingCategoryIndexes, cntr, polycntr, displayWidth, displayHeight, png)
{
    var canvas = $("#canvas");
    var color = color;
    var categoryIndex = categoryIndex;
    var colorString = "rgb(" + color + ")";
    var colorFadeString = "rgba(" + color + ",0.5)";
    var cardinalPoints = existingCardinalBoundaryPoints[cntr]; // should be a list of polygons now
    var existingCardinalBoundaryPoints = existingCardinalBoundaryPoints;  // a list of list of polygons
    var existingCategoryIndexes = existingCategoryIndexes;
    var svgPolygonDivision = null;
    var svgDivision = null;
    var ObjID = cntr;
    var PolygonID = polycntr;
    var width = displayWidth;
    var height = displayHeight;
    var PNG = png;
    

    this.init = function()
    {
        if (PolygonID == cardinalPoints.length) {
            var currentPolygon = new Array();
            cardinalPoints.push(currentPolygon);
        } 
        canvas[0].style.zIndex = 100;
        canvas.bind('click', onclickDrawing);
        me.clearAndRedraw();
    }

    var me = this;

    this.getObjID = function() {
        return ObjID;
    }
    function onclickDrawing(e)
    {
        var rect = canvas[0].getBoundingClientRect();
        var x = Math.floor(event.clientX - rect.left);
        var y = Math.floor(event.clientY - rect.top);
        var xy = new Array();
        xy.push(x);
        xy.push(y);
        cardinalPoints[PolygonID].push(xy);
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
        for (var i = 0; i < existingCardinalBoundaryPoints.length-1;i++)
        {
            // the last one is current
            for (var j = 0; j < existingCardinalBoundaryPoints[i].length; j++) {
                drawExistingBoundaryFaded(existingCardinalBoundaryPoints[i], existingCategoryIndexes[i]);
            }
        }
    }

    this.draw = function()
    {
        if (cardinalPoints[PolygonID].length > 0) {
            me.putPoint(cardinalPoints[PolygonID][0], color);
        }
        for (var i = 1; i < cardinalPoints[PolygonID].length; i++) {
            me.putPoint(cardinalPoints[PolygonID][i], color);
            me.drawLine(cardinalPoints[PolygonID][i - 1], cardinalPoints[PolygonID][i], color);
        }
    }

    this.drawFinal = function () {
        me.draw();
        if (cardinalPoints[PolygonID].length > 2)
        {
            me.drawLine(cardinalPoints[PolygonID][cardinalPoints[PolygonID].length - 1], cardinalPoints[PolygonID][0], color);
        }
    }


    this.drawFade = function () {
        if (cardinalPoints[PolygonID].length > 0) {
            me.putFadePoint(cardinalPoints[PolygonID][0], color);
        }
        for (var i = 1; i < cardinalPoints[PolygonID].length; i++) {
            me.putFadePoint(cardinalPoints[PolygonID][i],color);
            me.drawFadeLine(cardinalPoints[PolygonID][i - 1], cardinalPoints[PolygonID][i], color);
        }
    }

    this.drawFadeFinal = function () {
        me.drawFade();
        if (cardinalPoints[PolygonID].length > 2) {
            me.drawFadeLine(cardinalPoints[PolygonID][cardinalPoints[PolygonID].length - 1], cardinalPoints[PolygonID][0], color);
        }
    }



    this.finalize = function()
    {
        var ctx = canvas[0].getContext("2d");
        ctx.clearRect(0, 0, canvas[0].width, canvas[0].height);
        drawAllExistingBoundariesFaded();

        if (cardinalPoints[PolygonID].length > 2) {
            me.drawFinal();
            return true;
        }
        /*else {
            alert("You have to mark atleast 3 points to draw a boundary! Click Start Drawing New Boundary to start over again.");
            return false;
        }*/
    }

    this.undo = function()
    {
        if (cardinalPoints[PolygonID].length == 0)
        {
            return;
        }
        cardinalPoints[PolygonID].pop();
        me.clearAndRedraw();
    }

    this.getCardinalPoints = function () {
        var results = new Array();
        for (var j = 0; j < cardinalPoints.length; j++) {
            if (cardinalPoints[j].length < 3) {
                results.push(null);
            }
            var res = new Array();
            for (var i = 0; i < cardinalPoints[j].length; i++) {
                var xy = new Array();
                xy.push(cardinalPoints[j][i][0]);
                xy.push(cardinalPoints[j][i][1]);
                res.push(xy);
            }
            results.push(res);
        }
        
        return results;
    }

    this.getColor = function(){
        return color;
    }

    this.getPNG = function () {
        return PNG;
    }


    function updateAndDrawPNG() {
        var poly = new Polygon(cardinalPoints[PolygonID]);
        for (var i = 0; i < displayWidth; i++) {
            for (var j = 0; j < displayHeight; j++){
                var p = new Array();
                p.push(i);
                p.push(j);
                if (poly.Interior(p)) {
                    PNG[j * displayWidth + i] = categoryIndex;
                }

                if (PNG[j * displayWidth + i] != 0) {
                    var c = PNG[j * displayWidth + i];
                    putFadePoint(p, c);
                }
            }
        }
    }
    
    
    function createSVGPolygonDivision() {

        var minx = 100000;
        var maxx = 0;
        var miny = 100000;
        var maxy = 0;
        var id = "obj_" + ObjID;

        for (var j = 0; j < cardinalPoints.length; j++) {
            for (var i = 0; i < cardinalPoints[j].length; i++) {
                if (cardinalPoints[j][i][0] > maxx) {
                    maxx = cardinalPoints[j][i][0];
                }
                if (cardinalPoints[j][i][1] > maxy) {
                    maxy = cardinalPoints[j][i][1];
                }
                if (cardinalPoints[j][i][0] < minx) {
                    minx = cardinalPoints[j][i][0];
                }
                if (cardinalPoints[j][i][1] < miny) {
                    miny = cardinalPoints[j][i][1];
                }
            }
        }
        

        var width = maxx - minx;
        var height = maxy - miny;

        // remove previous svg of this obj, if any
        var element = document.getElementById(id);
        if (element != null) {
            element.parentNode.removeChild(element);
        }
        


        //var divString = "<svg viewBox = '" + minx + " " + miny + " " + maxx + " " + maxy + "' width= " + width + " height= " + height + " style='position:absolute'>";
        //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute'>";
        //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block'>";
        //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block;padding:" + miny + "px " + minx + "px'>";
        var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block;margin-left:" + minx + "px;margin-top:" + miny + "px'>";

        // polygon pointstring
        //var pointString = "'";
        //for (var i = 0; i < cardinalPoints.length; i++) {
        //    var x = cardinalPoints[i][0] - minx;
        //    var y = cardinalPoints[i][1] - miny;
        //    pointString += x + "," + y + " ";
        //}
        //pointString += "'";
        //divString += "<polygon id='" + id + "' points=" + pointString + " style='fill:rgba(" + color + ", 0.3)'>";

        var pointString = "'";
        for (var j = 0; j < cardinalPoints.length; j++) {
            if (cardinalPoints[j].length > 0) {
                var x0 = cardinalPoints[j][0][0] - minx;
                var y0 = cardinalPoints[j][0][1] - miny;
                pointString += "M" + x0 + ',' + y0 + " "; // move to
                for (var i = 1; i < cardinalPoints[j].length; i++) {
                    var x = cardinalPoints[j][i][0] - minx;
                    var y = cardinalPoints[j][i][1] - miny;
                    pointString += "L" + x + "," + y + " "; // line to
                }
                // close path
                pointString += "Z ";
            }
        }
        
        
        pointString += "'";
        var transparency = (ObjID % 5 + 1) / 5;
        divString += "<path id='" + id + "' fill-rule='evenodd' d=" + pointString + " style='fill:rgba(" + color + ", "+ transparency +")'>";

        
        divString += "</svg>";
        var div = $(divString).insertBefore($("#TheImage"));
        div[0].style.zIndex = 0;
        svgDivision = div;
        return $("#" + id);
    }

    //this.MergeAndReorganizeAllParts(){

    //    // add fully contained object's contour to the bounding object, so that the bounding object can delete that part inside.
    //    if (ObjCounter > 0) {
    //        var newObj = new Part(existingCardinalBoundaryPoints[ObjCounter], ObjCounter);
    //        for (var i = 0; i < existingCardinalBoundaryPoints.length - 1; i++) {
    //            var oldObj = new Part(existingCardinalBoundaryPoints[i], i);
    //            if (newObj.IsFullyContained(oldObj)) {
    //                var newObjPolys = newObj.getPolys();
    //                for (var k = 0; k < newObjPolys.length; k++) {
    //                    existingCardinalBoundaryPoints[i].push(newObjPolys[k]);
    //                }
    //                break;// there should be only one fully containED relationship
    //            }
    //            // we handle the case of oldObj contained by newObj when the newobj is drawn
    //        }
    //    }


    //    // check if the new object is containing any other object, if so deduct those object.
    //    if (ObjCounter > 0) {
    //        var newObj = new Part(existingCardinalBoundaryPoints[ObjCounter]);
    //        // there could be multiple fully containING relationship
    //        // must treat each independently, with respect to the new drawn polygon
    //        var oldObjContained = new Array();
    //        for (var i = 0; i < existingCardinalBoundaryPoints.length - 1; i++) {
    //            var oldObj = new Part(existingCardinalBoundaryPoints[i]);
    //            if (oldObj.IsFullyContained(newObj)) {
    //                oldObjContained.push(oldObj);
    //            }
    //            // we handle the case of newObj contained by oldObj when the newobj is FINISHED
    //        }
    //        for (var i = 0; i < oldObjContained.length; i++) {
    //            var oldObjPolys = oldObjContained[i].getPolys();
    //            for (var k = 0; k < oldObjPolys.length; k++) {
    //                existingCardinalBoundaryPoints[ObjCounter].push(oldObjPolys[k]);
    //            }
    //        }
    //    }
    //}


    this.finish = function()
    {
        var fin = me.finalize();

        //MergeAndReorganizeAllParts();

        if (fin) {
            svgPolygonDivision = createSVGPolygonDivision();
            //updateAndDrawPNG();
        }
        canvas.unbind('click');
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