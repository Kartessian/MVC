﻿var ktsn = {

    init: function () {

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
            t._container.hide();
            if (t._current) {
                t._current.hide();
                t._current.find(".bnClose,.bnSave,.bnLoad,.map-list li,a").off("click");
                t._current.off("click");
                t._current = null;
            }
            $(".nano").nanoScroller({ destroy: true });
            $("#ktsn-busy-background,#ktsn-tools").hide();
        },

        show: function (name, type) {

            ktsn.dialogs.hide();

            if (type == "popup") {
                $("#ktsn-busy-background").show();
                this._container.css("display", "table");
                this._current = $("#ktsn-dialogs-" + name).css("display", "inline-block");
            } else {
                $("#ktsn-tools").show();
                this._current = $("#ktsn-dialogs-" + name).show();
            }

            var current = this._current;

            current.find(".bnClose").on("click", ktsn.dialogs.hide);

            switch (name) {
                case "user":
                    current.find(".bnSave").on("click", ktsn.dialogs._account_save)
                    break;
                case "my":
                    current.on("click", ".map-list li", function (e) {
                        $(this).addClass("selected").siblings().removeClass("selected");
                    });
                    current.find(".bnLoad").on("click", function (e) {
                        ktsn.map.load(
                            ktsn.dialogs._current.find(".map-list li.selected").data("id")
                        );
                        ktsn.dialogs.hide();
                    });
                    current.find(".bnCreate").on("click", function (e) {
                        current.hide();
                        var dialog = $("#ktsn-dialogs-new").css("display", "inline-block");
                        dialog.find(".bnClose").on("click", function (e) {
                            dialog.hide();
                            current.css("display", "inline-block");
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
                    current.find("a.pick-color").on("click", function (e) {
                        e.preventDefault();
                        alert("Not implemented yet!");
                    });
                    break;
                case "layers":
                    break;
                case "share":
                    break;
            }

            $(".nano:visible").nanoScroller({ alwaysVisible: true });
        }
    },

    map: {

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
        }

    },

    sidebar: {

        _sidebar_click: function () {
            var t = $(this);
            ktsn.dialogs.show(t.data("dialog"), t.data("type"));
        },

        init: function () {
            this.style = $(".sidebar-user").on("click", this._sidebar_click);
            this.maps = $(".sidebar-my").on("click", this._sidebar_click);
            this.style = $(".sidebar-style").on("click", this._sidebar_click);
            this.layers = $(".sidebar-layers").on("click", this._sidebar_click);
            this.share = $(".sidebar-share").on("click", this._sidebar_click);

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
    }
}
