Number.prototype.formatNumber = function (decPlaces, thouSeparator, decSeparator) {
    var n = this, decPlaces = isNaN(decPlaces = Math.abs(decPlaces)) ? 2 : decPlaces, decSeparator = decSeparator == undefined ? "." : decSeparator, thouSeparator = thouSeparator == undefined ? "," : thouSeparator, sign = n < 0 ? "-" : "", i = parseInt(n = Math.abs(+n || 0).toFixed(decPlaces)) + "", j = (j = i.length) > 3 ? j % 3 : 0;
    return sign + (j ? i.substr(0, j) + thouSeparator : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thouSeparator) + (decPlaces ? decSeparator + Math.abs(n - i).toFixed(decPlaces).slice(2) : "");
};


var ktsn = {

    init: function () {

        // ajax setup initialization
        // this is used to cancel all pending ajax request 
        // if the stop() function is called. Intended to be used
        // when the user performs an action before other action
        // that is awaiting ajax calls is being executed, so it
        // will cancel all pending ajax calls.
        this._ajaxPool = [];
        $.ajaxSetup({
            beforeSend: function (jqXHR) {
                // add the call to the pool array
                ktsn._ajaxPool.push(jqXHR);
            },
            complete: function (jqXHR) {
                // try to find the call in the array
                var index = ktsn._ajaxPool.indexOf(jqXHR);
                // if it's there, remove from the array
                if (index > -1) {
                    ktsn._ajaxPool.splice(index, 1);
                }
            }
        });



        this.sidebar.init();
        this.map.init();
        
        this.sidebar.style.hide();
        this.sidebar.layers.hide();
        this.sidebar.share.hide();


        // setup ajax calls to handle exceptions
        // still plenty of work to do here, but the basic will work for now
        $(document).ajaxError(function (event, xhr, settings) {
            if (xhr.status == 401) { // unauthorized - reload the page
                document.location = "/";
            }
        });

        this.busy(false);
    },

    busy: function (action) {
        if (action) {
            $("#ktsn-busy-background,#ktsn-busy-dialog").show();
        } else {
            $("#ktsn-busy-background,#ktsn-busy-dialog").hide();
        }
    },

    dialogs: {

        _dialogs: null,
        _container: null,
        _current: null,

        _account_save: function () {
            var t = $(this);
            t.html("...").off("click");
            $.post('UpdateAccount', { name: $("#dlg-user-name").val(), email: $("#dlg-user-email").val(), password: $("#dlg-user-password").val() }, function (result) {
                t.siblings("span").html("Saved!");
                t.html("Save").on("click", ktsn.dialogs._account_save);
            });
        },

        init: function () {
            this._dialogs = $(".dialog-window");
            this._container = $("#ktsn-dialogs");
        },

        hide: function () {
            var t = ktsn.dialogs;
            $(".nano:visible").nanoScroller({ destroy: true });
            t._container.hide();
            if (t._current) {
                t._current.hide().removeClass("nano-content");
                t._current.find(".bnClose,.bnSave,.bnLoad,.map-list li,a").off("click");
                t._current.off("click");
                t._current = null;

                $("a.pick-color").spectrum("destroy");

            }
            $("#ktsn-busy-background,#ktsn-tools").hide();
            $("#ktsn-menu .selected").removeClass("selected");
        },

        show: function (option) {

            var name = option.data("dialog"), type = option.data("type"), width = option.data("width");

            ktsn.dialogs.hide();

            if (type == "popup") {
                $("#ktsn-busy-background").show();
                this._container.css("display", "table");
                this._current = $("#ktsn-dialogs-" + name).css("display", "inline-block");
            } else {
                this._current = $("#ktsn-dialogs-" + name).show().addClass("nano-content");
                $("#ktsn-tools").show(); //.animate({ "width": width }, 100);
                option.addClass("selected");
            }

            var current = this._current;

            current.find(".bnClose").on("click", ktsn.dialogs.hide);

            switch (name) {
                case "user":
                    current.find(".bnSave").on("click", ktsn.dialogs._account_save)
                    break;
                case "my":
                    current.on("click", ".map-list li", function (e) {
                        var t = $(this);
                        t.addClass("selected").siblings().removeClass("selected");
                        ktsn.map.load(t.data("id"));
                        ktsn.map.name(t.find("span").text());
                        ktsn.dialogs.hide();
                    });
                    current.find(".bnCreate").on("click", function (e) {
                        var dialog = $("#ktsn-dialogs-new").css("display", "inline-block");
                        $("#ktsn-busy-background").show();
                        ktsn.dialogs._container.css("display", "table");

                        dialog.find(".bnClose").on("click", function (e) {
                            ktsn.dialogs._container.hide();
                            dialog.hide();
                            $("#ktsn-busy-background").hide();
                            $(this).off("click").siblings().off("click");
                        });

                        dialog.find(".bnSave").on("click", function (e) {
                            var t = $(this);
                            $.post("/CreateMap", { name: $("#map-name").val(), descr: $("#map-descr").val() }, function (result) {
                                $("#map-list").append('<li data-id="' + result.id + '"><span class="public">' + result.name + '</span> <small>' + result.created + '</small></li>');
                                t.siblings(".bnClose").click();
                            });
                        });
                    });
                    break;
                case "style":
                    //current.find("a.pick-color").on("click", function (e) {
                        //e.preventDefault();
                        //alert("Not implemented yet!");
                    //});
                    $("#map-features input").off("change").on("change", function (e) {
                        ktsn.map.setStyle(ktsn.dialogs.buildStyle());
                    });
                    $("a.pick-color").spectrum({
                        showInitial: true,
                        showPalette: true,
                        showInput: true,
                        change: function (color) {
                            $(this).siblings("input").data("color", color.toHexString());
                            ktsn.map.setStyle(ktsn.dialogs.buildStyle());
                        }
                    });
                    break;
                case "layers":
                    break;
                case "share":
                    break;
            }

            $(".nano:visible").nanoScroller({ alwaysVisible: true }, true);
        },

        buildStyle: function () {
            var styles = [];
            $.each($("#map-features input"), function (ix, input) {
                input = $(input);

                var entry = {
                    stylers: [
                        { visibility: input.prop("checked") ? "on" : "off" }
                    ]
                };

                entry[input.data("type")] = input.data("name");

                $.each(["color", "hue", "lightness", "saturation", "gamma", "inverse_lightness", "width", "weight"], function (ix, property) {
                    if (input.data(property)) {
                        var aux = {};
                        aux[property] = input.data(property);
                        entry.stylers.push(aux);
                    }
                });

                styles.push(entry);

            });
            return styles;
        }
    },

    map: {

        _map: null,

        _mapName: null,

        _datasets: null,

        _timer: null,

        _canvas: null,

        init: function () {
            this._mapName = $("#ktsn-mapname");

            // depending on the user selection
            // should init Google Maps or Mapbox
            // by default, use Google Maps

            // initializes google maps - should be called once only.
            var mapOptions = {
                center: new google.maps.LatLng(37.72658651203338, 264.55766830232244)
                , zoom: 5
                , mapTypeId: google.maps.MapTypeId.ROADMAP
                , mapTypeControl: false
                , streetViewControl: false
                , panControl: false
                , zoomControl: false
                , minZoom: 3
            };

            this._map = new google.maps.Map(document.getElementById("ktsn-map"), mapOptions);
            // event listener for the sidebar zoom tool
            google.maps.event.addListener(ktsn.map._map, 'zoom_changed', function () { $(".zoom-level").text(this.zoom); });

            K.init({ map: this._map });
        },

        clean: function () {
            K.clean();
            ktsn.map._datasets = null;
            ktsn.map.name("Kartessian Map Editor");

            $("#ktsn-layer-list").empty();
        },

        load: function (mapId) {
            ktsn.busy(true);
            ktsn.map.clean();
            $.post("/LoadMap", { "id": mapId }, function (result) {
                var ds = [];
                $.each(result, function (ix, dataset) {
                    ktsn.map.loadDataset(
                        dataset,
                        ds.length
                    );
                    ds.push(dataset);
                });
                ktsn.map._datasets = ds;

                ktsn.map._timer = setInterval(function () {
                    var complete = true;
                    $.each(ktsn.map._datasets, function (ix, dataset) {
                        if (dataset.canvasLayer == null) {
                            complete = false;
                            return false;
                        }
                    });
                    if (complete) {
                        ktsn.busy(false);
                        clearInterval(ktsn.map._timer);
                    }
                }, 100);

                ktsn.sidebar.style.show();
                ktsn.sidebar.layers.show();
                ktsn.sidebar.share.show();

            });
        },

        loadDataset: function (dataset, ix) {

            var template = $('<li data-id="' + dataset.id + '" data-ix="' + ix + '"><span>' + dataset.name + '</span>' +
                '<div class="ds-load">loading...</div><div class="hidden">' +
                '<a href="#" class="bn-icon-delete right" title="Delete Dataset"></a>' +
                '<a href="#" class="bn-icon-data right" title="View Data"></a>' +
                '<a href="#" class="bn-icon-edit right" title="Edit Layer"></a>' +
                '<a href="#" class="bn-icon-math right" title="Functions"></a>' +
                '<div class="right"><input type="checkbox" ' + (dataset.visible ? "checked" : "") + ' class="switch" id="ckb-' + dataset.id + '" /><label for="ckb-' + dataset.id + '"></label></div>' +
                '<small>0</small></div></li>');

            $("#ktsn-layer-list").append(template);

            $.post('/LoadDataset', { ds: dataset.id }, function (result) {
                //need to adapt the style to the proper style...
                var layer = new K.Layer(dataset.name, {
                    fillColor: drawing.hexToRgb(dataset.style.color1),
                    borderColor: drawing.hexToRgb(dataset.style.color2),
                    alpha: dataset.style.alpha,
                    type: dataset.style.type,
                    pointSize: dataset.style.size
                });

                for (var i = 0, data = result.data, len = data.length; i < len; i++) {
                    var point = data[i];
                    layer.addPoint(new K.Point(point[1], point[2], { id: point[0] }));
                }
                template.find(".hidden").show().siblings(".ds-load").remove();
                template.find("small").html(result.data.length.formatNumber(0, ',', '.'));
                template.find("input").prop("checked", dataset.visible);
                //use default click for now
                //layer.onClick = function (point) {};

                ktsn.map._datasets[ix].canvasLayer = K.addLayer(layer);
            });
        },

        name: function (name) {
            if (name !== undefined) {
                this._mapName.html(name);
            } else {
                return this._mapName.html();
            }
        },

        setStyle: function (style) {
            this._map.setOptions({ 'styles': style });
        }
    },

    sidebar: {

        _sidebar_click: function () {
            ktsn.dialogs.show($(this));
        },

        init: function () {
            var menu = $("#ktsn-menu");
            this.style = menu.find(".sidebar-user").on("click", this._sidebar_click);
            this.maps = menu.find(".sidebar-my").on("click", this._sidebar_click);
            this.style = menu.find(".sidebar-style").on("click", this._sidebar_click);
            this.layers = menu.find(".sidebar-layers").on("click", this._sidebar_click);
            this.share = menu.find(".sidebar-share").on("click", this._sidebar_click);

            $(".zoom-in").on("click", function () {
                var z = ktsn.map._map.getZoom();
                if (z < 20)
                    ktsn.map._map.setZoom(z + 1);
            });
            $(".zoom-out").on("click", function () {
                var z = ktsn.map._map.getZoom();
                if (z > 1)
                    ktsn.map._map.setZoom(z - 1);
            });

            ktsn.dialogs.init();
        },

        account: null,
        layers: null,
        maps: null,
        share: null,
        style: null
    },

    // cancel all pending ajax calls
    stopAjax: function () {
        $(this._ajaxPool).each(function (idx, jqXHR) {
            jqXHR.abort();
        });
        this._ajaxPool = [];
    }
}
