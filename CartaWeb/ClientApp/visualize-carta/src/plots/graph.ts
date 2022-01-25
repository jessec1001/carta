import * as d3 from "d3";
import { SimulationNodeDatum } from "d3";
import { Plotter } from "types";

interface IGraphVertex {
  id: string;
  label: string;
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
interface IGraphInteraction {
  onClickNode?: (node: IGraphVertex) => void;
  onClickSpace?: () => void;
}

// TODO: Consider using WebCoLa to improve the performance of the visualization.
// TODO: Make sure to add definitions to the SVG for optimal performance.

const GraphPlot: Plotter<IGraphPlot, IGraphInteraction> = (
  container: HTMLElement,
  plot: IGraphPlot,
  interaction?: IGraphInteraction
): ((data: IGraphPlot) => void) => {
  const width = plot.size?.width ?? 800;
  const height = plot.size?.height ?? 640;

  const svgElement = d3
    .select(container)
    .append("svg")
    .attr("viewBox", `${-width / 2} ${-height / 2} ${width} ${height}`);
  const zoomElement = svgElement.append("g");

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

  if (!plot.vertices || !plot.edges) return () => {};

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

  const forceNode = d3.forceManyBody().strength(-500);
  const forceLink = d3
    .forceLink<IGraphVertex & SimulationNodeDatum, IGraphEdge>()
    .id(({ id }) => id)
    .distance(100);
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
      .selectAll("circle");
  // TODO: Preferably, this should be a child of the nodes so that changing the position of nodes doesn't affect the text.
  let text: d3.Selection<d3.BaseType, IGraphVertex, SVGElement, unknown> =
    zoomElement.append("g").selectAll("text");

  // This function updates the data. We call it initially to setup the plot.
  const update = (plot: IGraphPlot) => {
    // We want to preserve positioning and velocity of nodes that are already in the graph.
    const nodeMap = new Map(node.data().map((d) => [d.id, d]));
    const nodes = plot.vertices.map((d) => ({ ...nodeMap.get(d.id), ...d }));
    const links = plot.edges.map((d) => ({ ...d }));

    simulation.nodes(nodes);
    simulation
      .force<d3.ForceLink<IGraphVertex & SimulationNodeDatum, IGraphEdge>>(
        "link"
      )
      ?.links(links);
    simulation.alpha(1).restart();

    link = link
      .data(links, ({ source, target }) => source + "-" + target)
      .join("line")
      .attr("marker-end", ({ directed }) => (directed ? "url(#arrow)" : null));
    node = node
      .data(nodes)
      .join("circle")
      .attr("r", 15)
      .call(drag(simulation as any) as any)
      .on("click", (node) => interaction?.onClickNode?.(node));
    text = text
      .data(nodes)
      .join("text")
      .text(({ label }) => label)
      .attr("text-anchor", "middle");
  };
  update(plot);

  const zoom = d3.zoom<SVGSVGElement, unknown>().on("zoom", (event) => {
    zoomElement.attr("transform", event.transform);
  });
  svgElement.call(zoom).call(zoom.transform, d3.zoomIdentity);

  return update;
};

export default GraphPlot;
