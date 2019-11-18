function Polygon(category, categoryIndex, cPoints)
{
    var category = category;
    var categoryIndex = categoryIndex;
    var cardinalPoints = cPoints;

}



function BoundaryDrawer(color, cPoints, existingCardinalPoints, cntr)
{
    var canvas = $("#canvas");
    var color = color;
    var colorString = "rgb(" + color + ")";
    var colorFadeString = "rgba(" + color + ",0.5)";  
    var cardinalPoints = cPoints;
    var existingBoundaries = existingCardinalPoints;
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


    this.putPoint = function(xy)
    {
        var pointSize = 4; // Change according to the size of the point.
        var ctx = canvas[0].getContext("2d");
        ctx.fillStyle = colorString; // Red color
        ctx.beginPath(); //Start path
        ctx.arc(xy[0], xy[1], pointSize, 0, Math.PI * 2, true); // Draw a point using the arc function of the canvas with a point structure.
        ctx.fill(); // 
    }

    this.putFadePoint = function (xy) {
        var pointSize = 4; // Change according to the size of the point.
        var ctx = canvas[0].getContext("2d");
        ctx.fillStyle = colorFadeString; // Red color
        ctx.beginPath(); //Start path
        ctx.arc(xy[0], xy[1], pointSize, 0, Math.PI * 2, true); // Draw a point using the arc function of the canvas with a point structure.
        ctx.fill(); // 
        ctx.closePath();
    }

    this.drawLine = function(startxy,endxy)
    {
        var ctx = canvas[0].getContext("2d");
        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.strokeStyle = colorString;
        ctx.moveTo(startxy[0], startxy[1]);
        ctx.lineTo(endxy[0], endxy[1]);
        ctx.stroke();
        ctx.closePath();
    }

    this.drawFadeLine = function (startxy, endxy) {
        var ctx = canvas[0].getContext("2d");
        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.strokeStyle = colorFadeString;
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


    function drawExistingBoundaryFaded(points)
    {
        if (points.length > 0) {
            me.putFadePoint(points[0]);

            for (var i = 1; i < points.length; i++) {
                me.putFadePoint(points[i]);
                me.drawFadeLine(points[i - 1], points[i]);
            }

            if(points.length > 2)
            {
                me.drawFadeLine(points[points.length - 1], points[0]);
            }
        }
    }

    function drawAllExistingBoundariesFaded()
    {
        for(var i=0;i<existingBoundaries.length;i++)
        {
            drawExistingBoundaryFaded(existingBoundaries[i]);
        }
    }

    this.draw = function()
    {
        if (cardinalPoints.length > 0) {
            me.putPoint(cardinalPoints[0]);
        }
        for (var i = 1; i < cardinalPoints.length; i++) {
            me.putPoint(cardinalPoints[i]);
            me.drawLine(cardinalPoints[i-1],cardinalPoints[i]);
        }
    }

    this.drawFinal = function () {
        me.draw();
        if(cardinalPoints.length > 2)
        {
            me.drawLine(cardinalPoints[cardinalPoints.length-1],cardinalPoints[0]);
        }
    }


    this.drawFade = function () {
        if (cardinalPoints.length > 0) {
            me.putFadePoint(cardinalPoints[0]);
        }
        for (var i = 1; i < cardinalPoints.length; i++) {
            me.putFadePoint(cardinalPoints[i]);
            me.drawFadeLine(cardinalPoints[i - 1], cardinalPoints[i]);
        }
    }

    this.drawFadeFinal = function () {
        me.drawFade();
        if (cardinalPoints.length > 2) {
            me.drawFadeLine(cardinalPoints[cardinalPoints.length - 1], cardinalPoints[0]);
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

    this.getCardinalPoints = function()
    {
        if(cardinalPoints.length < 3)
        {
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

    function createSVGPolygonDivision()
    {
        var minx = 100000;
        var maxx = 0;
        var miny = 100000;
        var maxy = 0;

        for (var i = 0; i < cardinalPoints.length; i++)
        {
            if(cardinalPoints[i][0] > maxx)
            {
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

        var divString = "<svg width= " + width + " height= " + height + " style='position:absolute'>";
        var pointString = "'";
        for (var i = 0; i < cardinalPoints.length; i++)
        {
            pointString += cardinalPoints[i][0] + "," + cardinalPoints[i][1] + " ";
        }
        pointString += "'";

        var id = "polygon_" + counter;
        divString += "<polygon id='" + id + "' points=" + pointString + " style='fill:rgba(255,0,0,0.3)'>";
        divString += "</svg>";
        var div = $(divString).insertBefore($("#TheImage"));
        div[0].style.zIndex = 0;
        svgDivision = div;
        return $("#"+id);
    }

    this.finish = function()
    {
        var fin = me.finalize();
        if (fin) {
            svgPolygonDivision = createSVGPolygonDivision();
        }
        canvas.unbind('click');
    }

    

    this.getPolygonDivision = function()
    {
        return svgPolygonDivision;
    }

    this.getSVGDivision = function () {
        return svgDivision;
    }

    return me;
}