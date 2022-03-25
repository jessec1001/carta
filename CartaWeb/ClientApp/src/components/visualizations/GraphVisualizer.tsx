import { FC, useCallback, useEffect, useState } from "react";
import { Job } from "library/api";
import {
  GraphPlot,
  IGraphPlot,
  IGraphVertex,
  IGraphEdge,
  IGraphInteraction,
} from "visualize-carta";

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
  /** The unique identifier of the vertex. */
  id: string;
  /** The label for the vertex. */
  label?: string;
  /** The properties assigned to the vertex. */
  properties: Record<string, IVisualizeProperty>;
  /** The edges assigned to the vertex. */
  edges?: IVisualizeEdge[];

  /** The coloration value of the vertex. */
  value?: number;
  // TODO: style
}
/** The structure of a visualization retrieved from the API. */
interface IVisualizeGraph {
  /** The vertices assigned to the graph. */
  vertices: IVisualizeVertex[];
}

/** Represents a collapsible vertex. */
interface ICollapsibleVertex extends IGraphVertex {
  /** Whether the vertex is collapsed. */
  collapsed: boolean;
  /** Whether the vertex's children are collapsed. */
  collapsedChildren: boolean;
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
  // We store the vertex and edge information in state.
  // We make sure to keep information about the identifiers that have been retrieved so as to avoid unnecessary updates.
  const [vertices, setVertices] = useState<Map<string, ICollapsibleVertex>>(
    new Map()
  );
  const [edges, setEdges] = useState<Map<string, IGraphEdge>>(new Map());

  // We store a state reference to the plot so that we can update it when the data changes.
  const [plot, setPlot] = useState<
    ((data: IGraphPlot, interaction?: IGraphInteraction) => void) | null
  >(null);

  // Set up some common operations for manipulating the data.
  const doCollapseExpandVertexTree = useCallback(
    (id: string) => {
      // If the current vertex has children collapsed, we expand them while subchildren are not collapsed.
      // If the current vertex has children not collapsed, we collapse all descendants.
      const vertex = vertices.get(id);
      if (!vertex) return vertices;

      // For now, we only consider descendants along direct edges.
      const pending: string[] = [id];
      const descendants: string[] = [];
      while (pending.length > 0) {
        const pendingId = pending.pop()!;
        for (const [, edge] of edges) {
          if (edge.directed && edge.source === pendingId) {
            descendants.push(edge.target);
            pending.push(edge.target);
          }
        }
      }

      const newVertices = new Map(vertices);
      // for (const [id, vertex] of vertices) {
      //   newVertices.set(
      //     id,
      //     descendants.includes(id)
      //       ? { ...vertex, collapsed: !vertex.collapsed }
      //       : vertex
      //   );
      // }
      return newVertices;
    },
    [edges, vertices]
  );

  // Set up some event handlers for the plot.
  const handleClickSpace = useCallback(() => {
    // When the user clicks the space, we deselect all of the vertices.
    setVertices((vertices) => {
      vertices = new Map(vertices);
      for (const vertex of vertices.values()) {
        vertices.set(vertex.id, { ...vertex, selected: false });
      }
      return vertices;
    });
  }, []);
  const handleSingleClickNode = useCallback((id: string) => {
    // When a node is single-clicked, we toggle the selection of the vertex.
    setVertices((vertices) => {
      const vertex = vertices.get(id);
      if (vertex) {
        vertices = new Map(vertices);
        vertices.set(id, {
          ...vertex,
          selected: !vertex.selected,
        });
      }
      return vertices;
    });
  }, []);
  const handleDoubleClickNode = useCallback(
    (id: string) => {
      // When a node is double-clicked, we expand or collapse the vertex.
      setVertices((vertices) => {
        const vertex = vertices.get(id);
        if (vertex) return doCollapseExpandVertexTree(vertex.id);
        else return vertices;
      });
    },
    [doCollapseExpandVertexTree]
  );

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

  // Whenever the data is updated, update the plot.
  useEffect(() => {
    if (plot) {
      plot(
        {
          vertices: Array.from(vertices.values()).filter(
            (vertex) => !vertex.collapsed
          ),
          edges: Array.from(edges.values()),
        },
        {
          onClickSpace: handleClickSpace,
          onSingleClickNode: handleSingleClickNode,
          onDoubleClickNode: handleDoubleClickNode,
        }
      );
    }
  }, [
    plot,
    vertices,
    edges,
    handleDoubleClickNode,
    handleClickSpace,
    handleSingleClickNode,
  ]);

  useEffect(() => {
    const updateData = async () => {
      // TODO: Conditionally fetch only the data that we have not already fetched.
      // Fetch all of the data
      const response = await fetch(`${path}/all`);
      const data = (await response.json()) as Job<IVisualizeGraph>;
      const graph = data.result;

      // Check that the graph is well-defined.
      if (!graph) return;

      // Convert the data to the format used by the visualization library.
      setVertices((vertices) => {
        let changed = false;
        for (const fetchVertex of graph.vertices) {
          // Check if we already have this vertex.
          const existingVertex = vertices.get(fetchVertex.id);
          if (existingVertex) continue;

          // Fetch the vertex's edges.
          // If the vertex has no directed edges into the vertex, it is a root-like vertex that should be uncollapsed.
          const edges = fetchVertex.edges ?? [];
          const directed = edges.some((edge) => edge.directed);
          const collapsed =
            !edges.some((edge) => edge.target === fetchVertex.id) && directed;

          // Add the vertex to the set of fetched vertices.
          if (!changed) {
            changed = true;
            vertices = new Map(vertices);
          }
          vertices.set(fetchVertex.id, {
            id: fetchVertex.id,
            label: fetchVertex.label,
            selected: false,
            collapsed: collapsed,
            collapsedChildren: directed,
          });
        }

        return vertices;
      });
      setEdges((edges) => {
        let changed = false;
        for (const fetchVertex of graph.vertices) {
          for (const fetchEdge of fetchVertex.edges ?? []) {
            // Check if we already have this edge.
            const existingEdge = edges.get(fetchEdge.id);
            if (existingEdge) continue;

            // Add the vertex to the set of fetched vertices.
            if (!changed) {
              changed = true;
              edges = new Map(edges);
            }
            edges.set(fetchEdge.id, {
              source: fetchEdge.source,
              target: fetchEdge.target,
              directed: fetchEdge.directed,
            });
          }
        }

        return edges;
      });
    };

    // TODO: Check if the plot data actually needs to be updated depending on whether the field was previously complete.
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
