canvasLabels.prototype = new google.maps.OverlayView();

function idleHandler(canvas) {
    return function () {
        canvas.drawn();
    }
}

function isCanvasContained(canvasbounds, mapbounds) {
    /* minlat, maxlat, minlng, maxlng */

    // canvasbounds inside mapbounds
    if (canvasbounds[1] < mapbounds[0] || canvasbounds[0] > mapbounds[1]) return false;
    if (canvasbounds[3] < mapbounds[2] || canvasbounds[2] > mapbounds[3]) return false;

    return true;

}

function canvasLabels(map, name, style, data) {
    this.canvas_ = null;
    this.hiddencanvas_ = document.createElement("canvas"); // used to draw in the background and then applied to the canvas
    this.colors_ = null;
    this.idleEvent_ = null;
    this.zoomadjust_ = style.adjust;
    this.icon_ = style.icon;
    this.levels_ = style.levels;
    if (style.column != null) {
        this.column = style.column.split('~')[0];
        this.columnValues = parseInt(style.column.split('~')[1]);
    } else {
        this.column = null;
        this.columnValues = 0;
    }

    this.name_ = name;
    this.fillColor = drawing.hexToRgb(style.color1);
    this.borderColor = drawing.hexToRgb(style.color2);
    this.pointSize = style.size;
    this.alphaSize = style.alpha;
    this.dotType = style.type;
    this.selected = [];

    var i = 0, len = data.length,
        minlat = 180, maxlat = -180, minlng = 90, maxlng = -90,
        latLng = google.maps.LatLng;

    while (i < len) {
        var geo = new latLng(data[i][1], data[i][2]),
            lat = geo.lat(),
            lng = geo.lng();

        if (isNaN(lat) || isNaN(lng)) {
            // if there is no geo position, remove from the array
            data.splice(i, 1);
            len = data.length;
            continue;

        } else {
            if (lat > maxlat && lat <= 90) { maxlat = lat; }
            if (lat < minlat && lat >= -90) { minlat = lat; }
            if (lng > maxlng && lng <= 180) { maxlng = lng; }
            if (lng < minlng && lng >= -180) { minlng = lng; }
            
            // change the current array to store the id and the geo object
            data[i] = { "id": data[i][0], "geo": geo };
        }

        i++;
    }

    this.data_ = data;

    this.bounds = [minlat, maxlat, minlng, maxlng];

    this.baseSetMap = this.setMap;
    this.setMap = function (map) {
        if (map != null) {
            this.draw = function () {
                this.drawn();
                this.draw = function () { };
            };
        }
        this.baseSetMap(map);
    }

    if (map != null) this.setMap(map);

}

canvasLabels.prototype.destroy = function () {
    this.setMap(null);
    this.hiddencanvas_ = null;
    this.data_ = null;
    if (this.canvas_ != null) {
        this.canvas_.parentNode.removeChild(this.canvas_);
        this.canvas_ = null;
    }
    if (this.idleEvent_ != null) {
        google.maps.event.removeListener(this.idleEvent_); // clear the idle event from the overlay
        this.idleEvent_ = null;
    }
}

canvasLabels.prototype.addSelected = function (geo) {
    this.selected.push(geo);
}

canvasLabels.prototype.cleanSelected = function () {
    this.selected = [];
}

canvasLabels.prototype.onAdd = function () {
    if (this.idleEvent_ == null) {
        this.idleEvent_ = google.maps.event.addListener(this.map, 'idle', idleHandler(this));
    }
    var canvas = document.createElement('canvas');
    canvas.style.position = "absolute";
    canvas.setAttribute('id', this.name_);
    this.canvas_ = canvas;
    this.getPanes().overlayMouseTarget.appendChild(canvas);

    if (this.icon_ != null && this.icon_.length > 0) {
        this.icon_image_ = document.createElement("img");
        this.icon_image_.setAttribute("src", '/images/icons/' + this.icon_);
    }
}

canvasLabels.prototype.draw = function () { }

canvasLabels.prototype.drawn = function (force) {
    if (this.data_ == null || this.map == null) return;

    var overlayProjection = this.getProjection();

    if (overlayProjection == null) {
        return; /*¿¿¿ Why should be null ???*/
    }

    if (force) {
        this.colors_ = null;
    }

    var zoomlevel = this.map.getZoom(),
        mapbounds = this.map.getBounds(),
        sw = overlayProjection.fromLatLngToDivPixel(mapbounds.getSouthWest()),
        ne = overlayProjection.fromLatLngToDivPixel(mapbounds.getNorthEast()),
        canvas = this.canvas_,
        //canvasWidth = Math.round(ne.x - sw.x),
        //canvasHeight = Math.round(sw.y - ne.y),
        hiddenCanvas = this.hiddencanvas_;

    /* could be that the canvas is not created yet */
    if (canvas == null) {
        return;
    }

    /* the canvas will always cover the full map */
    var canvasWidth = this.map.j.offsetWidth,
        canvasHeight = this.map.j.offsetHeight;

    canvas.style.left = Math.round(sw.x) + 'px';
    canvas.style.top = Math.round(ne.y) + 'px';
    canvas.width = canvasWidth;
    canvas.height = canvasHeight;

    var mne = mapbounds.getNorthEast(),
        msw = mapbounds.getSouthWest();

    //if (!isCanvasContained(this.bounds, [Math.min(mne.lat(), msw.lat()), Math.max(mne.lat(), msw.lat()), Math.min(mne.lng(), msw.lng()), Math.max(mne.lng(), msw.lng())])) {
    //    return;
    //}

    /* if alpha = 0 that means nothing will be visible, so exit here
       is placed here as need to clean the canvas first always
       and that is done when assigning the width and size to the canvas */
    if (this.alphaSize <= 0) return;

    /*
        everything will be rendered to a hidden canvas and then copy
        the hidden canvas into the actual visible one
    */
    hiddenCanvas.width = canvasWidth;
    hiddenCanvas.height = canvasHeight;

    if (this.dotType == 'gradient') // if point type == gradient
    {
        this.drawGradient(hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel);
    } else {
        if (this.dotType == 'simpleheat') {
            this.drawSimpleHeat(hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel);
        } else {
            if (this.external_ !== undefined && this.external_[0].DIRECTION !== undefined) {
                this.drawDirection(hiddenCanvas, mapbounds, overlayProjection, sw, ne);
            } else {
                if (this.icon_image_ != null && this.levels_.indexOf(',' + zoomlevel + ',') >= 0) {
                    this.drawIcon(hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel);
                }
                else {
                    this.drawDot(this.dotType, hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel);
                }
            }
        }
        //} else {
        //    this.drawGradientDot(hiddenCanvas, mapbounds, overlayProjection, sw, ne);
        //}
    }

    this.drawSelected(hiddenCanvas, mapbounds, overlayProjection, sw, ne);

    var ctx = canvas.getContext("2d");
    ctx.drawImage(hiddenCanvas, 0, 0);

    /* draws the white rectangle that will show the rendered area
       just to make it clear to the user what part is drawn
       todo: if the number of points is low everything can be drawn in real time,
       so no borders would be needed */

    ctx.strokeStyle = 'rgba(255,255,255,.5)';
    ctx.lineWidth = 1;
    ctx.rect(0, 0, canvasWidth - 1, canvasHeight - 1);
    ctx.stroke();
}

canvasLabels.prototype.onRemove = function () {
    this.canvas_.parentNode.removeChild(this.canvas_);
    this.canvas_ = null;
    if (this.idleEvent_ != null) {
        google.maps.event.removeListener(this.idleEvent_); // clear the idle event from the overlay
        this.idleEvent_ = null;
    }
    this.icon_image_ = null;
}

canvasLabels.prototype.drawSimpleHeat = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel) {

    var context = hiddenCanvas.getContext("2d"),
        canvasWidth = hiddenCanvas.width,
        canvasHeight = hiddenCanvas.height,
        imageData = context.getImageData(0, 0, canvasWidth, canvasHeight),
        bytes = new Uint32Array(imageData.data.buffer),
        pointSize = this.pointSize,
        adjust = pointSize / 2,
        adjustPow = adjust * adjust,
        alphaSize = this.alphaSize,
        //colors = this.colors_ || drawing.getGradientPow(this.fillColor, this.borderColor, pointSize),
        colors = drawing.getGradientPow(this.fillColor, this.borderColor, pointSize),
        swx = sw.x,
        ney = ne.y,
        maxPoint = 0,
        secMax = 0;

    for (var i = 0, data = this.data_, len = data.length; i < len; i++) {

        var ltlng = data[i].geo;

        if (mapbounds.contains(ltlng)) {
            var startProjection = overlayProjection.fromLatLngToDivPixel(ltlng),
                xpos = Math.round(startProjection.x - swx),
                ypos = Math.round(startProjection.y - ney);

            var x0 = Math.round(xpos - adjust); if (x0 < 0) x0 = 0;
            var x1 = Math.round(xpos + adjust); if (x1 >= canvasWidth) x1 = canvasWidth - 1;
            var y0 = Math.round(ypos - adjust); if (y0 < 0) y0 = 0;
            var y1 = Math.round(ypos + adjust); if (y1 >= canvasHeight) y1 = canvasHeight - 1;

            while (y0 < y1) {

                var yRow = y0 * canvasWidth;

                for (var x = x0; x < x1; x++) {

                    var dist = (x - xpos) * (x - xpos) + (y0 - ypos) * (y0 - ypos);
                    if (dist <= adjustPow) {

                        var size = bytes[yRow + x] + 1;
                        if (size > maxPoint) {
                            maxPoint = size;
                        } else {
                            if (size > secMax) {
                                secMax = size;
                            }
                        }
                        bytes[yRow + x] = size;
                    }

                }

                y0++;
            }

        }

    }

    var pattern = drawing.createPattern([{ r: 0, g: 0, b: 255 }, { r: 0, g: 255, b: 255 }, { r: 0, g: 255, b: 0 }, { r: 255, g: 255, b: 0 }, { r: 255, g: 0, b: 0 }], maxPoint);

    points = canvasHeight;
    while (points--) {
        var y = points * canvasWidth, x = canvasWidth;
        while (x--) {

            var ui32 = bytes[y + x];
            if (ui32 !== 0) {
                var size = Math.round(ui32 * 255 / maxPoint),
                    alpha = Math.round(ui32 * 50 / maxPoint);

                if (size > 255) size = 255;
                if (alpha > 50) alpha = 50;
                var c = pattern[ui32];

                bytes[y + x] = ((150 + alpha) << 24) | (c.b << 16) | (c.g << 8) | (c.r);
            }
        }
    }

    context.putImageData(imageData, 0, 0);
}

canvasLabels.prototype.drawGradient = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel) {

    var pointSize = this.pointSize;
    /* TODO -- need to adjust the this.colors_ array before changing the size of the point*/
    if (this.zoomadjust_) {
        //if (zoomlevel >= 19) {
        //    pointSize = pointSize * 4;
        //} else if (zoomlevel == 18) {
        //    pointSize = pointSize * 3;
        //} else if (zoomlevel == 17) {
        //    pointSize = pointSize * 2;
        //} else if (zoomlevel == 16) {
        //    pointSize = Math.round(pointSize * 1.5);
        //} else
        if (zoomlevel <= 3) {
            pointSize = 3;
        }
    }

    var context = hiddenCanvas.getContext("2d"),
        canvasWidth = hiddenCanvas.width,
        canvasHeight = hiddenCanvas.height,
        imageData = context.getImageData(0, 0, canvasWidth, canvasHeight),
        bytes = new Uint32Array(imageData.data.buffer),
        adjust = pointSize / 2,
        adjustPow = adjust * adjust,
        alphaSize = this.alphaSize,
        //colors = this.colors_ || drawing.getGradientPow(this.fillColor, this.borderColor, pointSize),
        colors = drawing.getGradientPow(this.fillColor, this.borderColor, pointSize),
        swx = sw.x,
        ney = ne.y;


    /* if alpha = 0 that means nothing will be visible, so exit here
       is placed here as need to clean the canvas first always
       and that is done when assigning the width and size to the canvas */
    if (this.alphaSize <= 0) return;

    for (var i = 0, data = this.data_, len = data.length; i < len; i++) {

        var ltlng = data[i].geo;

        if (mapbounds.contains(ltlng)) {

            var startProjection = overlayProjection.fromLatLngToDivPixel(ltlng),
                xpos = Math.round(startProjection.x - swx),
                ypos = Math.round(startProjection.y - ney);

            var x0 = Math.round(xpos - adjust); if (x0 < 0) x0 = 0;
            var x1 = Math.round(xpos + adjust); if (x1 >= canvasWidth) x1 = canvasWidth - 1;
            var y0 = Math.round(ypos - adjust); if (y0 < 0) y0 = 0;
            var y1 = Math.round(ypos + adjust); if (y1 >= canvasHeight) y1 = canvasHeight - 1;

            while (y0 < y1) {

                var yRow = y0 * canvasWidth;

                for (var x = x0; x < x1; x++) {

                    var dist = (x - xpos) * (x - xpos) + (y0 - ypos) * (y0 - ypos);
                    if (dist <= adjustPow) {
                        //dist = Math.round(Math.sqrt(dist)); color array bigger, this shouldn't be needed
                        var color = colors[dist];
                        var ui32 = bytes[yRow + x];

                        // todo: test alternative for performance: !(ui32 >>> 24 >= alphaSize - dist)
                        if (ui32 === 0
                                ||
                                (ui32 !== 0 && (ui32 >>> 24 < alphaSize - dist))
                            ) {
                            bytes[yRow + x] =
                            ((alphaSize - dist) << 24) |	// alpha
                            (color.b << 16) |	// blue
                            (color.g << 8) |	// green
                            color.r;		    // red
                        }
                    }

                }

                y0++;
            }
        }
    }

    context.putImageData(imageData, 0, 0);

}

canvasLabels.prototype.drawDirection = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne) {


    var littlePoints = [];

    for (var i = 0; i < 4; i++) {
        littlePoints.push(this.createCircle(i));
    }

    var xadjust = sw.x + this.pointSize / 2,
        yadjust = ne.y + this.pointSize / 2,
        context = hiddenCanvas.getContext("2d");

    for (var i = 0, len = this.data_.length; i < len; i++) {
        var d = this.data_[i];
        var ltlng = d.geo;

        if (mapbounds.contains(ltlng)) {

            var startProjection = overlayProjection.fromLatLngToDivPixel(ltlng);
            var xpos = Math.round(startProjection.x - xadjust);
            var ypos = Math.round(startProjection.y - yadjust);
            context.drawImage(littlePoints[d.d], xpos, ypos);

        }
    }

}

canvasLabels.prototype.drawDot = function (type, hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel) {

    /* adjust the size of the dot based on the zoom level  */
    var pointSize = this.pointSize;
    if (this.zoomadjust_) {
        if (zoomlevel >= 19) {
            pointSize = pointSize * 4;
        } else if (zoomlevel == 18) {
            pointSize = pointSize * 3;
        } else if (zoomlevel == 17) {
            pointSize = pointSize * 2;
        } else if (zoomlevel == 16) {
            pointSize = Math.round(pointSize * 1.5);
        } else if (zoomlevel <= 3) {
            pointSize = 3;
        }
    }

    /* the template point - will be copied everywhere a new point needs to be placed */
    var dot = drawing.createDot(type, this.fillColor.hex, this.borderColor.hex, pointSize, this.alphaSize, this.steps());

    /* adjust to the position of the point as
        the center of the littlepoint should be actually the
        location of the point */
    var xadjust = sw.x + pointSize / 2,
        yadjust = ne.y + pointSize / 2,
        context = hiddenCanvas.getContext("2d");

    if (this.column == null && dot.length) {
        // failsafe in case there is no column selected for the scattered dot => plain dot
        dot = dot[0];
    }

    if (dot.length) {
        /* scattered dot - multiple colors possible for different values */
        for (var i = 0, data = this.data_, len = data.length; i < len; i++) {

            var ltlng = data[i].geo;

            if (mapbounds.contains(ltlng)) {
                // TODO -> Find the color for the point
                var index = 0; //this.data_[i][this.column];

                var startProjection = overlayProjection.fromLatLngToDivPixel(ltlng);

                context.drawImage(
                        dot[index],
                        Math.round(startProjection.x - xadjust),
                        Math.round(startProjection.y - yadjust)
                    );

            }
        }
    }
    else {
        for (var i = 0, data = this.data_, len = data.length; i < len; i++) {

            var ltlng = data[i].geo;

            if (mapbounds.contains(ltlng)) {

                var startProjection = overlayProjection.fromLatLngToDivPixel(ltlng);

                context.drawImage(
                        dot,
                        Math.round(startProjection.x - xadjust),
                        Math.round(startProjection.y - yadjust)
                    );

            }
        }
    }

}

canvasLabels.prototype.drawIcon = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel) {


    /* the template point - will be copied everywhere a new point needs to be placed */
    var icon = this.icon_image_,
        pointSize = icon.width;

    /* adjust to the position of the point as
        the center of the littlepoint should be actually the
        location of the point */
    var xadjust = sw.x + pointSize / 2,
        yadjust = ne.y + pointSize / 2,
        context = hiddenCanvas.getContext("2d");


    for (var i = 0, data = this.data_, len = data.length; i < len; i++) {

        var ltlng = data[i].geo;

        if (mapbounds.contains(ltlng)) {

            var startProjection = overlayProjection.fromLatLngToDivPixel(ltlng);

            context.drawImage(
                    icon,
                    Math.round(startProjection.x - xadjust),
                    Math.round(startProjection.y - yadjust)
                );

        }
    }

}

canvasLabels.prototype.createCircle = function (dir) {
    var littlepoint = document.createElement("canvas");
    var x = 0, y = 0;
    if (dir != null) {
        if (dir < 2) {
            littlepoint.width = this.pointSize;
            littlepoint.height = this.pointSize + 5;
            y = (dir == 0 ? 5 : 0);
        } else {
            littlepoint.width = this.pointSize + 5;
            littlepoint.height = this.pointSize;
            x = (dir == 3 ? 5 : 0);
        }
    } else {
        littlepoint.width = this.pointSize;
        littlepoint.height = this.pointSize;
    }

    var context = littlepoint.getContext("2d");
    context.globalAlpha = this.alphaSize / 255;

    if (dir != null) {
        context.beginPath();
        context.fillStyle = this.borderColor.hex;
        if (dir < 2) {
            context.moveTo(1, Math.round(littlepoint.height / 2) + (dir == 0 ? 5 : 0));
            context.lineTo(Math.round(littlepoint.width / 2), dir == 1 ? littlepoint.height : 0);
            context.lineTo(littlepoint.width - 1, Math.round(littlepoint.height / 2) + (dir == 0 ? 5 : 0));
        } else {
            context.moveTo(Math.round(littlepoint.width / 2) + (dir == 3 ? 5 : 0), 1);
            context.lineTo(dir == 2 ? littlepoint.width : 0, Math.round(littlepoint.height / 2));
            context.lineTo(Math.round(littlepoint.width / 2) + (dir == 3 ? 5 : 0), littlepoint.height - 1);
        }
        context.fill();
    }

    context.beginPath();
    context.arc(this.pointSize / 2 + x, this.pointSize / 2 + y, this.pointSize / 2, 0, 2 * Math.PI, false);
    context.fillStyle = this.fillColor.hex;
    context.fill();
    context.lineWidth = 1;
    context.strokeStyle = this.borderColor.hex;
    context.stroke();


    return littlepoint;
}

canvasLabels.prototype.drawSelected = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne) {
    /* if there is any selected point 
        renders a custom icon for selected points */
    if (this.selected.length > 0) {

        var xadjust = sw.x + 16,
            yadjust = ne.y + 36,
            context = hiddenCanvas.getContext("2d");

        /* image with the icon to be used for the selected points */
        var selImage = document.getElementById("selPointimg");

        for (var i = 0, selected = this.selected, len = selected.length; i < len; i++) {
            var ltlng = selected[i];

            if (mapbounds.contains(ltlng)) {

                var startProjection = overlayProjection.fromLatLngToDivPixel(ltlng);

                context.drawImage(
                        selImage,
                        Math.round(startProjection.x - xadjust),
                        Math.round(startProjection.y - yadjust),
                        32,
                        37
                    );
            }

        }
    }
}

canvasLabels.prototype.steps = function () {
    var steps = null;
    if (this.minValue != null && this.maxValue != null) {
        steps = this.maxValue - this.minValue;
    }
    return steps;
}