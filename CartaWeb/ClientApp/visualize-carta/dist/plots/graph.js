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
// TODO: Consider using WebCoLa to improve the performance of the visualization.
// TODO: Make sure to add definitions to the SVG for optimal performance.
var GraphPlot = function (container, plot, interaction) {
    var _a;
    var _b, _c, _d, _e;
    var width = (_c = (_b = plot.size) === null || _b === void 0 ? void 0 : _b.width) !== null && _c !== void 0 ? _c : 800;
    var height = (_e = (_d = plot.size) === null || _d === void 0 ? void 0 : _d.height) !== null && _e !== void 0 ? _e : 640;
    var margin = __assign({ left: 60, right: 20, top: 20, bottom: 40 }, plot.margin);
    var svgElement = d3
        .select(container)
        .append("svg")
        .attr("viewBox", "0 0 " + width + " " + height);
    var zoomElement = svgElement.append("g");
    if (!plot.data)
        return function () { };
    var ticked = function () {
        link
            .attr("x1", function (_a) {
            var source = _a.source;
            return source.x;
        })
            .attr("y1", function (_a) {
            var source = _a.source;
            return source.y;
        })
            .attr("x2", function (_a) {
            var target = _a.target;
            return target.x;
        })
            .attr("y2", function (_a) {
            var target = _a.target;
            return target.y;
        });
        node.attr("cx", function (_a) {
            var x = _a.x;
            return x;
        }).attr("cy", function (_a) {
            var y = _a.y;
            return y;
        });
    };
    var nodes = d3.map(plot.data, function (d) { return ({ id: d.id }); });
    var links = plot.data.map(function (d) {
        return d.neighbors.map(function (n) { return ({ source: d.id, target: n }); });
    });
    var linksFlat = (_a = []).concat.apply(_a, links);
    var nodeIds = d3.map(plot.data, function (d) { return d.id; });
    var nodeLabels = d3.map(plot.data, function (d) { return d.label; });
    var forceNode = d3.forceManyBody();
    var forceLink = d3.forceLink(linksFlat).id(function (_a) {
        var i = _a.index;
        return nodeIds[i];
    });
    var drag = function (simulation) {
        var onDragStarted = function (event) {
            if (!event.active)
                simulation.alphaTarget(0.3).restart();
            event.subject.fx = event.subject.x;
            event.subject.fy = event.subject.y;
        };
        var onDragEnded = function (event) {
            if (!event.active)
                simulation.alphaTarget(0.0);
            event.subject.fx = null;
            event.subject.fy = null;
        };
        var onDragged = function (event) {
            event.subject.fx = event.x;
            event.subject.fy = event.y;
        };
        return d3
            .drag()
            .on("start", onDragStarted)
            .on("end", onDragEnded)
            .on("drag", onDragged);
    };
    // TODO: Change the center of the graph to the center of the container.
    var simulation = d3
        .forceSimulation(nodes)
        .force("link", forceLink)
        .force("charge", forceNode)
        .force("center", d3.forceCenter(width / 2, height / 2))
        .on("tick", ticked);
    var link = zoomElement
        .append("g")
        .attr("stroke", "#999")
        .attr("stroke-opacity", 0.6)
        .attr("stroke-width", 1)
        .selectAll("line")
        .data(linksFlat)
        .join("line");
    var node = zoomElement
        .append("g")
        .attr("fill", "#53b853")
        .attr("stroke", "#000000")
        .attr("stroke-width", 1)
        .selectAll("circle")
        .data(nodes)
        .join("circle")
        .attr("r", 5)
        .call(drag(simulation));
    var zoom = d3.zoom().on("zoom", function (event) {
        zoomElement.attr("transform", event.transform);
    });
    svgElement.call(zoom).call(zoom.transform, d3.zoomIdentity);
    return function () { };
};
exports.default = GraphPlot;
//# sourceMappingURL=graph.js.map