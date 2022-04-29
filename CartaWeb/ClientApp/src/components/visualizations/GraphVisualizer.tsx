import { FC, useCallback, useEffect, useRef, useState } from "react";
import { Job } from "library/api";
import {
  GraphPlot,
  IGraphVertex,
  IGraphEdge,
  IGraphPlotEvents,
  EventDriver,
} from "visualize-carta";
import { useViews } from "components/views";
import { useAPI } from "hooks";

// #region API-defined Structures
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
}
/** The structure of a visualization vertex retrieved from the API. */
interface IVisualizeVertex {
  /** The unique identifier of the vertex. */
  id: string;
  /** The label for the vertex. */
  label?: string;
  /** The properties assigned to the vertex. */
  properties?: Record<string, IVisualizeProperty>;
  /** The edges assigned to the vertex. */
  edges?: IVisualizeEdge[];

  /** The coloration value of the vertex. */
  value?: number;
}
/** The structure of a visualization retrieved from the API. */
interface IVisualizeGraph {
  /** The discriminant type of the plot. */
  type: "graph";

  /** The vertices assigned to the graph. */
  vertices: IVisualizeVertex[];
}
// #endregion

// #region Visualizer-defined Structures
/** Represents a vertex stored by the visualizer. */
interface IDataVertex extends IGraphVertex, IVisualizeVertex {
  /** Whether the vertex is hidden. */
  hidden: boolean;
  /** Whether the vertex is expanded. */
  expanded: boolean;
  /** Whether the vertex has been interacted with by the user. */
  interacted: boolean;
}
/** Represents an edge stored by the visualizer. */
interface IDataEdge extends IGraphEdge, IVisualizeEdge {
  /** The identifier of the source vertex. */
  source: string;
  /** The identifier of the target vertex. */
  target: string;
}
// #endregion

/** The props used for the {@link GraphVisualizer} component. */
interface GraphVisualizerProps {
  /** The field name of the graph visualization. */
  field: string;
  /** The base path to fetch graph information from. */
  path: string;
  /** The container where the plot should be visualized. */
  container: HTMLElement | null;
}
/** A component that helps with rendering a graph visualization using the Carta API and visualization library. */
const GraphVisualizer: FC<GraphVisualizerProps> = ({
  field,
  path,
  container,
}) => {
  // We store a reference to the plot so that we can update it when the data changes.
  const { baseAPI } = useAPI();
  const plotRef = useRef<GraphPlot & EventDriver<IGraphPlotEvents>>(
    new GraphPlot() as any
  );

  // Construct the graph plot.
  useEffect(() => {
    if (container) {
      // Clear the container.
      container.innerHTML = "";
      plotRef.current.container = container;
      plotRef.current.render();
    }
  }, [container]);

  // We store the vertex and edge information in state.
  // We make sure to keep information about the identifiers that have been retrieved so as to avoid unnecessary updates.
  const dataRef = useRef<{
    vertices: Map<string, IDataVertex>;
    edges: Map<string, IDataEdge>;
  }>({
    vertices: new Map(),
    edges: new Map(),
  });

  // We keep a reference to the size of the element.
  const [size, setSize] = useState<[number, number]>([0, 0]);
  useEffect(() => {
    if (!container) return;
    const handleChangeSize = () => {
      setSize((size) => {
        const boundingRect = container.getBoundingClientRect();
        if (size[0] === boundingRect.width && size[1] === boundingRect.height)
          return size;
        else return [boundingRect.width, boundingRect.height];
      });
    };

    const interval = setInterval(handleChangeSize, 25);
    return () => clearInterval(interval);
  }, [container]);
  useEffect(() => {
    plotRef.current.layout = {
      ...plotRef.current.layout,
      size: { width: size[0], height: size[1] },
    };
    plotRef.current.render();
  }, [size]);

  // We perform actions on the view state when the user interacts with the plot.
  const {
    viewId,
    actions: { setTag },
  } = useViews();
  useEffect(() => {
    setTag(viewId, "visualization", field);
  }, [field, setTag, viewId]);

  // Set up some common operations for manipulating the data.
  const doCollapseExpandVertexTree = useCallback((id: string): boolean => {
    // If the current vertex has children collapsed, we expand them while subchildren are not collapsed.
    // If the current vertex has children not collapsed, we collapse all descendants.
    const data = dataRef.current;
    const vertex = data.vertices.get(id);
    if (!vertex) return false;

    // For now, we only consider descendants along direct edges.
    const expanding = !vertex.expanded;
    const completed: string[] = [];
    const pending: string[] = [id];

    const vertices = data.vertices;
    while (pending.length > 0) {
      // Get the next vertex to process.
      const pendingId = pending.pop()!;
      const pendingVertex = vertices.get(pendingId);
      if (!pendingVertex) continue;
      if (completed.includes(pendingId)) continue;
      completed.push(pendingId);

      if (expanding) {
        // If the vertex has collapsed children, expand until descendants have collapsed children.
        vertices.set(pendingId, {
          ...pendingVertex,
          expanded: true,
        });
        for (const [, edge] of data.edges) {
          if (edge.directed && edge.source === pendingId) {
            const targetVertex = vertices.get(edge.target);
            if (targetVertex) {
              vertices.set(edge.target, {
                ...targetVertex,
                hidden: false,
              });
            }
            if (targetVertex?.expanded) pending.push(edge.target);
          }
        }
      } else {
        // If the vertex has expanded children, collapse child nodes.
        vertices.set(id, { ...vertex, expanded: false });
        for (const [, edge] of data.edges) {
          if (edge.target === id) continue;
          if (edge.directed && edge.source === pendingId) {
            const targetVertex = vertices.get(edge.target);
            if (targetVertex)
              vertices.set(edge.target, {
                ...targetVertex,
                hidden: true,
              });
            pending.push(edge.target);
          }
        }
      }
    }
    return true;
  }, []);
  const addVertex = useCallback((vertex: IVisualizeVertex) => {
    // We grab information about whether the vertex has directed ancestors to determine whether it should be collapsed.
    const data = dataRef.current;
    const hidden =
      vertex.edges?.some((edge) => {
        if (edge.target === vertex.id && edge.directed) {
          const parent = data.vertices.get(edge.source);
          return parent ? !parent.expanded : false;
        }
        return false;
      }) ?? false;

    // Set the vertex.
    data.vertices.set(vertex.id, {
      ...vertex,
      selected: false,
      hidden: hidden,
      expanded: false,
      interacted: false,
    });

    // Do updates to all descendants of the vertex including itself.
    // When a vertex is being added or reset, we need to check that it does not cause any descendents to be hidden or unhidden.
    // The only case in which a vertex should become unhidden is if itself is part of a cycle consisting of this vertex.
    // The case in which a vertex should become hidden is if this vertex is a directed ancestor of it and it has not been interacted with.
    let cyclic = false;
    const descendants: { id: string; directed: boolean }[] = [
      { id: vertex.id, directed: false },
    ];
    let descendantIndex = 0;
    while (descendantIndex < descendants.length) {
      const current = descendants[descendantIndex++];
      const currentVertex = data.vertices.get(current.id);
      for (const edge of currentVertex?.edges ?? []) {
        const pendingId = edge.target;
        if (current.id !== edge.source) continue;
        if (pendingId === vertex.id) cyclic = true;
        const existingIndex = descendants.findIndex(
          (descendantId) => descendantId.id === pendingId
        );
        if (existingIndex < 0) {
          descendants.push({
            id: pendingId,
            directed: current.directed || edge.directed,
          });
        } else {
          descendants[existingIndex].directed =
            descendants[existingIndex].directed || edge.directed;
        }
      }
    }

    // If cyclic, make sure that all of the descendants are made visiible.
    if (cyclic) {
      for (const descendantId of descendants) {
        const vertex = data.vertices.get(descendantId.id);
        if (vertex)
          data.vertices.set(descendantId.id, {
            ...vertex,
            expanded: true,
            hidden: false,
          });
      }
    }
    // Otherwise, hide directed vertices.
    else {
      for (const descendantId of descendants) {
        const vertex = data.vertices.get(descendantId.id);
        if (vertex && !vertex.interacted && descendantId.directed)
          data.vertices.set(descendantId.id, {
            ...vertex,
            expanded: false,
            hidden: true,
          });
      }
    }
  }, []);
  const updatePlotData = useCallback(() => {
    const data = dataRef.current;
    const vertices = Array.from(data.vertices.values()).filter(
      (v) => !v.hidden
    );
    const edges = Array.from(data.edges.values());
    plotRef.current.data = { vertices, edges };
    plotRef.current.render();
  }, []);
  const updateData = useCallback(
    (graph: IVisualizeGraph) => {
      const data = dataRef.current;

      // We update edges.
      // We do this first so that we are able to process the hierarchy correctly in the vertex updating stage.
      let edgesChanged = false;
      for (const fetchVertex of graph.vertices) {
        for (const fetchEdge of fetchVertex.edges ?? []) {
          // Check if we already have this edge.
          const existingEdge = data.edges.get(fetchEdge.id);
          if (existingEdge) continue;

          // Add the edge to the set of fetched edge.
          const sourceVertex = data.vertices.get(fetchEdge.source);
          const targetVertex = data.vertices.get(fetchEdge.target);
          if (
            (sourceVertex && !sourceVertex.hidden) ||
            (targetVertex && !targetVertex.hidden)
          ) {
            edgesChanged = true;
          }
          data.edges.set(fetchEdge.id, {
            id: fetchEdge.id,
            source: fetchEdge.source,
            target: fetchEdge.target,
            directed: fetchEdge.directed,
          });
        }
      }
      const removedEdges = new Set(data.edges.keys());
      for (const fetchVertex of graph.vertices) {
        for (const fetchEdge of fetchVertex.edges ?? []) {
          removedEdges.delete(fetchEdge.id);
        }
      }
      if (removedEdges.size > 0) {
        edgesChanged = true;
        for (const edgeId of removedEdges) {
          data.edges.delete(edgeId);
        }
      }

      // We update vertices.
      // We need to check if any vertex has been given an ancestor but has not been interacted with.
      let verticesChanged = false;
      for (const fetchVertex of graph.vertices) {
        // Check if we already have this vertex.
        let existingVertex = data.vertices.get(fetchVertex.id);
        if (existingVertex) continue;

        // Add the vertex to the set of fetched vertices.
        addVertex(fetchVertex);
        existingVertex = data.vertices.get(fetchVertex.id);
        if (existingVertex && !existingVertex.hidden) verticesChanged = true;
      }
      const removedVertices = new Set(data.vertices.keys());
      for (const fetchVertex of graph.vertices) {
        removedVertices.delete(fetchVertex.id);
      }
      if (removedVertices.size > 0) {
        verticesChanged = true;
        for (const vertexId of removedVertices) {
          data.vertices.delete(vertexId);
        }
      }

      if (edgesChanged || verticesChanged) {
        updatePlotData();
        plotRef.current.simulate();
      }
    },
    [addVertex, updatePlotData]
  );

  // Set up some event handlers for the plot.
  const handleClickSpace = useCallback(() => {
    // When the user clicks the space, we deselect all of the vertices.
    const data = dataRef.current;
    let modified = false;
    for (const vertex of data.vertices.values()) {
      if (vertex.selected) {
        modified = true;
        data.vertices.set(vertex.id, { ...vertex, selected: false });
      }
    }
    if (modified) {
      plotRef.current.render();
      updatePlotData();
      const data = dataRef.current;
      const selection = Array.from(data.vertices.values()).filter(
        (v) => v.selected
      );
      setTag(viewId, "selected", selection);
    }
  }, [updatePlotData, setTag, viewId]);
  const handleSingleClickNode = useCallback(
    ({ id }) => {
      // When a node is single-clicked, we toggle the selection of the vertex.
      const data = dataRef.current;
      const vertex = data.vertices.get(id);
      if (vertex) {
        data.vertices.set(id, { ...vertex, selected: !vertex.selected });
        updatePlotData();
        plotRef.current.render();
        const selection = Array.from(data.vertices.values()).filter(
          (v) => v.selected
        );
        setTag(viewId, "selected", selection);
      }
    },
    [updatePlotData, setTag, viewId]
  );
  const handleDoubleClickNode = useCallback(
    ({ id }) => {
      // When a node is double-clicked, we expand or collapse the vertex.
      // We also make sure to unselect the vertex to avoid unnecessary selections.
      const data = dataRef.current;
      const vertex = data.vertices.get(id);
      if (vertex) {
        data.vertices.set(id, { ...vertex, selected: !vertex.selected });
        const selection = Array.from(data.vertices.values()).filter(
          (v) => v.selected
        );
        setTag(viewId, "selected", selection);
      }
      if (doCollapseExpandVertexTree(id)) {
        updatePlotData();
        plotRef.current.simulate();
      }
    },
    [updatePlotData, setTag, viewId, doCollapseExpandVertexTree]
  );

  useEffect(() => {
    const plot = plotRef.current;
    plot.on("clickSpace", handleClickSpace);
    plot.on("singleClickNode", handleSingleClickNode);
    plot.on("doubleClickNode", handleDoubleClickNode);

    return () => {
      plot.off("clickSpace", handleClickSpace);
      plot.off("singleClickNode", handleSingleClickNode);
      plot.off("doubleClickNode", handleDoubleClickNode);
    };
  }, [handleClickSpace, handleDoubleClickNode, handleSingleClickNode]);

  useEffect(() => {
    const fetchData = async () => {
      // TODO: Conditionally fetch only the data that we have not already fetched.
      // TODO: Use smaller batch requests when possible.
      // Fetch all of the data.
      const response = await baseAPI.fetch(`${path}/all`);
      const data = (await response.json()) as Job<IVisualizeGraph>;
      const graph = data.result;

      // Check that the graph is well-defined.
      if (!graph) return;

      updateData(graph);
    };

    // TODO: Check if the plot data actually needs to be updated depending on whether the field was previously complete.
    const interval = setInterval(fetchData, 10000);
    return () => clearInterval(interval);
  }, [path, updateData, baseAPI]);

  return null;
};

export default GraphVisualizer;
export type {
  GraphVisualizerProps,
  IVisualizeProperty,
  IVisualizeVertex,
  IVisualizeEdge,
  IVisualizeGraph,
  IDataVertex,
  IDataEdge,
};
