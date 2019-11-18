/*
 * Allows the user to draw a box on the screen.
 */

function Box(container, image, handle, boxes, color, bounds) {
    this.container = container;
    this.image = image;
    this.handle = handle;
    //this.handle[0].style.opacity = 0.5;
    this.boxes=boxes;
    this.editable = false;
    var me = this;
    this.deleteIcon = $('<div> <img src="Images/cross.jpg" width="10px" height="10px"> </div>').appendTo(this.handle);
    this.category = category;
    this.categoryIndex = categoryIndex;
    this.color = color;
    this.categoryDisplay = $('<div style="color:#FFFFFF;background-color:' + this.color + ';opacity:0.7;text-align:center;font-weight:bold;font-size:0.5em">' + this.category + ' </div>').appendTo(this.deleteIcon);
    this.bounds = bounds;

    

    this.allowEdit = function () {
        me.handle.draggable();
        me.handle.resizable({
            handles: "n ,e, s, w, ne, se, sw, nw",
            containment: this.image
        });
        me.deleteIcon.click(function () {
            if (confirm("Are you sure you want to delete this box?")) {
                me.handle.remove();
                me.handle = null;
                for (var i = 0; i < me.boxes.length; i++) {
                    if (me.boxes[i].handle == null) {
                        me.boxes.splice(i, 1);
                    }
                }
            }
        });

        me.handle[0].onmouseover = function () {
            for (var i = 0; i < boxes.length; i++) {
                boxes[i].dehighlight();
            }
            me.fillhighlight();
        }

        me.handle[0].onmouseout = function () {
            me.dehighlight();
        }

    }

    this.dehighlight = function () {
        if (me.handle != null) {
            me.handle[0].style.opacity = 0.5;
            me.handle[0].style.backgroundColor = "transparent";
            me.handle[0].style.borderColor = this.color;
            me.handle[0].style.borderWidth = "3px"
        }
    }

    this.fillhighlight = function () {
        if (me.handle != null) {
            me.handle[0].style.opacity = 0.5;
            me.handle[0].style.backgroundColor = this.color;
            me.handle[0].style.borderColor = "black";
            me.handle[0].style.borderWidth = "3px"
        }
    }

    this.highlight = function () {
        if(me.handle != null){
            me.handle[0].style.opacity = 1;
            me.handle[0].style.backgroundColor = "transparent";
            me.handle[0].style.borderColor = this.color;
            me.handle[0].style.borderWidth = "3px"
        }
    }


    this.disallowEdit = function () {
        this.handle.draggable('destroy');
        this.handle.resizable('destroy');
        this.handle[0].onmouseover = null;
        this.handle[0].onmouseout = null;
    }

    this.handle[0].onmouseup = function (e) {
        updatebounds(e);
    }

    var updatebounds = function (e) {
        var x = parseFloat(me.handle[0].style.left);
        var y = parseFloat(me.handle[0].style.top);
        var width = parseFloat(me.handle[0].style.width);
        var height = parseFloat(me.handle[0].style.height);



        me.bounds[0][0] = x;
        me.bounds[0][1] = y;
        me.bounds[1][0] = x + width;
        me.bounds[1][1] = y + height;
    }

}



function BoxDrawer(container, image, boxes, color,category) {
    var me = this;

    this.onstartdraw = [];
    this.onstopdraw = [];

    this.bounds = new Array();

    this.enabled = false;
    this.drawing = false;

    this.startx = 0;
    this.starty = 0;

    this.container = container;
    this.image = image;
    this.handle = null;
    this.color = color;
    this.category = category;

    this.boxes = boxes;

    this.vcrosshair = $('<div id="vcrosshair" style="width:3px;height:100%;z-index:10;position:absolute;left:inherit;background-color:' + this.color + ';"></div>').insertBefore(image).hide();
    this.hcrosshair = $('<div id="hcrosshair" style="height:3px;width:100%;z-index:10;position:absolute;top:inherit;background-color:' + this.color + ';"></div>').insertBefore(image).hide();




    /*
     * Enables the drawer.
     */
    this.enable = function () {
        this.enabled = true;
        this.container.style.cursor = 'crosshair';
        this.vcrosshair.show();
        this.hcrosshair.show();
    }


    ////Update the boxes
    //this.updateBoxes = function () {
    //    for (var i = 0; i < this.boxes.length; i++) {
    //        if (this.boxes[i].handle == null) {
    //            this.boxes.splice(i, 1);
    //        }
    //    }
    //}

    /*
     * Disables the drawer. No boxes can be drawn and interface cues are
     * disabled.
     */
    this.disable = function () {
        this.vcrosshair.hide();
        this.hcrosshair.hide();
        this.enabled = false;
        this.container.style.cursor = 'default';
    }

    /*
     * Method called when we receive a click on the target area.
     */
    this.click = function (xc, yc) {
        if (this.enabled) {
            if (this.handle == null) {
                this.startdrawing(xc, yc);
            }
            else {
                this.finishdrawing(xc, yc);
            }
        }
    }

    /*
     * Updates the current visualization of the current box.
     */
    this.updatedrawing = function (xc, yc) {
        if (this.drawing) {
            var pos = this.calculateposition(xc, yc);
            this.handle.css({
                "top": pos.ytl + "px",
                "left": pos.xtl + "px",
                "width": (pos.width - 3) + "px",
                "height": (pos.height - 3) + "px",
            });
            this.bounds[0][0] = pos.xtl;
            this.bounds[0][1] = pos.ytl;
            this.bounds[1][0] = pos.xtl + pos.width - 3;
            this.bounds[1][1] = pos.ytl + pos.height - 3;
        }
    }

    /*
     * Updates the cross hairs.
     */
    this.updatecrosshairs = function (visible, xc, yc) {
        if (visible && !this.drawing) {
            this.vcrosshair.show().css('left', xc + 'px');
            this.hcrosshair.show().css('top', yc + 'px');
        }
        else {
            this.vcrosshair.hide();
            this.hcrosshair.hide();
        }
    }

    /*
     * Calculates the position of the box given the starting coordinates and
     * some new coordinates.
     */
    this.calculateposition = function (xc, yc) {
        var xtl = Math.min(xc, this.startx);
        var ytl = Math.min(yc, this.starty);
        var xbr = Math.max(xc, this.startx);
        var ybr = Math.max(yc, this.starty);
        return new Position(xtl, ytl, xbr, ybr)
    }

    /*
     * Starts drawing a box.
     */
    this.startdrawing = function (xc, yc) {
        if (!this.drawing) {
            console.log("Starting new drawing");

            this.bounds = new Array();

            this.startx = xc;
            this.starty = yc;

            var top = new Array();
            top.push(xc);
            top.push(yc);

            var bottom = new Array();
            bottom.push(xc + 3);
            bottom.push(yc + 3);

            this.bounds.push(top);
            this.bounds.push(bottom);

            this.drawing = true;

            this.handle = $('<div class="boundingbox" ><div>').insertBefore(this.image);
            this.handle[0].style.borderColor = this.color;
            this.updatedrawing(xc, yc);
        }
    }

    /*
     * Completes drawing the box. This will remove the visualization, so you will 
     * have to redraw it.
     */
    this.finishdrawing = function (xc, yc) {
        if (this.drawing) {
            console.log("Finishing drawing");
            var position = this.calculateposition(xc, yc);
            this.drawing = false;
            this.startx = 0;
            this.starty = 0;
            var box = new Box(this.container, this.image, this.handle, this.boxes, this.color, this.bounds);
            this.boxes.push(box);
            this.handle[0].style.opacity = 0.5;
            this.handle = null;
        }
    }

    /*
     * Cancels the current drawing.
     */
    this.canceldrawing = function () {
        if (this.drawing) {
            console.log("Cancelling drawing");
            this.drawing = false;
            this.handle.remove();
            this.handle = null;
            this.startx = 0;
            this.starty = 0;
        }
    }

    var respondtoclick = function (e) {
        me.click(e.pageX - container.offsetLeft, e.pageY - container.offsetTop);
    };

    var ignoremouseup = false;


    container.onmousedown = function (e) {
        ignoremouseup = true;
        window.setTimeout(function () {
            ignoremouseup = false;
        }, 500);

        respondtoclick(e);
    };


    container.onmouseup = function (e) {
        if (!ignoremouseup) {
            respondtoclick(e);
        }
        else {
            me.canceldrawing();
        }
    };

    container.click = function (e) {
        e.stopPropagation();
    };

    container.onmousemove = function (e) {
        //        var offset = container.offset();
        var xc = e.pageX - container.offsetLeft;
        var yc = e.pageY - container.offsetTop;

        me.updatedrawing(xc, yc);
        me.updatecrosshairs(true, xc, yc);
    };

    this.clear = function () {
        this.vcrosshair.remove();
        this.hcrosshair.remove();
    };

    $("body").click(function (e) {
        me.canceldrawing();
    });

    return this;
}

function Position(xtl, ytl, xbr, ybr) {
    this.xtl = xtl;
    this.ytl = ytl;
    this.xbr = xbr;
    this.ybr = ybr;
    this.width = xbr - xtl;
    this.height = ybr - ytl;

    if (this.xbr <= this.xtl) {
        this.xbr = this.xtl + 1;
    }

    if (this.ybr <= this.ytl) {
        this.ybr = this.ytl + 1;
    }

    this.serialize = function () {
        return this.xtl + "," +
                this.ytl + "," +
                this.xbr + "," +
                this.ybr + "," +
                this.occluded + "," +
                this.outside;
    }

    this.serialize = function (scalingFactor) {
        var sxtl = Math.floor(this.xtl / scalingFactor);
        var sytl = Math.floor(this.ytl / scalingFactor);
        var sxbr = Math.floor(this.xbr / scalingFactor);
        var sybr = Math.floor(this.ybr / scalingFactor);

        return sxtl + "," +
                sytl + "," +
                sxbr + "," +
                sybr;
    }

    this.clone = function () {
        return new Position(this.xtl,
                            this.ytl,
                            this.xbr,
                            this.ybr)
    }
}
