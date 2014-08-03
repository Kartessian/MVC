var K = {
    Map: L.Class.extend({

        // Leaflet API implementation

        initialize: function () {
            this._pane = null;
            this._layers = [];
        },

        onAdd: function (map) {
            var pane = map.getPanes().overlayPane;
            // add the canvas for each layer (if any)
            for (var i = 0, len = this._layers.length; i < len; i++) {
                var canvas = document.createElement("canvas");
                pane.appendChild(canvas);
                this._layers[i]._canvas = canvas;
            }
            this._pane = pane;


            this._moveend = map.on('moveend', this._draw, this);
        },

        onRemove: function (map) {
            var pane = this._pane;
            // remove all the canvas objects inside the pane
            for (var i = 0, len = this._layers.length; i < len; i++) {
                pane.removeChild(this._layers[i]._canvas);
            }
            map.off('moveend', this._draw, this);
        },

    }),

    // public methods

    addLayer: function (layer) {
        this.Map._layers.push(layer);
        return this.Map._layers.length;
    },

    removeLayer: function (layerId) { },

    // constructors

    Layer: function (name, style) {
        this._points = [];
        this._style = style;
        this._alpha = 255;
        this._visible = true;

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
    },

    Point: function (lat, lng, properties) {
        this.geo = L.latLng(lat, lng);
        for (var prop in properties) {
            this[prop] = properties[prop];
        }
    }
}