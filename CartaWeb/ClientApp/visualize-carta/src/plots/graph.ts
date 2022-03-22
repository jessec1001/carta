import * as d3 from "d3";
import { SimulationNodeDatum } from "d3";
import { Plotter } from "types";
import { createSvg } from "utility";

interface IGraphVertex {
  id: string;
  label?: string;
  selected?: boolean;
  depth?: number;
}
interface IGraphEdge {
  directed: boolean;
  source: string;
  target: string;
}
interface IGraphPlot {
  vertices: IGraphVertex[];
  edges: IGraphEdge[];

  size?: {
    width: number;
    height: number;
  };

  margin?: {
    left?: number;
    right?: number;
    top?: number;
    bottom?: number;
  };
}

/** Represents the possible interactions with the graph visualization. */
interface IGraphInteraction {
  /** An event listener that is called when a node is called exactly once (does not fire on double click). */
  onSingleClickNode?: (node: string) => void;
  /** An event listener that is called when a node is clicked exactly twice (does not fire on single click). */
  onDoubleClickNode?: (node: string) => void;
  /** An event listener that is called when the empty space is clicked. */
  onClickSpace?: () => void;
}

// TODO: Consider using WebCoLa to improve the performance of the visualization.
// TODO: Make sure to add definitions to the SVG for optimal performance.

const GraphPlot: Plotter<IGraphPlot, IGraphInteraction> = (
  container: HTMLElement,
  plot: IGraphPlot,
  interaction?: IGraphInteraction
): ((data: IGraphPlot, interaction?: IGraphInteraction) => void) => {
  // We store a reference to the data and interaction of the plot.
  const state = {
    plot: { ...plot } as IGraphPlot,
    interaction: { ...(interaction ?? {}) } as IGraphInteraction | undefined,
  };

  // Create the SVG element.
  const { svg } = createSvg(container, plot, true);

  // We set up some event handlers for common interactions.
  const onClickNode = (event: PointerEvent, id: string) => {
    const numClicks = event.detail;
    if (numClicks === 1) {
      state.interaction?.onSingleClickNode?.(id);
    }
    if (numClicks === 2) {
      state.interaction?.onDoubleClickNode?.(id);
    }
  };
  const onClickSpace = (event: PointerEvent) => {
    // Make sure that target is directed at the SVG.
    if (event.target !== svg.node()) return;
    state.interaction?.onClickSpace?.();
  };

  // Construct the SVG element.
  svg.on("click", (event) => onClickSpace(event));
  const zoomElement = svg.append("g");

  // Add a definition for the arrow markers for directed edges.
  svg
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

  // Update the positions of elements when the simulation ticks forward.
  const ticked = () => {
    link
      .attr("x1", ({ source }) => (source as any).x)
      .attr("y1", ({ source }) => (source as any).y)
      .attr("x2", ({ target }) => (target as any).x)
      .attr("y2", ({ target }) => (target as any).y);

    (node as any)
      .attr("cx", ({ x }: { x: any }) => x)
      .attr("cy", ({ y }: { y: any }) => y);

    (text as any)
      .attr("x", ({ x }: { x: number }) => x)
      .attr("y", ({ y }: { y: number }) => y + 35);
  };

  // Handle dragging of nodes.
  const drag = (
    simulation: d3.Simulation<d3.SimulationNodeDatum, undefined>
  ) => {
    const onDragStarted = (event: any) => {
      if (!event.active) simulation.alphaTarget(0.3).restart();
      event.subject.fx = event.subject.x;
      event.subject.fy = event.subject.y;
    };
    const onDragEnded = (event: any) => {
      if (!event.active) simulation.alphaTarget(0.0);
      event.subject.fx = null;
      event.subject.fy = null;
    };
    const onDragged = (event: any) => {
      event.subject.fx = event.x;
      event.subject.fy = event.y;
    };

    return d3
      .drag()
      .on("start", onDragStarted)
      .on("end", onDragEnded)
      .on("drag", onDragged);
  };

  // Setup the forces in the simulation.
  const forceNode = d3.forceManyBody().strength(-500);
  const forceLink = d3
    .forceLink<IGraphVertex & SimulationNodeDatum, IGraphEdge>()
    .id(({ id }) => id)
    .distance(50);
  const forceCenter = d3.forceCenter(0, 0);

  // TODO: Change the center of the graph to the center of the container.
  const simulation = d3
    .forceSimulation<IGraphVertex & SimulationNodeDatum, IGraphEdge>()
    .force("link", forceLink)
    .force("charge", forceNode)
    .force("center", forceCenter)
    .on("tick", ticked);

  let link: d3.Selection<d3.BaseType, IGraphEdge, SVGElement, unknown> =
    zoomElement
      .append("g")
      .attr("stroke", "#999")
      .attr("stroke-opacity", 0.6)
      .attr("stroke-width", 1)
      .selectAll("line");
  let node: d3.Selection<d3.BaseType, IGraphVertex, SVGElement, unknown> =
    zoomElement
      .append("g")
      .attr("fill", "#a1d7a1")
      .attr("stroke", "#53b853")
      .attr("stroke-width", 3)
      .style("cursor", "pointer")
      .selectAll("circle");

  // TODO: Preferably, this should be a child of the nodes so that changing the position of nodes doesn't affect the text.
  let text: d3.Selection<d3.BaseType, IGraphVertex, SVGElement, unknown> =
    zoomElement.append("g").attr("fill", "currentcolor").selectAll("text");

  // This function updates the data. We call it initially to setup the plot.
  const update = (plot: IGraphPlot, interaction?: IGraphInteraction) => {
    // We want to preserve positioning and velocity of nodes that are already in the graph.
    const nodeMap = new Map(state.plot.vertices.map((v) => [v.id, v]));

    // Set the new state.
    state.plot = plot;
    state.plot.vertices = state.plot.vertices.map((v) => ({
      ...v,
      ...nodeMap.get(v.id),
    }));
    state.interaction = interaction;

    // We ensure that we only include edges that have corresponding vertices.
    const nodeIds = new Set(state.plot.vertices.map((d) => d.id));
    const nodes = state.plot.vertices;
    const links = state.plot.edges
      .map((d) => ({ ...d }))
      .filter((d) => nodeIds.has(d.source) && nodeIds.has(d.target));

    // If the nodes have changed, we need to update the simulation.
    let nodesChanged = false;
    if (nodeMap.size !== nodeIds.size) nodesChanged = true;
    else {
      for (const nodeId in nodeIds) {
        if (!nodeMap.has(nodeId)) {
          nodesChanged = true;
          break;
        }
      }
    }
    if (nodesChanged) {
      simulation.nodes(nodes);
      simulation
        .force<d3.ForceLink<IGraphVertex & SimulationNodeDatum, IGraphEdge>>(
          "link"
        )
        ?.links(links);
      simulation.alpha(1).restart();
    }

    // Set all of the data.
    link = link
      .data(links, ({ source, target }) => source + "-" + target)
      .join("line")
      .attr("marker-end", ({ directed }) => (directed ? "url(#arrow)" : null));
    node = node
      .data(nodes)
      .join("circle")
      .attr("r", 15)
      .call(drag(simulation as any) as any)
      .on("click", (event, data) => onClickNode(event, data.id));
    text = text
      .data(nodes)
      .join("text")
      .text(({ label }) => label ?? "")
      .attr("text-anchor", "middle");
  };
  update(plot);

  const zoom = d3
    .zoom<SVGSVGElement, unknown>()
    .on("zoom", (event) => {
      zoomElement.attr("transform", event.transform);
    })
    .on("dblclick.zoom", null);
  svg.call(zoom).call(zoom.transform, d3.zoomIdentity);

  return update;
};

export default GraphPlot;
export type { IGraphPlot, IGraphVertex, IGraphEdge, IGraphInteraction };
