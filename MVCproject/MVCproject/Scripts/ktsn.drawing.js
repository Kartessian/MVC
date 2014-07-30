var drawing = {
    getGradient: function (initialColor, finalColor, pointSize) {

        if (typeof initialColor == 'string') {
            initialColor = this.hexToRgb(initialColor);
        }

        if (typeof finalColor == 'string') {
            finalColor = this.hexToRgb(finalColor);
        }

        var steps = Math.round(pointSize / 2),
            rStep = (finalColor.r - initialColor.r) / steps,
            gStep = (finalColor.g - initialColor.g) / steps,
            bStep = (finalColor.b - initialColor.b) / steps,
            colors = [];

        for (var i = 0; i <= steps; i++) {
            var r = Math.round(initialColor.r + (rStep * i)),
                g = Math.round(initialColor.g + (gStep * i)),
                b = Math.round(initialColor.b + (bStep * i));

            colors.push({ "r": r, "g": g, "b": b, "hex": '#' + this.toHex(r) + this.toHex(g) + this.toHex(b) });
        }

        return colors;
    },

    getGradientPow: function (fillColor, endColor, pointSize) {
        var steps = Math.round(pointSize * pointSize / 4);

        var rStep = (endColor.r - fillColor.r) / steps;
        var gStep = (endColor.g - fillColor.g) / steps;
        var bStep = (endColor.b - fillColor.b) / steps;

        var colors = [];

        for (var i = 0; i <= steps; i++) {
            var r = Math.round(fillColor.r + (rStep * i));
            var g = Math.round(fillColor.g + (gStep * i));
            var b = Math.round(fillColor.b + (bStep * i));

            colors.push({ "r": r, "g": g, "b": b, "hex": '#' + this.toHex(r) + this.toHex(g) + this.toHex(b) });
        }

        return colors;
    },

    createPattern: function (colors, count) {
        var pattern = [], steps = Math.round(count / (colors.length - 1));

        for (var i = 0, len = colors.length - 1 ; i < len; i++) {

            var initialColor = colors[i],
                finalColor = colors[i + 1],
                rStep = (finalColor.r - initialColor.r) / steps,
                gStep = (finalColor.g - initialColor.g) / steps,
                bStep = (finalColor.b - initialColor.b) / steps;

            for (var j = 0 ; j <= steps; j++) {
                var r = Math.round(initialColor.r + (rStep * j)),
                    g = Math.round(initialColor.g + (gStep * j)),
                    b = Math.round(initialColor.b + (bStep * j));

                pattern.push({ "r": r, "g": g, "b": b });
            }

        }
        
        var fC = colors[colors.length - 1];
        while (pattern.length < count) {
            pattern.push(fC);
        }

        return pattern;
    },

    toHex: function (num) {
        num = num.toString(16);
        if (num.length == 1) num = "0" + num;
        return num;
    },

    hexToRgb: function (hex) {
        var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16),
            hex: hex
        } : null;
    },

    invertColor: function (color) {
        if (color[0] == '#') {
            color = color.substring(1);           // remove #
        }
        color = parseInt(color, 16);          // convert to integer
        color = 0xFFFFFF ^ color;             // invert three bytes
        color = color.toString(16);           // convert to hex
        color = ("000000" + color).slice(-6); // pad with leading zeros
        color = "#" + color;                  // prepend #
        return color;
    },

    createDot: function (type, fillColor, borderColor, size, alpha, steps) {
        /// <param name="type">dot type: simple, shadow</param>
        /// <param name="fillColor">dot inner color</param>
        /// <param name="borderColor">dot border color</param>
        /// <param name="size">total diameter of the dot</param>
        /// <param name="alpha">max alpha value</param>
        /// <param name="steps">number of steps for scatter dots</param>

        if (type == 'scatter') {
            if (steps > 1) {
                return this.createScatterDot(fillColor, borderColor, size, alpha, steps);
            } else {
                type = 'plain';
            }
        }

        var canvas = document.createElement("canvas");
        canvas.width = size;
        canvas.height = size;

        var context = canvas.getContext("2d");

        switch (type) {
            case 'shadow':
                // firt draw the shadow
                context.globalAlpha = (alpha / 255) / 2;
                context.beginPath();
                context.arc(size / 2, size / 2, size / 2, 0, 2 * Math.PI, false);
                context.fillStyle = fillColor;
                context.fill();
                // then place the dot
                context.globalAlpha = alpha / 255;
                context.beginPath();
                context.arc(size / 2, size / 2, size / 4, 0, 2 * Math.PI, false);
                context.fillStyle = fillColor;
                context.fill();
                context.lineWidth = 1;
                context.strokeStyle = borderColor;
                context.stroke();
                break;
            case 'plain':
                context.globalAlpha = alpha / 255;
                context.beginPath();
                context.arc(size / 2, size / 2, size / 2, 0, 2 * Math.PI, false);
                context.fillStyle = fillColor;
                context.fill();
                context.lineWidth = 1;
                context.strokeStyle = borderColor;
                context.stroke();
                break;
        }

        return canvas;
    },

    createScatterDot: function (initialColor, finalColor, size, alpha, steps) {

        var colors = this.getGradient(initialColor, finalColor, steps * 2),
            points = [];
        for (var i = 0, len = colors.length; i < len; i++) {
            points.push(this.createDot('plain', colors[i].hex, this.invertColor(colors[i].hex), size, alpha));
        }
        return points;

    }
}