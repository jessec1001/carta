"use strict";
// Line Plot characteristcs
/*
  - Can draw multiple sets of independent data (with one label per set of data).
  - Perhaps allow for specifying both (x and y dimensions to allow for freeform plots.)
*/
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
// TODO: Support ordered and unordered domains. (For now, assume that the inputs are already ordered.)
// TODO: Support padding for axis ranges.
// TODO: Support different axis sides.
// TODO: Support axis labels.
// TODO: Time-style axes?
// TODO: Add labels (legend) for the lines.
// TODO: Support different colors.
// TODO: Support line width.
var LinePlot = function (container, data) {
    // TODO: Abstract this section.
    var width = 800;
    var height = 640;
    var marginLeft = 60;
    var marginRight = 20;
    var marginTop = 20;
    var marginBottom = 40;
    // TODO: Abstract this section.
    var svg = d3
        .select(container)
        .append("svg")
        .attr("width", width + marginLeft + marginRight)
        .attr("height", height + marginTop + marginBottom)
        .append("g")
        .attr("transform", "translate(".concat(marginLeft, ",").concat(marginTop, ")"));
    // TODO: Make our data structures as compatible with these data structures as possible.
    var xValues = data.dims[0];
    var yValues = data.dims[1];
    var d = [];
    for (var k = 0; k < xValues.length; k++)
        d.push({ x: xValues[k], y: yValues[k] });
    var x = d3
        .scaleLinear()
        .domain([Math.min.apply(Math, xValues), Math.max.apply(Math, xValues)])
        .range([0, width]);
    var y = d3
        .scaleLinear()
        .domain([Math.min.apply(Math, yValues), Math.max.apply(Math, yValues)])
        .range([height, 0]);
    // TODO: Abstract this section.
    // TODO: Use d3.axis() and .orient("bottom"/"left")
    svg
        .append("g")
        .attr("transform", "translate(0, ".concat(height, ")"))
        .call(d3.axisBottom(x));
    svg.append("g").call(d3.axisLeft(y));
    svg
        .append("path")
        .datum(d)
        .attr("fill", "none")
        .attr("stroke", "#1d7535")
        .attr("stroke-width", 2)
        .attr("d", d3.line(function (d) { return x(d.x); }, function (d) { return y(d.y); }));
};
exports.default = LinePlot;
//# sourceMappingURL=line.js.map