var ktsn = {

    init: function () {

        this.sidebar.init();
        this.map.init();
        
        this.sidebar.style.hide();
        this.sidebar.layers.hide();
        this.sidebar.share.hide();

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

        init: function () {
            this._dialogs = $(".dialog-window");
            this._container = $("#ktsn-dialogs");
        },


        hide: function () {
            var t = ktsn.dialogs;
            t._container.hide();
            if (t._current) {
                t._current.hide();
                t._current.find(".bnClose").off("click");
                t._current = null;
            }
            $("#ktsn-busy-background").hide();
        },

        show: function (name) {
            $("#ktsn-busy-background").show();
            this._container.css("display","table");
            this._current = $("#ktsn-dialogs-" + name).css("display", "inline-block");

            this._current.find(".bnClose").on("click", ktsn.dialogs.hide);
        }
    },

    map: {

        _mapName: null,

        init: function () {
            this._mapName = $("#ktsn-mapname");

            // depending on the user selection
            // should init Google Maps or Mapbox
            // by default, use Google Maps
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
