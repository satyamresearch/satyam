function ObjectInstance(_parts, _objid, _category, _color) {
    var parts = _parts;
    var ObjID = _objid;
    var category = _category;
    var color = _color;

    this.getSVGDivisionString = function(){
        var minx = 100000;
        var maxx = 0;
        var miny = 100000;
        var maxy = 0;
        var id = "obj_" + ObjID;

        for (var j = 0; j < parts.length; j++) {
            for (var i = 0; i < parts[j].length; i++) {
                if (parts[j][i][0] > maxx) {
                    maxx = parts[j][i][0];
                }
                if (parts[j][i][1] > maxy) {
                    maxy = parts[j][i][1];
                }
                if (parts[j][i][0] < minx) {
                    minx = parts[j][i][0];
                }
                if (parts[j][i][1] < miny) {
                    miny = parts[j][i][1];
                }
            }
        }
        var width = maxx - minx;
        var height = maxy - miny;

        var divString = "<svg width= " + width + " height= " + height + " style='position:absolute;display:inline-block;margin-left:" + minx + "px;margin-top:" + miny + "px'>";
        
        var pointString = "'";
        for (var j = 0; j < parts.length; j++) {
            if (parts[j].length > 0) {
                var x0 = parts[j][0][0] - minx;
                var y0 = parts[j][0][1] - miny;
                pointString += "M" + x0 + ',' + y0 + " "; // move to
                for (var i = 1; i < parts[j].length; i++) {
                    var x = parts[j][i][0] - minx;
                    var y = parts[j][i][1] - miny;
                    pointString += "L" + x + "," + y + " "; // line to
                }
                // close path
                pointString += "Z ";
            }
        }
        pointString += "'";
        //var transparency = (ObjID % 5 + 1) / 5;
        var transparency = 0.5;
        divString += "<path id='" + id + "' fill-rule='evenodd' d=" + pointString + " style='fill:rgba(" + color + ", " + transparency + ")'>";

        divString += "</svg>";
        return divString;
    }
}


function Part(segmentpoints, objid) {
    var polys = segmentpoints; // list of (list of points)
    var ObjID = objid;

    this.Interior = function (xy) {

        if (this.OnCardinalPoint(xy)) {
            return true; // [] closed interval
        }
        else {
            
        }

        var count = 0;
        for (var i = 0; i < polys.length; i++) {
            // for each polygon, count even odd
            var poly = new Polygon(polys[i]);
            if (poly.Interior(xy)) {
                count = count + 1;
            }
        }
        return count % 2 == 1;
    }

    this.OnCardinalPoint = function (xy) {
        for (var i = 0; i < polys.length; i++) {
            var poly = new Polygon(polys[i]);
            if (poly.OnCardinalPoint(xy)) {
                return true;
            } else {

            }
        }
        return false;
    }


    this.IsFullyContained = function (seg) {
        for (var i = 0; i < polys.length; i++) {
            for (var j = 0; j < polys[i].length; j++) {
                var xy = polys[i][j];
                if (!seg.Interior(xy)) {
                    return false;
                }
            }
        }
        return true;
    }

    this.getPolys = function () {
        return polys;
    }

    this.getObjID = function () {
        return ObjID;
    }


    this.insertPoly = function (points) {
        var sameIndex = -1;
        for (var i = 0; i < polys.length; i++) {
            var same = true;
            for (var j = 0; j < polys[i].length; j++) {
                if (polys[i][j][0] != points[j][0] && polys[i][j][1] != points[j][1]) {
                    same = false;
                    break;
                }
            }
            if (same) {
                sameIndex = i;
                break;
            }
        }

        if (sameIndex != -1) {
            polys.splice(sameIndex, 1);
            return false;
        } else {
            polys.push(points);
            return true;
        }
    }
}


function Polygon(points) {
    var cardinalPoints = points; // list of points
    this.Interior = function (xy)
    {
        if (this.OnCardinalPoint(xy)) {
            return true; // [] closed interval
        }
        var count = 0;
        for (var i = 0; i < cardinalPoints.length; i++) {


            var x1 = cardinalPoints[i][0];
            var y1 = cardinalPoints[i][1];
            var j = (i + 1) % cardinalPoints.length;
            var x2 = cardinalPoints[j][0];
            var y2 = cardinalPoints[j][1];

            var IsIntersected= this.intersection(0, 0, xy[0], xy[1], x1, y1, x2, y2);
            if (IsIntersected) {
                count = count + 1;
            }
        }
        return count % 2 == 1;
    }

    this.OnCardinalPoint = function (xy) {
        for (var i = 0; i < cardinalPoints.length; i++) {
            if (cardinalPoints[i][0] == xy[0] && cardinalPoints[i][1] == xy[1]) {
                return true;
            } else {
                
            }
        }
               
        return false;
    }
    

    this.intersection = function(x11, y11, x12, y12, x21, y21, x22, y22){
        var ret = "intersect";
        lambdas = new Array(2);
        if (x22 - x21 == 0 && x12 - x11 != 0 && y22 - y21 != 0) {
            lambdas[0] = (x21 - x11) / (x12 - x11);
            lambdas[1] = (lambdas[0] * (y12 - y11) - (y21 - y11)) / (y22 - y21);
        }
        else if (x22 - x21 == 0 && (x12 - x11 == 0 || y22 - y21 == 0)) {
            ret = "nointersect";
        }
        else if (y22 - y21 == 0 && y12 - y11 != 0 && x22 - x21 != 0) {
            lambdas[0] = (y21 - y11) / (y12 - y11);
            lambdas[1] = (lambdas[0] * (x12 - x11) - (x21 - x11)) / (x22 - x21);
        }
        else if (y22 - y21 == 0 && (y12 - y11 == 0 || x22 - x21 == 0)) {
            ret = "nointersect";
        }
        else if (x12 - x11 == 0 && x22 - x21 != 0 && y12 - y11 != 0) {
            lambdas[1] = -1.0 * (x21 - x11) / (x22 - x21);
            lambdas[0] = ((y21 - y11) + lambdas[1] * (y22 - y21)) / (y12 - y11);
        }
        else if (x12 - x11 == 0 && (x22 - x21 == 0 || y12 - y11 == 0)) {
            ret = "nointersect";
        }
        else if (y12 - y11 == 0 && y22 - y21 != 0 && x12 - x11 != 0) {
            lambdas[1] = -1.0 * (y21 - y11) / (y22 - y21);
            lambdas[0] = ((x21 - x11) + lambdas[1] * (x22 - x21)) / (x12 - x11);
        }
        else if (y12 - y11 == 0 && (y22 - y21 == 0 || x12 - x11 == 0)) {
            ret = "nointersect";
        }
        else if (((x22 - x21) / (x12 - x11)) - ((y22 - y21) / (y12 - y11)) == 0 && x12 - x11 != 0 && y12 - y11 != 0) //identical slopes
        {
            if ((x21 - x11) / (x12 - x11) == (y21 - y11) / (y12 - y11)) {
                ret = "overlap";
            }
            else {
                ret = "nointersect";
            }
        }
        else {
            lambdas[1] = (((y21 - y11) / (y12 - y11)) - ((x21 - x11) / (x12 - x11))) / (((x22 - x21) / (x12 - x11)) - ((y22 - y21) / (y12 - y11)));
            lambdas[0] = (((x21 - x11) / (x22 - x21)) - ((y21 - y11) / (y22 - y21))) / (((x12 - x11) / (x22 - x21)) - ((y12 - y11) / (y22 - y21)));
        }


        if (ret == "intersect" && lambdas[0] >= 0 && lambdas[0] <= 1 && lambdas[1] >= 0 && lambdas[1] <= 1) {
            return true;
        } else {
            return false;
        }

    }


    this.IsFullyContained = function(poly){
        for (var i = 0; i < cardinalPoints.length; i++) {
            var xy = cardinalPoints[i];
            if (!poly.Interior(xy)) {
                return false;
            }
        }
        return true;
    }
}