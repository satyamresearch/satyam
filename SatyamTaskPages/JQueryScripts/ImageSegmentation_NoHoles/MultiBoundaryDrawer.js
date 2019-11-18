function BoundaryDrawer(canvas,colors,cPoints)
{
    var canvas = $("#canvas");
    var color = color;
    var colorString = "rgb(" + color + ")";
    var colorFadeString = "rgb(" + color + ",0.3)";  
    var cardinalPoints = cPoints;
    var points = new Array();

    this.init = function()
    {
        points = new Array();
        if(cardinalPoints.length == 0)
        {
            return;
        }
        if(cardinalPoints.length==1)
        {
            var xy = new Array();
            xy.push(cardinalPoints[0][0]);
            xy.push(cardinalPoints[0][1]);
            points.push(xy);
        }
        else{
            for(var i=0;i<cardinalPoints.length;i++)
            {
                me.join(cardinalPoints[i - 1], cardinalPoints[i]);
            }
            var xy = new Array()
            xy.push(cardinalPoints[cardinalPoints.length-1][0]);
            xy.push(cardinalPoints[cardinalPoints.length-1][1]);
            points.push(xy);
        }
    }

    var me = this;

    me.canvas.click(function (e) {
        var rect = canvas[0].getBoundingClientRect();
        var x = Math.floor(event.clientX - rect.left);
        var y = Math.floor(event.clientY - rect.top);
        var xy = new Array();
        xy.push(x);
        xy.push(y);
        cardinalPoints.push(xy);
        me.extendPoints(xy);
        me.draw(x,y);
    });

    this.extendPoints = function(xy)
    {
        if (points.length > 0) {
            me.join(points[points.length - 1], xy);
        }
        else {
            var xyn = new Array();
            xyn.push(xy[0]);
            xyn.push(xy[1]);
            points.push(xyn);
        }
    }

    this.putPoint = function(xy)
    {
        var pointSize = 3; // Change according to the size of the point.
        var ctx = canvas[0].getContext("2d");
        ctx.fillStyle = colorString; // Red color
        ctx.beginPath(); //Start path
        ctx.arc(xy[0], xy[1], pointSize, 0, Math.PI * 2, true); // Draw a point using the arc function of the canvas with a point structure.
        ctx.fill(); // 
    }

    this.putPointFade = function (xy) {
        var pointSize = 3; // Change according to the size of the point.
        var ctx = canvas[0].getContext("2d");
        ctx.fillStyle = colorFadeString; // Red color
        ctx.beginPath(); //Start path
        ctx.arc(xy[0], xy[1], pointSize, 0, Math.PI * 2, true); // Draw a point using the arc function of the canvas with a point structure.
        ctx.fill(); // 
        ctx.closePath();
    }


    this.clearAndRedraw = function(xy)
    {
        var ctx = canvas[0].getContext("2d");
        ctx.clearRect(0, 0, canvas[0].width, canvas[0].height);
        for(var i=0;i<points.length;i++)
        {
            me.putPoint(points[i]);
        }
    }

    this.draw = function(xy)
    {
        for (var i = 0; i < points.length; i++) {
            me.putPoint(points[i]);
        }
    }

    this.drawFade = function (xy) {
        for (var i = 0; i < points.length; i++) {
            me.putPointFade(points[i]);
        }
    }


    this.join = function(startxy,finalxy)
    {
        var slope = 0;
        var vertical = false;
        if (finalxy[1] != startxy[1]) {
            slope = (finalxy[0] - startxy[0]) / (finalxy[0] - startxy[0])
        }
        else {
            r
            vertical = true;
        }

        var l = Math.sqrt((finalxy[0] - startxy[0]) * (finalxy[0] - startxy[0]) + (finalxy[1] - startxy[1]) * (finalxy[1] - startxy[1]));
        var dr = 1 / (2 * l);
        for (var r = 0; r < 1 ; r += dr) {
            var newx = Math.floor(startxy[0] + r * (finalxy[0] - startxy[0]));
            var newy = null;
            if (vertical) {
                newy = startxy[1];
            }
            else {
                newy = Math.floor(startxy[1] + r * (finalxy[1] - startxy[1]));
            }
            if (!(newx == points[points.length - 1][0] && newy == points[points.length - 1][1])) {
                var xy = new Array();
                xy.push(newx);
                xy.push(newy);
                points.push(xy);
            }
        }
    }

    this.finalize = function()
    {
        if (points.length > 1) {
            cardinalPoints.push(points[0]); //the final point is same as the first point
            me.extendPoints(points[0]);
        }
        else {
            alert("You cannot finalize since you have to mark atleast 2 points.");
        }
    }

    this.undo = function()
    {
        if (cardinalPoints.length == 0)
        {
            return;
        }
        if (cardinalPoints.length > 1) {
            var finalxy = cardinalPoints[cardinalPoints.length - 2];
            while(!(points[points.length-1][0] == finalxy[0] && points[points.length-1][1] == finalxy[1]))
            {
                var xy = points.pop();
            }
        }
        else {
            var xy = points.pop();
        }
        cardinalPoints.pop();
        me.clearAndRedraw();
    }

    this.getCardinalPoints = function()
    {
        var results = new Array();
        for (var i = 0; i < cardinalPoints.length; i++) {
            var xy = new Array();
            xy.push(cardinalPoints[i][0]);
            xy.push(cardinalPoints[i][1]);
            results.push(xy);
        }
        return results;
    }

    this.finish = function()
    {
        me.finalize();
        me.clearAndRedraw();
    }

    return me;
}