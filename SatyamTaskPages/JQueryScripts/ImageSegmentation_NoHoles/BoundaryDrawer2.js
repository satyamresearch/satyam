//function Polygon(id, cPoints, color)
//{
//    var id = id;
//    //var category = category;
//    //var categoryIndex = categoryIndex;
//    var cardinalPoints = cPoints;
//    var color = color;



//    this.getCardinalPoints = function () {
//        if (cardinalPoints.length < 3) {
//            return null;
//        }
//        var results = new Array();
//        for (var i = 0; i < cardinalPoints.length; i++) {
//            var xy = new Array();
//            xy.push(cardinalPoints[i][0]);
//            xy.push(cardinalPoints[i][1]);
//            results.push(xy);
//        }
//        return results;
//    }
//}

colors = [["256,0,0"], ["0,256,0"], ["0,0,256"], ["256,0,256"], ["256,256,0"], ["0,256,256"], ["0,0,0"], ["256,256,256"],
["128,0,0"], ["0,128,0"], ["0,0,128"], ["128,0,128"], ["128,128,0"], ["0,128,128"], ["0,0,0"], ["128,128,128"]];



function BoundaryDrawer(color, categoryIndex, cPoints, existingCardinalBoundaryPoints, existingCategoryIndexes, cntr)
{
    var canvas = $("#canvas");
    var color = color;
    var categoryIndex = categoryIndex;
    var colorString = "rgb(" + color + ")";
    var colorFadeString = "rgba(" + color + ",0.5)";  
    var cardinalPoints = cPoints;
    var existingCardinalBoundaryPoints = existingCardinalBoundaryPoints;
    var existingCategoryIndexes = existingCategoryIndexes;
    var svgPolygonDivision = null;
    var svgDivision = null;
    var counter = cntr;

    this.init = function()
    {
        canvas[0].style.zIndex = 100;
        canvas.bind('click', onclickDrawing);
        me.clearAndRedraw();
    }

    var me = this;


    function onclickDrawing(e)
    {
        var rect = canvas[0].getBoundingClientRect();
        var x = Math.floor(event.clientX - rect.left);
        var y = Math.floor(event.clientY - rect.top);
        var xy = new Array();
        xy.push(x);
        xy.push(y);
        cardinalPoints.push(xy);
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
        //var points = polygon.cardinalPoints;
        if (points.length > 0) {
            me.putFadePoint(points[0], colors[colorIndexes]);

            for (var i = 1; i < points.length; i++) {
                me.putFadePoint(points[i], colors[colorIndexes]);
                me.drawFadeLine(points[i - 1], points[i], colors[colorIndexes]);
            }

            if(points.length > 2)
            {
                me.drawFadeLine(points[points.length - 1], points[0], colors[colorIndexes]);
            }
        }
    }

    function drawAllExistingBoundariesFaded()
    {
        for (var i = 0; i < existingCardinalBoundaryPoints.length;i++)
        {
            drawExistingBoundaryFaded(existingCardinalBoundaryPoints[i], existingCategoryIndexes[i]);
        }
    }

    this.draw = function()
    {
        if (cardinalPoints.length > 0) {
            me.putPoint(cardinalPoints[0], color);
        }
        for (var i = 1; i < cardinalPoints.length; i++) {
            me.putPoint(cardinalPoints[i], color);
            me.drawLine(cardinalPoints[i - 1], cardinalPoints[i], color);
        }
    }

    this.drawFinal = function () {
        me.draw();
        if(cardinalPoints.length > 2)
        {
            me.drawLine(cardinalPoints[cardinalPoints.length-1],cardinalPoints[0], color);
        }
    }


    this.drawFade = function () {
        if (cardinalPoints.length > 0) {
            me.putFadePoint(cardinalPoints[0], color);
        }
        for (var i = 1; i < cardinalPoints.length; i++) {
            me.putFadePoint(cardinalPoints[i],color);
            me.drawFadeLine(cardinalPoints[i - 1], cardinalPoints[i], color);
        }
    }

    this.drawFadeFinal = function () {
        me.drawFade();
        if (cardinalPoints.length > 2) {
            me.drawFadeLine(cardinalPoints[cardinalPoints.length - 1], cardinalPoints[0], color);
        }
    }



    this.finalize = function()
    {
        var ctx = canvas[0].getContext("2d");
        ctx.clearRect(0, 0, canvas[0].width, canvas[0].height);
        drawAllExistingBoundariesFaded();

        if (cardinalPoints.length > 2) {
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
        if (cardinalPoints.length == 0)
        {
            return;
        }
        cardinalPoints.pop();
        me.clearAndRedraw();
    }

    this.getCardinalPoints = function () {
        if (cardinalPoints.length < 3) {
            return null;
        }
        var results = new Array();
        for (var i = 0; i < cardinalPoints.length; i++) {
            var xy = new Array();
            xy.push(cardinalPoints[i][0]);
            xy.push(cardinalPoints[i][1]);
            results.push(xy);
        }
        return results;
    }

    this.getColor = function(){
        return color;
    }


    
    function createSVGPolygonDivision() {

        var minx = 100000;
        var maxx = 0;
        var miny = 100000;
        var maxy = 0;
        var id = "polygon_" + counter;


        for (var i = 0; i < cardinalPoints.length; i++) {
            if (cardinalPoints[i][0] > maxx) {
                maxx = cardinalPoints[i][0];
            }
            if (cardinalPoints[i][1] > maxy) {
                maxy = cardinalPoints[i][1];
            }
            if (cardinalPoints[i][0] < minx) {
                minx = cardinalPoints[i][0];
            }
            if (cardinalPoints[i][1] < miny) {
                miny = cardinalPoints[i][1];
            }
        }

        var width = maxx - minx;
        var height = maxy - miny;

        //var divString = "<svg viewBox = '" + minx + " " + miny + " " + maxx + " " + maxy + "' width= " + width + " height= " + height + " style='position:absolute'>";
        //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute'>";
        //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block'>";
        //var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block;padding:" + miny + "px " + minx + "px'>";
        var divString = "<svg width= " + width + " height= " + height + " style='fill-rule:evenodd;position:absolute;display:inline-block;margin-left:" + minx + "px;margin-top:" + miny + "px'>";
        var pointString = "'";
        //var pointString = "'[";
        for (var i = 0; i < cardinalPoints.length; i++) {
            var x = cardinalPoints[i][0] - minx;
            var y = cardinalPoints[i][1] - miny;
            pointString += x + "," + y + " ";
        }
        pointString += "'";
        //pointString += "'][100,100 100,200 200,200 100,200]";


        divString += "<polygon id='" + id + "' points=" + pointString + " style='fill-rule:evenodd;fill:rgba(" + color + ", 0.3)'>";
        divString += "</svg>";
        var div = $(divString).insertBefore($("#TheImage"));
        div[0].style.zIndex = 0;
        svgDivision = div;
        return $("#" + id);
    }

    this.finish = function()
    {
        var fin = me.finalize();
        if (fin) {
            svgPolygonDivision = createSVGPolygonDivision();
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