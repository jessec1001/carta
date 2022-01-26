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
var HistogramPlot = function (container, plot) {
    var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m, _o, _p, _q, _r, _s, _t, _u, _v, _w;
    var width = (_b = (_a = plot.size) === null || _a === void 0 ? void 0 : _a.width) !== null && _b !== void 0 ? _b : 800;
    var height = (_d = (_c = plot.size) === null || _c === void 0 ? void 0 : _c.height) !== null && _d !== void 0 ? _d : 640;
    var margin = __assign({ left: 60, right: 20, top: 20, bottom: 40 }, plot.margin);
    var svgElement = d3
        .select(container)
        .append("svg")
        .attr("viewBox", "0 0 ".concat(width, " ").concat(height));
    if (!plot.data)
        return;
    var rangePoints = [];
    for (var k = 0; k < plot.data.length; k++) {
        rangePoints.push(plot.data[k].min);
        rangePoints.push(plot.data[k].max);
    }
    var extentValues = d3.extent(rangePoints);
    var scaleValues = d3
        .scaleLinear()
        .domain([
        (_h = (_g = (_f = (_e = plot.axes) === null || _e === void 0 ? void 0 : _e.x) === null || _f === void 0 ? void 0 : _f.minimum) !== null && _g !== void 0 ? _g : extentValues[0]) !== null && _h !== void 0 ? _h : 0,
        (_m = (_l = (_k = (_j = plot.axes) === null || _j === void 0 ? void 0 : _j.x) === null || _k === void 0 ? void 0 : _k.maximum) !== null && _l !== void 0 ? _l : extentValues[1]) !== null && _m !== void 0 ? _m : 0,
    ])
        .range([margin.left, width - margin.right]);
    var extentFreq = d3.extent(plot.data, function (value) { return value.frequency; });
    var scaleFreq = d3
        .scaleLinear()
        .domain([
        (_r = (_q = (_p = (_o = plot.axes) === null || _o === void 0 ? void 0 : _o.y) === null || _p === void 0 ? void 0 : _p.maximum) !== null && _q !== void 0 ? _q : extentFreq[0]) !== null && _r !== void 0 ? _r : 0,
        (_v = (_u = (_t = (_s = plot.axes) === null || _s === void 0 ? void 0 : _s.y) === null || _t === void 0 ? void 0 : _t.maximum) !== null && _u !== void 0 ? _u : extentFreq[1]) !== null && _v !== void 0 ? _v : 0,
    ])
        .range([height - margin.bottom, margin.top]);
    svgElement
        .append("g")
        .attr("transform", "translate(0, ".concat(height - margin.bottom, ")"))
        .call(d3.axisBottom(scaleValues));
    svgElement
        .append("g")
        .attr("transform", "translate(".concat(margin.left, ", 0)"))
        .call(d3.axisLeft(scaleFreq));
    svgElement
        .append("g")
        .selectAll("rect")
        .data(plot.data)
        .join("rect")
        // TODO: Abstract spacing.
        .attr("x", function (d) { return scaleValues(d.min) + 1; })
        .attr("width", function (d) { return Math.max(0, scaleValues(d.max) - scaleValues(d.min) - 1); })
        .attr("y", function (d) { return scaleFreq(d.frequency); })
        .attr("height", function (d) { return scaleFreq(0) - scaleFreq(d.frequency); })
        .attr("fill", (_w = plot.color) !== null && _w !== void 0 ? _w : "#000");
};
exports.default = HistogramPlot;
//# sourceMappingURL=histogram.js.map