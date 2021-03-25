import { resolve } from "path";
import { DataSet, DataView } from "vis-data";

import { dataGetGraph, dataGetVertex, dataGetChildren } from "./carta";
import { Graph, Node, Edge } from "./types/graph";

interface GraphEntry {
  id: number;
  parameters?: Record<string, any>;
}

interface DataNode extends Node {
  visible?: boolean;
  childrenLoaded?: boolean;
  childrenCount?: number;
}
interface DataEdge extends Edge {
  visible?: boolean;
}

interface DataRequests {
  size: number;
  active: number;
  queue: Array<{
    function: () => Promise<any>;
    resolve: () => void;
  }>;
}

export interface GraphIds {
  nodes: string[];
  edges: string[];
}
export interface GraphProperties {
  directed?: boolean;
  dynamic?: boolean;
}
export interface GraphDataSet {
  id: number;
  nodes: DataSet<DataNode>;
  edges: DataSet<DataEdge>;
}
export interface GraphDataView {
  id: number;
  nodes: DataView<DataNode>;
  edges: DataView<DataEdge>;
  properties: Readonly<GraphProperties>;
}
export interface GraphBuffer extends GraphDataSet {
  source: string;
  resource: string;
  parameters?: Record<string, any>;

  initial?: GraphIds;
  properties: GraphProperties;
}

/** The stored graphs in source-resource-graph entry combinations. */
const graphs: Record<string, Record<string, GraphEntry[]>> = {};
/** The stored views in view identifier-graph identifier pairs. */
const views: Record<number, number> = {};

/** The next identifier to assign to a graph. Gets incremented per use. */
let graphNextId: number = 0;
/** The next identifier to assign to a view. Gets incremented per use. */
let viewNextId: number = 0;

/** The stored graph data in identifier-data pairs. */
const graphData: Record<number, GraphBuffer> = {};
/** The stored view data in identifier-data pairs. */
const viewData: Record<number, GraphDataSet> = {};

/** The current state of batched requests. */
const requests: DataRequests = {
  size: 16,
  active: 0,
  queue: [],
};

/**
 * Checks if two objects are shallowly equal to each other.
 * @param obj1 The first object to compare.
 * @param obj2 The second object to compare.
 */
function areObjectsEqual(
  obj1?: Record<string, any>,
  obj2?: Record<string, any>
): boolean {
  if (obj1 === obj2) return true;
  if (!obj1 || !obj2) return false;
  if (Object.keys(obj1).some((key) => obj1[key] !== obj2[key])) return false;
  if (Object.keys(obj2).some((key) => obj1[key] !== obj2[key])) return false;
  return true;
}

/**
 * Makes an asynchronous request that should be batched.
 * @param request The request to batch.
 */
function makeRequest(request: () => Promise<any>): Promise<void> {
  return new Promise((res) => {
    if (requests.active < requests.size) {
      requests.active++;
      request().then(() => {
        requests.active--;
        
        const request = requests.queue.pop();
        if (request) {
          makeRequest(request.function).then(() => resolve());
        }
        res();
      });
    } else requests.queue.push({ function: request, resolve: res });
  });
}

/**
 * Registers a new graph data and returns an integer representing the identifier of the entry.
 * @param source The primary source of the the graph data. For instance, "synthetic" or "hyperthought".
 * @param resource The secondary source of the graph data. For instance, the name or unique identifier in the source.
 * @param parameters The extra parameters of the graph.
 */
export function registerGraph(
  source: string,
  resource: string,
  parameters?: Record<string, any>
): number {
  // We convert the source and resource to lowercase because this should not
  // influence the actual data source.
  source = source.toLowerCase();
  resource = resource.toLowerCase();

  // Check if the graph is already contained in our data sources.
  // We signify a found graph by the index and an unfound graph by -1.
  let foundSource = source in graphs;
  let foundResource = foundSource && resource in graphs[source];
  let foundGraph = -1;
  if (foundResource) {
    for (let k = 0; k < graphs[source][resource].length; k++) {
      if (areObjectsEqual(graphs[source][resource][k].parameters, parameters)) {
        foundGraph = k;
        break;
      }
    }
  }

  if (foundGraph >= 0) {
    // We found the graph so we can return the already created resource.
    return graphs[source][resource][foundGraph].id;
  } else {
    // We did not find the graph so we must create and return a new graph resource.
    if (!foundSource) graphs[source] = {};
    if (!foundResource) graphs[source][resource] = [];
    graphs[source][resource].push({
      id: graphNextId,
      parameters: parameters,
    });
    graphData[graphNextId] = {
      id: graphNextId,
      source: source,
      resource: resource,
      parameters: parameters,
      nodes: new DataSet<DataNode>(),
      edges: new DataSet<DataEdge>(),
      properties: {},
    };
    return graphNextId++;
  }
}

function getViewId(viewOrId: GraphDataView | number): number {
  if (typeof viewOrId === "number") return viewOrId;
  else return viewOrId.id;
}
function getGraphId(viewOrId: GraphDataView | number): number | undefined {
  return views[getViewId(viewOrId)];
}

/**
 * Creates a new view to graph data specified by an identifier. This view will be updated automatically by calls to
 * update the view. Returns null if the graph identifier cannot be found. Otherwise, returns the view identifier and the
 * view content.
 * @example
 * const graphId = registerGraph("Synthetic", "InfiniteDirectedGraph");
 * const view = createView(graphId);
 * @param graphId The identifier for the graph data. Should be retrieved from <code>registerGraph</code>.
 */
export function createView(graphId: number): GraphDataView | null {
  // We need to create an underlying data set for each of the views. Then, create data views using a visibility property
  // on the underlying data set that gives access to only visible data.
  if (graphId in graphData) {
    // If there is a graph corresponding to the identifier, we create new views of the graph data and return them.
    // Notice that we do not need access to the view once created since we perform visibility filtering by identifiers.
    const viewId = viewNextId++;
    views[viewId] = graphId;

    // Construct the data buffers for the view.
    viewData[viewId] = {
      id: viewId,
      nodes: new DataSet<DataNode>(),
      edges: new DataSet<DataEdge>(),
    };

    // Return the constructed data views.
    const graph = graphData[graphId];
    return {
      id: viewId,
      nodes: new DataView<DataNode>(viewData[viewId].nodes, {
        filter: (node) => !!node.visible,
      }),
      edges: new DataView<DataEdge>(viewData[viewId].edges, {
        filter: (edge) => !!edge.visible,
      }),
      properties: graph.properties,
    };
  } else return null;
}
export function deleteView(viewOrId: GraphDataView | number): void {
  // Delete the data corresponding to the specified view.
  const viewId = getViewId(viewOrId);
  delete views[viewId];
  delete viewData[viewId];
}

function updateGraph(graph: GraphBuffer, data: Graph) {
  graph.nodes.update(data.nodes);
  graph.edges.update(data.edges);
}
async function initData(graph: GraphBuffer) {
  // Get the data.
  const data: Graph = await dataGetGraph(
    graph.source,
    graph.resource,
    graph.parameters
  );
  graph.properties.directed = data.directed;
  graph.properties.dynamic = data.dynamic;
  console.log(graph);

  // Set the initial identifiers for the graph.
  const initialNodes = data.nodes.map((node) => node.id);
  const initialEdges = data.edges
    .filter(
      (edge) =>
        initialNodes.includes(edge.from) && initialNodes.includes(edge.to)
    )
    .map((edge) => edge.id);
  graph.initial = {
    nodes: initialNodes,
    edges: initialEdges,
  };

  // Add the data to the graph.
  updateGraph(graph, data);
}
async function addDataVertex(graph: GraphBuffer, id: string) {
  // Get the data containing a single vertex.
  const data: Graph = await dataGetVertex(
    graph.source,
    graph.resource,
    id,
    graph.parameters
  );

  // Add the data to the graph.
  updateGraph(graph, data);
}
async function addDataChildrenVertices(graph: GraphBuffer, id: string) {
  // The children are almost loading, we set the corresponding parent data.
  graph.nodes.update({ id, childrenLoaded: false });

  // Get the data containing the children vertices.
  const data: Graph = await dataGetChildren(
    graph.source,
    graph.resource,
    id,
    graph.parameters
  );

  // The children are done loading, we set the corresponding parent data.
  graph.nodes.update({
    id,
    childrenLoaded: true,
    childrenCount: data.nodes.length,
  });

  // Add the data to the graph.
  updateGraph(graph, data);
}

function addVertexToView(
  graph: GraphBuffer,
  view: GraphDataSet,
  nodeOrId: DataNode | string
) {
  const node =
    typeof nodeOrId === "string" ? graph.nodes.get(nodeOrId) : nodeOrId;
  if (!node) return;

  const viewNode = view.nodes.get(node.id);
  if (!viewNode || !viewNode.visible) {
    // We need to make sure that if the view buffer already contains the data, we do not delete any custom data.
    view.nodes.update({ ...(viewNode ?? node), visible: true });
    view.edges.forEach((edge) => {
      if (edge.visible) return;
      if (
        (edge.from === node.id && view.nodes.get(edge.to)) ||
        (edge.to === node.id && view.nodes.get(edge.from))
      ) {
        view.edges.update({ id: edge.id, visible: true });
      }
    });
    graph.edges.forEach((edge) => {
      if (view.edges.get(edge.id)) return;
      if (
        (edge.from === node.id && view.nodes.get(edge.to)) ||
        (edge.to === node.id && view.nodes.get(edge.from))
      ) {
        view.edges.update({ ...edge, visible: true });
      }
    });

    // Make a request to get the children of the node if necessary.
    if (node.childrenLoaded === undefined && graph.properties.dynamic) {
      makeRequest(() => addDataChildrenVertices(graph, node.id));
    }
  }
}
function removeVertexFromView(view: GraphDataSet, nodeOrId: DataNode | string) {
  const node =
    typeof nodeOrId === "string" ? view.nodes.get(nodeOrId) : nodeOrId;
  if (!node) return;

  const viewNode = view.nodes.get(node.id);
  if (viewNode && viewNode.visible) {
    // We need to make sure that if the view buffer already contains the data, we do not delete any custom data.
    view.nodes.update({ id: node.id, visible: false });
    view.edges.forEach((edge) => {
      if (edge.from === node.id || edge.to === node.id) {
        view.edges.update({ id: edge.id, visible: false });
      }
    });
  }
}

/**
 * Initializes a view to the base state of a graph. This should be called before any other updates to the view are made.
 * This will clear any custom data stored in the view. Loads data as required.
 * @param viewOrId The view or view identifier to initialize.
 */
export function initView(viewOrId: GraphDataView | number): Promise<void> {
  const viewId = getViewId(viewOrId);
  const graphId = getGraphId(viewOrId);
  const view = viewData[viewId];
  if (graphId !== undefined) {
    const graph = graphData[graphId];
    if (graph.initial) {
      // The data already exists and is loaded so we can copy it to the view buffer.
      view.nodes.clear();
      view.edges.clear();

      graph.nodes
        .get(graph.initial.nodes)
        .forEach((node) => addVertexToView(graph, view, node));
      return Promise.resolve();
    } else {
      // The data does not exist and needs to be loaded before copying.
      return makeRequest(async () => {
        await initData(graph);
        await initView(viewOrId);
      });
    }
  }
  return Promise.reject();
}
export function showViewVertex(
  viewOrId: GraphDataView | number,
  id: string
): Promise<void> {
  const viewId = getViewId(viewOrId);
  const graphId = getGraphId(viewOrId);
  const view = viewData[viewId];
  if (graphId !== undefined) {
    const graph = graphData[graphId];
    const node = graph.nodes.get(id);
    if (node) {
      // The data already exists and is loaded so we only update the view buffer.
      addVertexToView(graph, view, node);
      return Promise.resolve();
    } else {
      // The data does not exist and needs to be loaded before updating.
      return makeRequest(async () => {
        await addDataVertex(graph, id);
        await showViewVertex(viewOrId, id);
      });
    }
  }
  return Promise.reject();
}
export function showViewChildrenVertices(
  viewOrId: GraphDataView | number,
  id: string
): Promise<void> {
  const viewId = getViewId(viewOrId);
  const graphId = getGraphId(viewOrId);
  const view = viewData[viewId];
  if (graphId !== undefined) {
    const graph = graphData[graphId];
    const node = graph.nodes.get(id);
    if (!graph.properties.dynamic) return Promise.resolve();
    if (node) {
      if (node.childrenLoaded === true) {
        // If the children are loaded, we can copy them over to the view.
        const childIds = graph.edges
          .get()
          .filter((edge) => node.id === edge.from)
          .map((edge) => edge.to);
        childIds.forEach((childId) => addVertexToView(graph, view, childId));
        return Promise.resolve();
      } else if (node.childrenLoaded === false) {
        // If the children are loading, we just need to wait.
        return Promise.reject();
      } else {
        // The children are not loaded or loaded so we need to request them loaded.
        return makeRequest(async () => {
          await addDataChildrenVertices(graph, id);
          await showViewChildrenVertices(viewOrId, id);
        });
      }
    } else {
      // The parent data does not exist and needs to be loaded before updating.
      return makeRequest(async () => {
        await addDataVertex(graph, id);
        await showViewChildrenVertices(viewOrId, id);
      });
    }
  }
  return Promise.reject();
}
export function hideViewVertex(
  viewOrId: GraphDataView | number,
  id: string
): Promise<void> {
  const viewId = getViewId(viewOrId);
  const view = viewData[viewId];
  removeVertexFromView(view, id);
  return Promise.resolve();
}
export function hideViewChildrenVertices(
  viewOrId: GraphDataView | number,
  id: string
): Promise<void> {
  const viewId = getViewId(viewOrId);
  const view = viewData[viewId];
  const node = view.nodes.get(id);
  if (node) {
    // The node exists. We need to make its children not visible.
    const parentIds: string[] = [node.id];
    while (parentIds.length > 0) {
      const parentId = parentIds.pop();
      const childIds = view.edges
        .get()
        .filter((edge) => edge.from === parentId)
        .map((edge) => edge.to);
      childIds.forEach((childId) => removeVertexFromView(view, childId));

      parentIds.push(...childIds);
    }
  }
  return Promise.resolve();
}
export function updateViewVertex(
  viewOrId: GraphDataView | number,
  node: { id: string; [key: string]: any }
): Promise<void> {
  const viewId = getViewId(viewOrId);
  const view = viewData[viewId];
  view.nodes.update(node);
  return Promise.resolve();
}
