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
                t._current.find(".bnClose,.bnSave,.bnLoad,.map-list li").off("click");
                t._current = null;
            }
            $(".nano").nanoScroller({ destroy: true });
            $("#ktsn-busy-background").hide();
        },

        show: function (name) {
            $("#ktsn-busy-background").show();
            this._container.css("display","table");
            this._current = $("#ktsn-dialogs-" + name).css("display", "inline-block");

            this._current.find(".bnClose").on("click", ktsn.dialogs.hide);

            switch(name) {
                case "user":
                    this._current.find(".bnSave").on("click", ktsn.dialogs._account_save)
                    break;
                case "my":
                    this._current.find(".map-list li").on("click", function (e) {
                        $(this).addClass("selected").siblings().removeClass("selected");
                    });
                    this._current.find(".bnLoad").on("click", function (e) {
                        ktsn.map.load(
                            ktsn.dialogs._current.find(".map-list li.selected").data("id")
                        );
                        ktsn.dialogs.hide();
                    });
                    break;
            }

            $(".nano").nanoScroller();
        }
    },

    map: {

        _mapName: null,

        _datasets: null,

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
        },

        clean: function () {
            if (ktsn.map._datasets != null) {
                $.each(ktsn.map._datasets, function (ix, ds) {
                    // remove the canvas layer
                    ds.canvasLabels.destroy();
                });
                ktsn.map._datasets = null;
            }
        },

        load: function (mapId) {
            ktsn.busy(true);
            ktsn.map.clean();
            $.post("/LoadMap", { "id": mapId }, function (result) {
                var ds = [];
                $.each(result, function (ix, dataset) {
                    ds.push(dataset);
                    ktsn.map.loadDataset(
                        dataset.id,
                        dataset.name,
                        ds.length - 1
                    );
                });
                ktsn.map._datasets = ds;
                ktsn.busy(false);
            });
        },

        loadDataset: function (id, name, ix) {
            $.post('/LoadDataset', { ds: id }, function (result) {
                ktsn.map._datasets[ix].canvasLabels = new canvasLabels(null, result, name);
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
            ktsn.dialogs.show($(this).data("dialog"));
        },

        init: function () {
            this.style = $(".sidebar-user").on("click", this._sidebar_click);
            this.maps = $(".sidebar-my").on("click", this._sidebar_click);
            this.style = $(".sidebar-style").on("click", this._sidebar_click);
            this.layers = $(".sidebar-layers").on("click", this._sidebar_click);
            this.share = $(".sidebar-share").on("click", this._sidebar_click);

            ktsn.dialogs.init();
        },

        account: null,
        layers: null,
        maps: null,
        share: null,
        style: null
    }
}
