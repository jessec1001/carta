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
    var _a, _b, _c, _d;
    var width = (_b = (_a = plot.size) === null || _a === void 0 ? void 0 : _a.width) !== null && _b !== void 0 ? _b : 800;
    var height = (_d = (_c = plot.size) === null || _c === void 0 ? void 0 : _c.height) !== null && _d !== void 0 ? _d : 640;
    var svgElement = d3
        .select(container)
        .append("svg")
        .attr("viewBox", -width / 2 + " " + -height / 2 + " " + width + " " + height);
    var zoomElement = svgElement.append("g");
    svgElement
        .append("defs")
        .append("marker")
        .attr("id", "arrow")
        .attr("viewBox", "0 -5 20 10")
        .attr("refX", 50)
        .attr("refY", 0)
        .attr("markerWidth", 10)
        .attr("markerHeight", 10)
        .attr("orient", "auto")
        .append("path")
        .attr("fill", "#99999988")
        .attr("d", "M0,-10L20,0L0,10");
    if (!plot.vertices || !plot.edges)
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
        node
            .attr("cx", function (_a) {
            var x = _a.x;
            return x;
        })
            .attr("cy", function (_a) {
            var y = _a.y;
            return y;
        });
        text
            .attr("x", function (_a) {
            var x = _a.x;
            return x;
        })
            .attr("y", function (_a) {
            var y = _a.y;
            return y + 35;
        });
    };
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
    var forceNode = d3.forceManyBody().strength(-500);
    var forceLink = d3
        .forceLink()
        .id(function (_a) {
        var id = _a.id;
        return id;
    })
        .distance(100);
    var forceCenter = d3.forceCenter(0, 0);
    // TODO: Change the center of the graph to the center of the container.
    var simulation = d3
        .forceSimulation()
        .force("link", forceLink)
        .force("charge", forceNode)
        .force("center", forceCenter)
        .on("tick", ticked);
    var link = zoomElement
        .append("g")
        .attr("stroke", "#999")
        .attr("stroke-opacity", 0.6)
        .attr("stroke-width", 1)
        .selectAll("line");
    var node = zoomElement
        .append("g")
        .attr("fill", "#a1d7a1")
        .attr("stroke", "#53b853")
        .attr("stroke-width", 3)
        .selectAll("circle");
    // TODO: Preferably, this should be a child of the nodes so that changing the position of nodes doesn't affect the text.
    var text = zoomElement.append("g").attr("fill", "currentcolor").selectAll("text");
    // This function updates the data. We call it initially to setup the plot.
    var update = function (plot) {
        var _a;
        // We want to preserve positioning and velocity of nodes that are already in the graph.
        // We ensure that we only include edges that have corresponding vertices.
        var nodeMap = new Map(node.data().map(function (d) { return [d.id, d]; }));
        var nodeIds = new Set(plot.vertices.map(function (d) { return d.id; }));
        var nodes = plot.vertices.map(function (d) { return (__assign(__assign({}, nodeMap.get(d.id)), d)); });
        var links = plot.edges
            .map(function (d) { return (__assign({}, d)); })
            .filter(function (d) { return nodeIds.has(d.source) && nodeIds.has(d.target); });
        simulation.nodes(nodes);
        (_a = simulation
            .force("link")) === null || _a === void 0 ? void 0 : _a.links(links);
        simulation.alpha(1).restart();
        link = link
            .data(links, function (_a) {
            var source = _a.source, target = _a.target;
            return source + "-" + target;
        })
            .join("line")
            .attr("marker-end", function (_a) {
            var directed = _a.directed;
            return (directed ? "url(#arrow)" : null);
        });
        node = node
            .data(nodes)
            .join("circle")
            .attr("r", 15)
            .call(drag(simulation))
            .on("click", function (node) { var _a; return (_a = interaction === null || interaction === void 0 ? void 0 : interaction.onClickNode) === null || _a === void 0 ? void 0 : _a.call(interaction, node); });
        text = text
            .data(nodes)
            .join("text")
            .text(function (_a) {
            var label = _a.label;
            return label !== null && label !== void 0 ? label : "";
        })
            .attr("text-anchor", "middle");
    };
    update(plot);
    var zoom = d3.zoom().on("zoom", function (event) {
        zoomElement.attr("transform", event.transform);
    });
    svgElement.call(zoom).call(zoom.transform, d3.zoomIdentity);
    return update;
};
exports.default = GraphPlot;
//# sourceMappingURL=graph.js.map