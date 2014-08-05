var K = new function (map, div, key) {
    this._map = L.mapbox.map(div, key).setView([37.72658651203338, -96.55766830232244], 5);
    this._base = new K.baseLayer(map);
    K._pane = null;
    K._layers = [];
    map.addLayer(this._base);
};

K.prototype.baseLayer = L.Class.extend({

    // Leaflet API implementation

    initialize: function () {},

    onAdd: function (map) {
        var pane = map.getPanes().overlayPane;
        // add the canvas for each layer (if any)
        for (var i = 0, len = K._layers.length; i < len; i++) {
            var canvas = document.createElement("canvas");
            pane.appendChild(canvas);
            K._layers[i]._canvas = canvas;
        }
        K._pane = pane;

        this._moveend = map.on('moveend', K._draw, this);
    },

    onRemove: function (map) {
        var pane = K._pane;
        // remove all the canvas objects inside the pane
        for (var i = 0, len = K._layers.length; i < len; i++) {
            pane.removeChild(K._layers[i]._canvas);
        }
        map.off('moveend', K._draw, this);
    },

});

 // public methods

K.prototype.addLayer = function (layer) {
    var canvas = document.createElement("canvas");
    this._pane.appendChild(canvas);

    layer._canvas = canvas;
    this._layers.push(layer);

    // shall it refresh after adding the layer?
    this._draw();

    return this._layers.length;
};

K.prototype.destroy = function () {
    // basic detroy implementation, needs further consideration
    K._base.onRemove(K._map);
    K._layers = [];
};

K.prototype.removeLayer = function (layerId) {
    // remove the canvas from the pane
    this._pane.removeChild(this._layers[layerId]);
    // remove the layer from the array
    this._layers.splice(layerId, 1);
};

// constructors

K.prototype.Layer = function (name, style) {
    this.points = [];
    this.style = style;
    this.alpha = 255;
    this.visible = true;

    this.name = name;

    this.addPoint = function (point) {
        this._points.push(point);
    };

    this.addRange = function (range) {
        var points = this._points;
        for (var i = 0, len = range.length; i < len; i++) {
            points.push(range[i]);
        }
    };
};

K.prototype.Point = function (lat, lng, properties) {
    this.geo = L.latLng(lat, lng);
    for (var prop in properties) {
        this[prop] = properties[prop];
    }
};

// private methods

K.prototype._draw = function () {

    var overlayProjection = this._map,
        mapbounds = this._map.getBounds(),
        zoomlevel = this._map.getZoom(),
        sw = overlayProjection.latLngToLayerPoint(mapbounds._southWest),
        ne = overlayProjection.latLngToLayerPoint(mapbounds._northEast),
        canvasWidth = Math.round(ne.x - sw.x),
        canvasHeight = Math.round(sw.y - ne.y),
        _left = Math.round(sw.x) + 'px',
        _top = Math.round(ne.y) + 'px';

    /* could be that the canvas is not created yet */
    for (var i = 0, len = K._layers.length; i < len; i++) {

        var layer = K._layers[i],
            style = layer.style,
            canvas = layer._canvas,
            hiddenCanvas = document.createElement("canvas");

        canvas.style.left = _left;
        canvas.style.top = _top;
        canvas.width = canvasWidth;
        canvas.height = canvasHeight;

        hiddenCanvas.width = canvasWidth;
        hiddenCanvas.height = canvasHeight;

        // if the canvas will be completely transparent, no need to draw anything
        if (style.alphaSize <= 0) return;

        if (style.dotType == 'gradient') // if point type == gradient
        {
            K.drawGradient(hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel);
        } else {
            if (style.dotType == 'simpleheat') {
                K.drawSimpleHeat(hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel);
            } else {
                K.drawDot(style.dotType, hiddenCanvas, mapbounds, overlayProjection, sw, ne);
            }
        }

        var ctx = canvas.getContext("2d");
        ctx.drawImage(hiddenCanvas, 0, 0);
    }

}

K.prototype.drawSimpleHeat = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel) {

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
            var startProjection = overlayProjection.latLngToLayerPoint(ltlng),
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

K.prototype.drawGradient = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne, zoomlevel) {

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

        if (ltlng != null && mapbounds.contains(ltlng)) {

            var startProjection = overlayProjection.latLngToLayerPoint(ltlng),
                xpos = Math.round(startProjection.x - swx);
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

K.prototype.drawDot = function (type, hiddenCanvas, mapbounds, overlayProjection, sw, ne) {

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
    var dot = drawing.createDot(type, this.fillColor.hex, this.borderColor.hex, pointSize, this.alphaSize);

    /* adjust to the position of the point as
        the center of the littlepoint should be actually the
        location of the point */
    var xadjust = sw.x + pointSize / 2,
        yadjust = ne.y + pointSize / 2,
        context = hiddenCanvas.getContext("2d");

    for (var i = 0, data = this.data_, len = data.length; i < len; i++) {

        var ltlng = data[i].geo;

        if (mapbounds.contains(ltlng)) {

            var startProjection = overlayProjection.latLngToLayerPoint(ltlng);

            context.drawImage(
                    dot,
                    Math.round(startProjection.x - xadjust),
                    Math.round(startProjection.y - yadjust)
                );

        }
    }

}

K.prototype.drawGradientDot = function (hiddenCanvas, mapbounds, overlayProjection, sw, ne) {
    /* like a dot but with different colors instead a solid one*/
    var littlepoint = document.createElement("canvas");

    littlepoint.width = this.pointSize;
    littlepoint.height = this.pointSize;

    var littlecontext = littlepoint.getContext("2d");
    littlecontext.globalAlpha = this.alphaSize / 255;
    littlecontext.beginPath();
    littlecontext.arc(this.pointSize / 2, this.pointSize / 2, this.pointSize / 2, 0, 2 * Math.PI, false);
    littlecontext.fillStyle = this.fillColor.hex;
    littlecontext.fill();
    littlecontext.lineWidth = 1;
    littlecontext.strokeStyle = this.borderColor.hex;
    littlecontext.stroke();

    var xadjust = sw.x + Math.round(this.pointSize / 2),
        yadjust = ne.y + Math.round(this.pointSize / 2),
        context = hiddenCanvas.getContext("2d");

    for (var i = 0, len = this.data_.length; i < len; i++) {

        var ltlng = this.data_[i].geo;

        if (mapbounds.contains(ltlng)) {

            var startProjection = overlayProjection.latLngToLayerPoint(ltlng);
            var xpos = Math.round(startProjection.x - xadjust);
            var ypos = Math.round(startProjection.y - yadjust);

            context.drawImage(littlepoint, xpos, ypos);

        }
    }

}
