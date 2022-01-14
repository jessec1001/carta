import * as d3 from "d3";
import { Plotter } from "types";

interface IGraphDatum {
  id: string;
  label: string | null;
  neighbors: string[];
}
interface IGraphPlot {
  data: IGraphDatum[];

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
interface IGraphInteraction {}

// TODO: Consider using WebCoLa to improve the performance of the visualization.
// TODO: Make sure to add definitions to the SVG for optimal performance.

const GraphPlot: Plotter<IGraphPlot, IGraphInteraction> = (
  container: HTMLElement,
  plot: IGraphPlot,
  interaction?: IGraphInteraction
): ((data: IGraphPlot) => void) => {
  const width = plot.size?.width ?? 800;
  const height = plot.size?.height ?? 640;

  const margin = {
    left: 60,
    right: 20,
    top: 20,
    bottom: 40,
    ...plot.margin,
  };

  const svgElement = d3
    .select(container)
    .append("svg")
    .attr("viewBox", `0 0 ${width} ${height}`);
  const zoomElement = svgElement.append("g");

  if (!plot.data) return () => {};

  const ticked = () => {
    link
      .attr("x1", ({ source }) => (source as any).x)
      .attr("y1", ({ source }) => (source as any).y)
      .attr("x2", ({ target }) => (target as any).x)
      .attr("y2", ({ target }) => (target as any).y);

    node.attr("cx", ({ x }) => x).attr("cy", ({ y }) => y);
  };

  const nodes: any[] = d3.map(plot.data, (d) => ({ id: d.id }));
  const links = plot.data.map((d) =>
    d.neighbors.map((n) => ({ source: d.id, target: n }))
  );
  const linksFlat = ([] as { source: string; target: string }[]).concat(
    ...links
  );

  const nodeIds = d3.map(plot.data, (d) => d.id);
  const nodeLabels = d3.map(plot.data, (d) => d.label);

  const forceNode = d3.forceManyBody();
  const forceLink = d3.forceLink(linksFlat).id(({ index: i }) => nodeIds[i!]);

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

  // TODO: Change the center of the graph to the center of the container.
  const simulation = d3
    .forceSimulation(nodes as any)
    .force("link", forceLink)
    .force("charge", forceNode)
    .force("center", d3.forceCenter(width / 2, height / 2))
    .on("tick", ticked);

  const link = zoomElement
    .append("g")
    .attr("stroke", "#999")
    .attr("stroke-opacity", 0.6)
    .attr("stroke-width", 1)
    .selectAll("line")
    .data(linksFlat)
    .join("line");

  const node = zoomElement
    .append("g")
    .attr("fill", "#53b853")
    .attr("stroke", "#000000")
    .attr("stroke-width", 1)
    .selectAll("circle")
    .data(nodes)
    .join("circle")
    .attr("r", 5)
    .call(drag(simulation) as any);

  const zoom = d3.zoom<SVGSVGElement, unknown>().on("zoom", (event) => {
    zoomElement.attr("transform", event.transform);
  });
  svgElement.call(zoom).call(zoom.transform, d3.zoomIdentity);

  return () => {};
};

export default GraphPlot;
