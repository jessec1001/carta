import { FC, useEffect, useState } from "react";
import { GraphPlot } from "visualize-carta";
import type { IGraphPlot, IGraphVertex, IGraphEdge } from "visualize-carta";
import { Job } from "library/api";

/** The structure of a visualization property retrieved from the API. */
interface IVisualizeProperty {
  /** The value assigned to the property. */
  value: any;
  /** The subproperties assigned under the property. */
  properties?: Record<string, IVisualizeProperty>;
}
/** The structure of a visualization edge retrieved from the API. */
interface IVisualizeEdge {
  /** The unique identifier of the edge. */
  id: string;
  /** The identifier of the source vertex. */
  source: string;
  /** The identifier of the target vertex. */
  target: string;
  /** Whether the edge is directed or undirected. */
  directed: boolean;
  // TODO: style
}
/** The structure of a visualization vertex retrieved from the API. */
interface IVisualizeVertex {
  id?: string;
  label?: string;
  properties: Record<string, IVisualizeProperty>;
  edges?: IVisualizeEdge[];
  value?: number;
  // TODO: style
}
interface IVisualizeGraph {
  vertices: IVisualizeVertex[];
}

/** The props used for the {@link GraphVisualizer} component. */
interface GraphVisualizerProps {
  /** The base path to fetch graph information from. */
  path: string;
  /** The container where the plot should be visualized. */
  container: HTMLElement | null;
}
/** A component that helps with rendering a graph visualization using the Carta API and visualization library. */
const GraphVisualizer: FC<GraphVisualizerProps> = ({ path, container }) => {
  const [data, setData] = useState<IGraphPlot>({
    vertices: [],
    edges: [],
  });
  const [plot, setPlot] = useState<((data: IGraphPlot) => void) | null>(null);
  console.log(data);

  // Construct the graph plot.
  useEffect(() => {
    if (container) {
      // Clear the container.
      container.innerHTML = "";
      setPlot(() =>
        GraphPlot(container, {
          vertices: [],
          edges: [],
        })
      );
    } else setPlot(null);
  }, [container]);

  useEffect(() => {
    if (plot) {
      plot(data);
    }
  }, [plot, data]);

  useEffect(() => {
    const updateData = async () => {
      const response = await fetch(`${path}/all`);
      const data = (await response.json()) as Job<IVisualizeGraph>;
      const graph = data.result;

      if (!graph) return;

      // Convert the data to the format used by the visualization library.
      const edges: IGraphEdge[] = graph.vertices.flatMap((vertex) => {
        const edges = !vertex.edges
          ? []
          : vertex.edges.map((edge) => ({
              source: edge.source,
              target: edge.target,
              directed: edge.directed,
            }));
        return edges;
      });
      const vertices: IGraphVertex[] = graph.vertices
        .filter((vertex) => vertex.id !== undefined)
        .map((vertex) => ({
          id: vertex.id!,
          label: vertex.label,
          selected: false,
        }));

      setData({
        vertices,
        edges,
      });
    };

    if (plot) {
      const interval = setInterval(updateData, 10000);
      updateData();
      return () => clearInterval(interval);
    }
  }, [plot, path]);

  return null;
};

export default GraphVisualizer;
export type { GraphVisualizerProps };
