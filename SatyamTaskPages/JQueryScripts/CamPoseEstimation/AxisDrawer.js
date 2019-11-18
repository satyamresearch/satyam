colors = [["256,0,0"], ["0,256,0"], ["256,0,0"], ["256,0,256"], ["256,256,0"], ["0,256,256"], ["0,0,0"], ["256,256,256"],
["128,0,0"], ["0,128,0"], ["0,0,128"], ["128,0,128"], ["128,128,0"], ["0,128,128"], ["128,128,128"],
["192,0,0"], ["0,192,0"], ["0,0,192"], ["192,0,192"], ["192,192,0"], ["0,192,192"], ["192,192,192"]];



function AxisDrawer(_color, axis_type, existing_car_axis_points, existing_x_axis_points, existing_y_axis_points, existing_z_axis_points, displayWidth, displayHeight, png) {
    var canvas = $("#canvas");
    var color = _color;
    var axis_type = axis_type;

    var colorString = "rgb(" + color + ")";
    var colorFadeString = "rgba(" + color + ",0.5)";
    var currentPolygonCardinalPoints = new Array();
    var existing_car_axis_points = existing_car_axis_points;
    var existing_x_axis_points = existing_x_axis_points;  // a list of parts, which is a list of polygons
    var existing_y_axis_points = existing_y_axis_points;
    var existing_z_axis_points = existing_z_axis_points;
    var width = displayWidth;
    var height = displayHeight;
    var PNG = png;
    var me = this;

    this.init = function () {
        canvas[0].style.zIndex = 100;
        canvas.bind('click', onclickDrawing);
        me.clearAndRedraw();
    }

    function onclickDrawing(e) {
        var rect = canvas[0].getBoundingClientRect();
        var x = Math.floor(event.clientX - rect.left);
        var y = Math.floor(event.clientY - rect.top);
        var xy = new Array();
        xy.push(x);
        xy.push(y);
        currentPolygonCardinalPoints.push(xy);
        me.clearAndRedraw();
    }

    this.clearAndRedraw = function (xy) {
        var ctx = canvas[0].getContext("2d");
        ctx.clearRect(0, 0, canvas[0].width, canvas[0].height);
        me.drawAllExistingBoundariesFaded();
        me.draw();
    }
    this.drawAllExistingBoundariesFaded = function () {
        if (existing_car_axis_points.length > 0) {
            me.putPoint(existing_car_axis_points[0], colors[0]);
        }
        for (var i = 1; i < existing_car_axis_points.length; i++) {
            me.putPoint(existing_car_axis_points[i], colors[i + 1]);
            me.drawLine(existing_car_axis_points[0], existing_car_axis_points[i], colors[i + 1]);

        }
        if (existing_x_axis_points.length > 0 && axis_type != 2) {
            me.putPoint(existing_x_axis_points[0], colors[2]);
            for (var i = 1; i < existing_x_axis_points.length; i++) {
                me.putPoint(existing_x_axis_points[i], colors[2]);
                if (i % 2 == 1) {
                    me.drawLine(existing_x_axis_points[i - 1], existing_x_axis_points[i], colors[2]);
                }
            }
        }
        if (existing_y_axis_points.length > 0 && axis_type != 3) {
            me.putPoint(existing_y_axis_points[0], colors[3]);
            for (var i = 1; i < existing_y_axis_points.length; i++) {
                me.putPoint(existing_y_axis_points[i], colors[3]);
                if (i % 2 == 1) {
                    me.drawLine(existing_y_axis_points[i - 1], existing_y_axis_points[i], colors[3]);
                }
            }
        }
        if (existing_z_axis_points.length > 0 && axis_type != 4) {
            me.putPoint(existing_z_axis_points[0], colors[4]);
            for (var i = 1; i < existing_z_axis_points.length; i++) {
                me.putPoint(existing_z_axis_points[i], colors[4]);
                if (i % 2 == 1) {
                    me.drawLine(existing_z_axis_points[i - 1], existing_z_axis_points[i], colors[4]);
                }
            }
        }
    }

    this.draw = function () {
        if (currentPolygonCardinalPoints.length > 0) {
            me.putPoint(currentPolygonCardinalPoints[0], color);
        }
        for (var i = 1; i < currentPolygonCardinalPoints.length; i++) {
            me.putPoint(currentPolygonCardinalPoints[i], color);
            if (i % 2 == 1) {
                me.drawLine(currentPolygonCardinalPoints[i - 1], currentPolygonCardinalPoints[i], color);
            }
        }

    }

    this.putPoint = function (xy, color) {
        var pointSize = 4; // Change according to the size of the point.
        var ctx = canvas[0].getContext("2d");
        ctx.fillStyle = "rgb(" + color + ")";
        ctx.beginPath(); //Start path
        ctx.arc(xy[0], xy[1], pointSize, 0, Math.PI * 2, true); // Draw a point using the arc function of the canvas with a point structure.
        ctx.fill(); // 
    }


    this.drawLine = function (startxy, endxy, color) {
        var ctx = canvas[0].getContext("2d");
        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.strokeStyle = "rgb(" + color + ")";
        ctx.moveTo(startxy[0], startxy[1]);
        ctx.lineTo(endxy[0], endxy[1]);
        ctx.stroke();
        ctx.closePath();
    }




    this.undo = function () {
        if (currentPolygonCardinalPoints == 0) {
            return;
        }
        currentPolygonCardinalPoints.pop();
        me.clearAndRedraw();
    }

    this.getCardinalPoints = function () {
        var results = new Array();
        var res = new Array();
        for (var i = 0; i < currentPolygonCardinalPoints.length; i++) {
            var xy = new Array();
            xy.push(currentPolygonCardinalPoints[i][0]);
            xy.push(currentPolygonCardinalPoints[i][1]);
            res.push(xy);
        }
        results.push(res);

        return currentPolygonCardinalPoints;
    }


    this.getColor = function () {
        return color;
    }

    this.getPNG = function () {
        return PNG;
    }


    this.finish = function () {
        canvas.unbind('click');
        me.clearAndRedraw();
        var ret = true;
        return ret;
    }



    return me;
}