function getMousePos(canvas, evt) {
    var rect = canvas.getBoundingClientRect();
    return {
        x: evt.clientX - rect.left,
        y: evt.clientY - rect.top
    };
}

function invertColor(hexTripletColor) {
    var color = hexTripletColor;
    color = color.substring(1);           // remove #
    color = parseInt(color, 16);          // convert to integer
    color = 0xFFFFFF ^ color;             // invert three bytes
    color = color.toString(16);           // convert to hex
    color = ("000000" + color).slice(-6); // pad with leading zeros
    color = "#" + color;                  // prepend #
    return color;
}

actionCanvas.prototype = new google.maps.OverlayView();

function actionCanvas(map) {
    this.map_ = map;
    this.canvas_ = null;
    this.mousePos_ = null;
    this.matrix_ = [];
    if (map != null) this.setMap(map);
}

canvasLabels.prototype.destroy = function () {
    this.setMap(null);
    if (this.canvas_ != null) {
        this.canvas_.parentNode.removeChild(this.canvas_);
        this.canvas_ = null;
    }
    this.mousePos_ = null;
    this.matrix_ = null;
}

actionCanvas.prototype.onAdd = function () {
    var canvas = document.createElement('canvas'), t = this;
    canvas.style.position = "absolute";
    canvas.setAttribute('id', 'actionCanvas')
    t.canvas_ = canvas;
    t.getPanes().overlayMouseTarget.appendChild(canvas);
    t.point_ = null;

    t.mousemove_ = google.maps.event.addListener(this.map_, 'mousemove', function (e) {
        var isOver = 0, ds = ktsn.map._datasets
        // the datasets are ordered, so the top one will be the one with the higher index
        // so we go from the last one to the first one
        for (var i = ds.length; i > 0 ; i--) {
            var canvas = ds[i-1].canvasLabels.canvas_;
            if (canvas != null) {
                var context = canvas.getContext("2d");
                var color = context.getImageData(e.pixel.x, e.pixel.y, 1, 1).data;
                if (color[3] > 0) {
                    isOver = i;
                    break;
                }
            }
        }

        this.setOptions({ draggableCursor: (isOver ? 'pointer' : '') });
        this.canClick = isOver;
    });

    t.mouseclick_ = google.maps.event.addListener(this.map_, 'click', function (e) {
        //find the closest point to its location
        if (this.canClick) {
            // mindist is the minimum distance we want to capture the event, squared (^2).
            // 100 = 10 pixels, 36 = 6 pixels, 25 = 5 pixels
            // the distance should be the point size to be sure the point is correctly found when clicked on it
            var x = e.latLng.k, y = e.latLng.A, minDist = 10, point = null, overlay = null, ix = null,
                ds = ktsn.map._datasets;

            // use the layer we found we are hover of
            var data = ds[this.canClick - 1].canvasLabels.data_;
            for (var p = 0, d = data.length; p < d; p++) {
                var dPoint = data[p], geo = dPoint.geo;
                if (geo != null) {
                    var xpos = geo.lat(), ypos = dPoint.geo.lng(),
                        dist = (x - xpos) * (x - xpos) + (y - ypos) * (y - ypos);
                    if (dist < minDist) {
                        minDist = dist;
                        point = dPoint;
                        ix = i;
                        overlay = ds[i].canvasLabels;
                        // if the distance is 0, we cannot get closer
                        if (dist == 0) {
                            break;
                        }
                    }
                }
            }

            if (point != null) {

                showPoint(ix, point, e.latLng);

                var fillColor = drawing.invertColor(overlay.fillColor.hex),
                    borderColor = drawing.invertColor(overlay.borderColor);

                t.point_ = {
                    type: overlay.dotType,
                    fillColor: drawing.invertColor(overlay.fillColor.hex),
                    borderColor: drawing.invertColor(overlay.borderColor),
                    size: overlay.pointSize,
                    alpha: overlay.alphaSize,
                    geo: point.geo
                };

                t.draw();

            } else {
                if (t.point_ != null) {
                    t.point_ = null;
                    t.draw();
                }
            }
        } else {
            if (t.point_ != null) {
                t.point_ = null;
                t.draw();
            }
        }

    });

}

actionCanvas.prototype.draw = function () {
    var mapbounds = this.map_.getBounds(),
        projection = this.getProjection(),
        sw = projection.fromLatLngToDivPixel(mapbounds.getSouthWest()),
        ne = projection.fromLatLngToDivPixel(mapbounds.getNorthEast()),
        canvas = this.canvas_,
        point = this.point_;

    canvas.style.left = Math.round(sw.x) + 'px';
    canvas.style.top = Math.round(ne.y) + 'px';
    // width and height will be always same as container
    canvas.height = this.map_.j.offsetHeight;
    canvas.width = this.map_.j.offsetWidth;

    if (point != null) {
        //overlay for point
        var dot = drawing.createDot(point.type, point.fillColor, point.borderColor, point.size, point.alpha),
            pointLocation = projection.fromLatLngToDivPixel(point.geo),
            context = canvas.getContext("2d");

        context.drawImage(
            dot,
            Math.round(pointLocation.x - (sw.x + point.size / 2)),
            Math.round(pointLocation.y - (ne.y + point.size / 2))
        );
    }
}

actionCanvas.prototype.onRemove = function () {
    this.canvas_.parentNode.removeChild(this.canvas_);
    this.canvas_ = null;
    // clear events
    if (this.mousemove_ != null) {
        google.maps.event.removeListener(this.mousemove_);
        this.mousemove_ = null;
    }
    if (this.mouseclick_ != null) {
        google.maps.event.removeListener(this.mouseclick_);
        this.mouseclick_ = null;
    }
}

function showPoint(ds, point, latlng) {
    var dataset = ktsn.overlays[ds];

    if (dataset.o.external_ === undefined) {
        $.post('/GetPoint', { ds: dataset.ix, id: point.id }, function (result) {
            var html = '';
            if (typeof result == 'object') {
                if (dataset.template != null) {
                    html = dataset.template;
                    $.each(result[0], function (name, value) {
                        html = html.replace('{' + name + '}', value);
                    });
                } else {
                    $.each(result[0], function (name, value) {
                        html += '<p><b>' + name + '</b> ' + value + '</p>';
                    });
                }
            } else {
                html = result;
            }

            createInfoBox(html, point.geo, dataset.o.pointSize);
        });
    } else {
        var html = '';
        if (dataset.template != null) {
            html = dataset.template;
            $.each(dataset.o.external_[point.id], function (name, value) {
                html = html.replace('{' + name + '}', value);
            });
        } else {
            $.each(dataset.o.external_[point.id], function (name, value) {
                html += '<p><b>' + name + '</b> ' + value + '</p>';
            });
        }
        createInfoBox(html, point.geo, dataset.o.pointSize);
    }
}

function createInfoBox(html, location, yoffset) {
    if (ktsn.infobox != null) {
        ktsn.infobox.setMap(null);
    }

    if (yoffset < 7) yoffset = 7;
    yoffset += 2;

    ktsn.infobox = new InfoBox({
        boxClass: 'infobox',
        content: '<div>' + html + '</div>',
        position: location,
        disableAutoPan: false,
        pixelOffset: new google.maps.Size(0, yoffset),
        zIndex: null,
        closeBoxMargin: "12px 4px 2px 2px",
        closeBoxURL: "http://www.google.com/intl/en_us/mapfiles/close.gif",
        infoBoxClearance: new google.maps.Size(1, 1),
        alignBottom: true
    });

    ktsn.infobox.open(ktsn.map._map);
}
