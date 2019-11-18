

/*function createLineElement(x, y, length, angle) {
    var line = document.createElement("div");
    var styles = 'border: 3px dashed red; '
               + 'width: ' + length + 'px; '
               + 'height: 0px; '
               + '-moz-transform: rotate(' + angle + 'rad); '
               + '-webkit-transform: rotate(' + angle + 'rad); '
               + '-o-transform: rotate(' + angle + 'rad); '
               + '-ms-transform: rotate(' + angle + 'rad); '
               //+ 'position: absolute; '
               + 'position: relative; '
               + 'top: ' + y + 'px; '
               + 'left: ' + x + 'px; ';
    line.setAttribute('style', styles);
    return line;
}*/

function createLineElement(x, y, length, angle, color) {
    var line = document.createElement("div");
    var styles = 'border: 3px dashed ' + color + ';'
               + 'width: ' + length + 'px; '
               + 'height: 0px; '
               + '-moz-transform: rotate(' + angle + 'rad); '
               + '-webkit-transform: rotate(' + angle + 'rad); '
               + '-o-transform: rotate(' + angle + 'rad); '
               + '-ms-transform: rotate(' + angle + 'rad); '
               + 'position: absolute; '
               //+ 'position: relative; ' 
               // 'position: fixed; '
               + 'top: ' + y + 'px; '
               + 'left: ' + x + 'px; ';
    line.setAttribute('style', styles);
    return line;
}

function createLine(x1, y1, x2, y2,color) {
    var a = x1 - x2,
        b = y1 - y2,
        c = Math.sqrt(a * a + b * b);

    var sx = (x1 + x2) / 2,
        sy = (y1 + y2) / 2;

    var x = sx - c / 2,
        y = sy;

    var alpha = Math.PI - Math.atan2(-b, a);

    return createLineElement(x, y, c, alpha,color);
}


function drawLine(handle, x1, y1, x2, y2)
{
    handle.appendChild(createLine(305, 220, 1020, 220));
}

function drawLines(handle,lineArray,color)
{
    pos = getElementPosition(handle);

    for(var i=0;i<lineArray.length;i++)
    {
        var line = lineArray[i];
        //handle.appendChild(createLine(line[0] + pos.x, line[1] + pos.y, line[2] + pos.x, line[3] + pos.y, color));
        handle.appendChild(createLine(line[0], line[1] , line[2], line[3], color));

    }
}

