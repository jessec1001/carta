"use strict";
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    Object.defineProperty(o, k2, { enumerable: true, get: function() { return m[k]; } });
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
var d3 = __importStar(require("d3"));
var three = __importStar(require("three"));
var OrbitControls_1 = require("three/examples/jsm/controls/OrbitControls");
var ScatterPlot3d = function (container, plot) {
    var _a, _b, _c, _d;
    var width = (_b = (_a = plot.size) === null || _a === void 0 ? void 0 : _a.width) !== null && _b !== void 0 ? _b : 800;
    var height = (_d = (_c = plot.size) === null || _c === void 0 ? void 0 : _c.height) !== null && _d !== void 0 ? _d : 640;
    var extentX = d3.extent(plot.data, function (d) { return d.x; });
    var extentY = d3.extent(plot.data, function (d) { return d.y; });
    var extentZ = d3.extent(plot.data, function (d) { return d.z; });
    var camera = new three.PerspectiveCamera(70, width / height, 0.01, 10000);
    camera.lookAt((extentX[0] + extentX[1]) / 2, (extentY[0] + extentY[1]) / 2, (extentZ[0] + extentZ[1]) / 2);
    camera.position.x = (extentX[0] + extentX[1]) / 2;
    camera.position.y = (extentY[0] + extentY[1]) / 2;
    camera.position.z = extentZ[0];
    camera.frustumCulled = false;
    var scene = new three.Scene();
    var material = new three.MeshNormalMaterial();
    for (var k = 0; k < plot.data.length; k++) {
        var geometry = new three.DodecahedronGeometry(plot.data[k].radius, 1);
        geometry.translate(plot.data[k].x, plot.data[k].y, plot.data[k].z);
        var mesh = new three.Mesh(geometry, material);
        mesh.frustumCulled = false;
        scene.add(mesh);
    }
    var renderer = new three.WebGLRenderer({ antialias: true });
    var controls = new OrbitControls_1.OrbitControls(camera, renderer.domElement);
    controls.target.set((extentX[0] + extentX[1]) / 2, (extentY[0] + extentY[1]) / 2, (extentZ[0] + extentZ[1]) / 2);
    controls.enableDamping = true;
    controls.dampingFactor = 0.01;
    controls.rotateSpeed = 0.15;
    renderer.setSize(width, height);
    renderer.setAnimationLoop(function (time) {
        controls.update();
        renderer.render(scene, camera);
    });
    container.appendChild(renderer.domElement);
    return function () { };
};
/**
 * Appends an SVG element containing a generated scatter plot to a container using specified plot data.
 * @param container The container that will have the SVG element plot appended to it.
 * @param plot The scatter plot information.
 */
var ScatterPlot = function (container, plot) {
    var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m, _o, _p, _q, _r, _s, _t, _u, _v;
    if (plot.axes && plot.axes.x && plot.axes.y && plot.axes.z)
        return ScatterPlot3d(container, plot);
    // TODO: Abstract this into a set of defaults.
    // Compute the size of the plot by assigning defaults if not specified.
    var width = (_b = (_a = plot.size) === null || _a === void 0 ? void 0 : _a.width) !== null && _b !== void 0 ? _b : 800;
    var height = (_d = (_c = plot.size) === null || _c === void 0 ? void 0 : _c.height) !== null && _d !== void 0 ? _d : 640;
    // TODO: Try to automatically compute margins using canvas context (https://stackoverflow.com/questions/29031659/calculate-width-of-text-before-drawing-the-text).
    //       This requires an additional 'canvas' package. We could also use 'string-pixel-width'.
    // TODO: Abstract this into a set of defaults.
    // Compute the margin of the plot by assigning defaults if not specified.
    var margin = __assign({ left: 60, right: 20, top: 20, bottom: 40 }, plot.margin);
    // TODO: Abstract this.
    // Create an SVG element with the correct viewbox size.
    var svgElement = d3
        .select(container)
        .append("svg")
        .attr("viewBox", "0 0 " + width + " " + height);
    var zoomElement = svgElement.append("g");
    // If the plot has no data, simply return here.
    if (!plot.data)
        return function () { };
    // TODO: Abstract this.
    // TODO: Add labels to the axes.
    // Construct the ranges of values based on the data or ranges specified by the user.
    var extentX = d3.extent(plot.data, function (value) { return value.x; });
    var scaleX = d3
        .scaleLinear()
        .domain([
        (_h = (_g = (_f = (_e = plot.axes) === null || _e === void 0 ? void 0 : _e.x) === null || _f === void 0 ? void 0 : _f.minimum) !== null && _g !== void 0 ? _g : extentX[0]) !== null && _h !== void 0 ? _h : 0,
        (_m = (_l = (_k = (_j = plot.axes) === null || _j === void 0 ? void 0 : _j.x) === null || _k === void 0 ? void 0 : _k.maximum) !== null && _l !== void 0 ? _l : extentX[1]) !== null && _m !== void 0 ? _m : 0,
    ])
        .range([margin.left, width - margin.right]);
    var extentY = d3.extent(plot.data, function (value) { return value.y; });
    var scaleY = d3
        .scaleLinear()
        .domain([
        (_r = (_q = (_p = (_o = plot.axes) === null || _o === void 0 ? void 0 : _o.y) === null || _p === void 0 ? void 0 : _p.minimum) !== null && _q !== void 0 ? _q : extentY[0]) !== null && _r !== void 0 ? _r : 0,
        (_v = (_u = (_t = (_s = plot.axes) === null || _s === void 0 ? void 0 : _s.y) === null || _t === void 0 ? void 0 : _t.maximum) !== null && _u !== void 0 ? _u : extentY[1]) !== null && _v !== void 0 ? _v : 0,
    ])
        .range([height - margin.bottom, margin.top]);
    // TODO: Abstract this.
    // Use the ranges of the values to create axis elements within the plot.
    zoomElement
        .append("g")
        .attr("transform", "translate(0, " + (height - margin.bottom) + ")")
        .call(d3.axisBottom(scaleX));
    zoomElement
        .append("g")
        .attr("transform", "translate(" + margin.left + ", 0)")
        .call(d3.axisLeft(scaleY));
    // Get the colormap that is used for this plot.
    var extentColor = d3.extent(plot.data, function (value) { return plot.color && value[plot.color]; });
    var scaleColor = findColormap(plot.colormap);
    if (typeof extentColor[0] === "number" && typeof extentColor[1] === "number")
        scaleColor.domain(extentColor);
    var zoom = d3.zoom().on("zoom", function (event) {
        zoomElement.attr("transform", event.transform);
    });
    svgElement.call(zoom).call(zoom.transform, d3.zoomIdentity);
    // Create the actual scatter plot elements as a specific shape.
    zoomElement
        .append("g")
        .selectAll("dot")
        .data(plot.data)
        .enter()
        .append("circle")
        .attr("cx", function (d) { return scaleX(d.x); })
        .attr("cy", function (d) { return scaleY(d.y); })
        .attr("r", 5)
        .style("fill", function (d) {
        var _a;
        var c = (_a = d.color) !== null && _a !== void 0 ? _a : plot.color;
        if (typeof c === "number") {
            return scaleColor(c);
        }
        else
            return c;
    });
    return function () { };
};
var findColormap = function (name) {
    // We use the names of schemes and interpolates directly from D3 to make a mapping from colormap names to colormap
    // ranges. We also add some additonal mappings for more sensible naming conventions especially for creating
    // enumerations.
    var schemesOrdinal = {
        category: d3.schemeCategory10,
        category10: d3.schemeCategory10,
        accent: d3.schemeAccent,
        dark: d3.schemeDark2,
        dark1: d3.schemeDark2,
        dark2: d3.schemeDark2,
        paired: d3.schemePaired,
        pastel: d3.schemePastel1,
        pastel1: d3.schemePastel1,
        pastel2: d3.schemePastel2,
        set1: d3.schemeSet1,
        set2: d3.schemeSet2,
        set3: d3.schemeSet3,
        tableau: d3.schemeTableau10,
        tableau10: d3.schemeTableau10,
    };
    var schemesHues = {
        // Diverging.
        BrBG: d3.schemeBrBG,
        PRGn: d3.schemePRGn,
        PiYG: d3.schemePiYG,
        PuOr: d3.schemePuOr,
        RdBu: d3.schemeRdBu,
        RdGy: d3.schemeRdGy,
        RdYlBu: d3.schemeRdYlBu,
        RdYlGn: d3.schemeRdYlGn,
        spectral: d3.schemeSpectral,
        // Non-diverging.
        BuGn: d3.schemeBuGn,
        BuPu: d3.schemeBuPu,
        GnBu: d3.schemeGnBu,
        OrRd: d3.schemeOrRd,
        PuBuGn: d3.schemePuBuGn,
        PuBu: d3.schemePuBu,
        PuRd: d3.schemePuRd,
        RdPu: d3.schemeRdPu,
        YlGnBu: d3.schemeYlGnBu,
        YlGn: d3.schemeYlGn,
        YlOrBr: d3.schemeYlOrBr,
        YlOrRd: d3.schemeYlOrRd,
        // Single hue.
        blues: d3.schemeBlues,
        greens: d3.schemeGreens,
        greys: d3.schemeGreys,
        oranges: d3.schemeOranges,
        purples: d3.schemePurples,
        reds: d3.schemeReds,
    };
    var interpolates = {
        // Diverging.
        BrBG: d3.interpolateBrBG,
        PRGn: d3.interpolatePRGn,
        PiYG: d3.interpolatePiYG,
        PuOr: d3.interpolatePuOr,
        RdBu: d3.interpolateRdBu,
        RdGy: d3.interpolateRdGy,
        RdYlBu: d3.interpolateRdYlBu,
        RdYlGn: d3.interpolateRdYlGn,
        spectral: d3.interpolateSpectral,
        // Non-diverging.
        BuGn: d3.interpolateBuGn,
        BuPu: d3.interpolateBuPu,
        GnBu: d3.interpolateGnBu,
        OrRd: d3.interpolateOrRd,
        PuBuGn: d3.interpolatePuBuGn,
        PuBu: d3.interpolatePuBu,
        PuRd: d3.interpolatePuRd,
        RdPu: d3.interpolateRdPu,
        YlGnBu: d3.interpolateYlGnBu,
        YlGn: d3.interpolateYlGn,
        YlOrBr: d3.interpolateYlOrBr,
        YlOrRd: d3.interpolateYlOrRd,
        // Single hue.
        blues: d3.interpolateBlues,
        greens: d3.interpolateGreens,
        greys: d3.interpolateGreys,
        oranges: d3.interpolateOranges,
        purples: d3.interpolatePurples,
        reds: d3.interpolateReds,
        // Multiple hue.
        turbo: d3.interpolateTurbo,
        viridis: d3.interpolateViridis,
        inferno: d3.interpolateInferno,
        magma: d3.interpolateMagma,
        plasma: d3.interpolatePlasma,
        cividis: d3.interpolateCividis,
        warm: d3.interpolateWarm,
        cool: d3.interpolateCool,
        cubehelix: d3.interpolateCubehelixDefault,
        cubehelixDefault: d3.interpolateCubehelixDefault,
        rainbow: d3.interpolateRainbow,
        sinebow: d3.interpolateSinebow,
    };
    // Ordinal color schemes do not support indices.
    // Diverging color schemes support indices in [3, 11].
    // Single hue color schemes support indices in [3, 9].
    // Multiple hue color schemes support indices in [3, 9].
    var defaultColors = ["#000", "#000"];
    if (!name)
        return d3.scaleSequential(defaultColors);
    // Try categorical.
    if (name in schemesOrdinal)
        return d3.scaleOrdinal(schemesOrdinal[name]);
    // Try interpolate to categorical.
    var match = name.match(/([a-zA-Z]+)(\d+)/);
    if (match !== null) {
        var index = parseInt(match[1]);
        name = match[0];
        // If we cannot find a categorical scheme with the correct number of items, we use a interpolative scheme.
        // Ideally, these should work identically in most circumstances.
        if (name in schemesHues) {
            var scheme = schemesHues[name][index];
            if (scheme)
                return d3.scaleOrdinal(schemesHues[name][index]);
        }
        else {
            return d3.scaleSequential(interpolates[name]);
        }
    }
    // Try interpolate.
    if (name in interpolates)
        return d3.scaleSequential(interpolates[name]);
    // Default.
    return d3.scaleSequential(defaultColors);
};
exports.default = ScatterPlot;
//# sourceMappingURL=scatter.js.map