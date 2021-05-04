import RequestBuffer from "library/requests/RequestBuffer";
import { DataSet, DataView } from "vis-data";
import { Graph, GraphProperties, Node, Edge } from ".";
import { DataApi } from "library/api";
import { GraphWorkflow } from "library/api/workflow";
import { Selector } from "library/api/workflow";

export interface DataNode extends Node {
  expanded?: boolean;
  visible?: boolean;

  loading?: number | null;

  childrenLoaded?: boolean;
  childrenLoader?: Promise<boolean> | null;
  childrenCount?: number;

  colorSpace?: [number, number];
  colorComponents?: {
    hue: number;
    saturation: number;
    lightness: number;
  };
}
export interface DataEdge extends Edge {
  visible?: boolean;
}

export type GraphDataEvent = "selectionChanged" | "dataChanged";

export default class GraphData {
  _source: string;
  _resource: string;
  _parameters?: Record<string, any>;

  _buffer: RequestBuffer;
  _initial?: { nodes: string[]; edges: string[] };

  _eventHandlers: Record<GraphDataEvent, Set<() => void>>;

  nodes: DataSet<DataNode>;
  edges: DataSet<DataEdge>;
  visibleNodes: DataView<DataNode>;
  visibleEdges: DataView<DataEdge>;

  workflow: GraphWorkflow;
  properties?: GraphProperties;
  selection: string[];

  /**
   * Creates new graph data. This class is singleton depending on the source, resource, and parameter arguments.
   * Identical-valued graph data share the same underlying data structures.
   * @param source The primary source of the the graph data. For instance, "synthetic" or "hyperthought".
   * @param resource The secondary source of the graph data. For instance, the name or unique identifier in the source.
   * @param parameters The extra parameters of the graph.
   * @param buffer
   */
  constructor(
    source: string,
    resource: string,
    workflow: GraphWorkflow,
    parameters?: Record<string, any>,
    buffer?: RequestBuffer
  ) {
    this._source = source;
    this._resource = resource;
    this._parameters = parameters;
    this._buffer = buffer ?? new RequestBuffer();

    // Create new data sets and views.
    this.nodes = new DataSet<DataNode>();
    this.edges = new DataSet<DataEdge>();
    this.visibleNodes = new DataView<DataNode>(this.nodes, {
      filter: (node) => node.visible === true,
    });
    this.visibleEdges = new DataView<DataEdge>(this.edges, {
      filter: (edge) => edge.visible === true,
    });

    // Create events.
    this._eventHandlers = {
      dataChanged: new Set(),
      selectionChanged: new Set(),
    };

    // Create the selection list.
    this.selection = [];

    // Set the workflow.
    this.workflow = workflow;
    this.setWorkflow(workflow);
  }

  duplicate() {
    const graph = new GraphData(
      this._source,
      this._resource,
      this.workflow,
      this._parameters
    );

    // Copy over the data.
    graph.nodes.update(this.nodes.get());
    graph.edges.update(this.edges.get());
    graph._initial = this._initial;
    graph.properties = this.properties;

    return graph;
  }

  _update(data: Graph) {
    // Set the properties of the graph.
    this.properties = {
      directed: data.directed,
      dynamic: data.dynamic,
    };

    // Perform the update to the underlying data.
    this.nodes.update(data.nodes);
    this.edges.update(data.edges);
  }
  _request(ids: string[], force?: boolean, stopPrefetch?: boolean) {
    // We should try to prefetch children of newly added nodes.
    if (!stopPrefetch) {
      ids
        .filter((id) => !this.hasNodeChildrenOrLoading(id))
        .forEach((id) => {
          this.addNodeChildren(id, force, true);
        });
    }
  }

  _show(ids: string[]) {
    // We need to add or make visible nodes in the view.
    this.nodes.update(ids.map((id) => ({ id, visible: true })));

    // We need to add or make visible edges in the view.
    this.edges.update(
      this.edges
        .get()
        .filter((edge) => {
          return this.hasNode(edge.to) && this.hasNode(edge.from);
        })
        .map((edge) => ({ id: edge.id, visible: true }))
    );
  }
  _hide(ids: string[]) {
    // Make the nodes and connected edges no longer visible.
    this.nodes.update(
      this.nodes.get(ids).map((node) => ({ id: node.id, visible: false }))
    );
    this.edges.update(
      this.edges
        .get()
        .filter((edge) => ids.includes(edge.from) || ids.includes(edge.to))
        .map((edge) => ({ id: edge.id, visible: false }))
    );
  }

  _callEvent(type: GraphDataEvent) {
    this._eventHandlers[type].forEach((handler) => handler());
  }

  on(type: GraphDataEvent, handler: () => void) {
    this._eventHandlers[type].add(handler);
  }
  off(type: GraphDataEvent, handler: () => void) {
    this._eventHandlers[type].delete(handler);
  }

  hasNode(id: string): boolean {
    return this.nodes.get(id) !== null;
  }
  hasNodeChildren(id: string): boolean {
    const node = this.nodes.get(id);
    return node !== null && node.childrenLoaded === true;
  }
  hasNodeChildrenOrLoading(id: string): boolean {
    const node = this.nodes.get(id);
    return node !== null && node.childrenLoaded !== undefined;
  }
  hasEdge(id: string): boolean {
    return this.edges.get(id) !== null;
  }

  computeSelection(selector: Selector): (node: DataNode) => boolean {
    let filter: (node: DataNode) => boolean = (node: DataNode) => true;

    switch (selector.type) {
      case "all":
        filter = () => true;
        break;
      case "none":
        filter = () => false;
        break;
      case "or":
        filter = (node) =>
          selector.selectors.some((subSelector) =>
            this.computeSelection(subSelector)(node)
          );
        break;
      case "and":
        filter = (node) =>
          selector.selectors.every((subSelector) =>
            this.computeSelection(subSelector)(node)
          );
        break;
      case "include":
        filter = (node) => selector.ids.includes(node.id);
        break;
      case "exclude":
        filter = (node) => !selector.ids.includes(node.id);
        break;
      case "expanded":
        filter = (node) => (node as any).expanded === true;
        break;
      case "collapsed":
        filter = (node) => (node as any).expanded !== true;
        break;
      case "vertexName":
        let vertexPattern = selector.pattern;
        filter = (node) =>
          !!node.label && !!node.label.match(new RegExp(vertexPattern, "g"));
        break;
      case "propertyName":
        let propertyPattern = selector.pattern;
        filter = (node) => {
          const regexp = new RegExp(propertyPattern, "g");
          if (node.properties) {
            return node.properties.some(
              (property) => property.id.match(regexp) !== null
            );
          }
          return false;
        };
        break;
      case "propertyRange":
        filter = (node) => {
          if (!node.properties) return false;
          const property = node.properties.find(
            (property) => property.id === selector.property
          );
          if (!property) return false;
          const observations = property.values;
          return observations.some((observation) => {
            if (typeof observation.value !== "number") return false;
            const value = observation.value;
            if (selector.minimum && value < selector.minimum) return false;
            if (selector.maximum && value > selector.maximum) return false;
            return true;
          });
        };
        break;
      case "descendants":
        const descendantIds: string[] = [];
        const openDescendantIds: string[] = [...selector.ids];
        while (openDescendantIds.length > 0) {
          const descendantId = openDescendantIds.pop();
          const newDescendantIds = this.edges
            .get()
            .filter((edge) => edge.from === descendantId)
            .map((edge) => edge.to);

          descendantIds.push(...newDescendantIds);
          openDescendantIds.push(...newDescendantIds);
        }
        filter = (node) => descendantIds.includes(node.id);
        break;
      case "ancestors":
        const ancestorIds: string[] = [];
        const openAncestorIds: string[] = [...selector.ids];
        while (openAncestorIds.length > 0) {
          const ancestorId = openAncestorIds.pop();
          const newAncestorIds = this.edges
            .get()
            .filter((edge) => edge.to === ancestorId)
            .map((edge) => edge.from);

          ancestorIds.push(...newAncestorIds);
          openAncestorIds.push(...newAncestorIds);
        }
        filter = (node) => ancestorIds.includes(node.id);
        break;
      case "degree":
        const inDegree = selector.inDegree;
        const outDegree = selector.outDegree;
        filter = (node) => {
          const inEdgeCount = this.edges
            .get()
            .filter((edge) => edge.to === node.id).length;
          const outEdgeCount = this.edges
            .get()
            .filter((edge) => edge.from === node.id).length;
          if (inDegree !== undefined && inDegree !== inEdgeCount) return false;
          if (outDegree !== undefined && outDegree !== outEdgeCount)
            return false;
          return true;
        };
        break;
    }
    return filter;
  }
  updateSelection = () => {
    const selector = this.workflow.getSelector();
    const filter = this.computeSelection(selector);
    const selectedIds = this.visibleNodes
      .get()
      .filter(filter)
      .map((node) => node.id);
    this.selection = selectedIds;
    this._callEvent("selectionChanged");
  };
  updateData = () => {
    // Force a reload of all the selected nodes.
    const filter = this.computeSelection(this.workflow.getSelector());
    this._parameters = { ...this._parameters, workflow: this.workflow?._id };
    this.nodes
      .get()
      .filter(filter)
      .forEach((node) =>
        this.addNode(node.id as string, true, true).then(() => {
          this._callEvent("dataChanged");
        })
      );
  };

  setWorkflow(workflow: GraphWorkflow) {
    // Set up event handlers.
    this.workflow.off("selectorChanged", this.updateSelection);
    this.workflow.off("workflowChanged", this.updateData);
    this.workflow = workflow;
    this.workflow.on("selectorChanged", this.updateSelection);
    this.workflow.on("workflowChanged", this.updateData);

    // Set the workflow related information.
    this.updateSelection();
    this.updateData();
  }

  colorComponentsToHSL(components: {
    hue: number;
    saturation: number;
    lightness: number;
  }) {
    const h = `${360 * components.hue}`;
    const s = `${100 * components.saturation}%`;
    const l = `${100 * components.lightness}%`;
    return `hsl(${h},${s},${l})`;
  }
  colorNode(node: DataNode) {
    const parentIds = this.visibleEdges
      .get()
      .filter((edge) => edge.to === node.id)
      .map((edge) => edge.from);

    if (parentIds.length > 0) {
      // There is a parent node - for now we assume only one.
      const parentId = parentIds.shift() as string;
      const parent: DataNode | null = this.visibleNodes.get(parentId);
      if (parent) {
        if (parent.colorSpace) {
          // The parent is colored so we need to split the colorspace and assign this node a color.
          const childIds = this.visibleEdges
            .get()
            .filter((edge) => edge.from === parentId)
            .map((edge) => edge.to);

          // We split the parent colorspace evenly among the children. Then, the children get the specific color that
          // is the midway point of the colorspace.
          const parentColorSpace = parent.colorSpace;

          const splitSize =
            (parentColorSpace[1] - parentColorSpace[0]) /
            (childIds.length || 1);
          const splitIndex = childIds.indexOf(node.id);

          const colorSpace = [
            parentColorSpace[0] + splitSize * splitIndex,
            parentColorSpace[0] + splitSize * (splitIndex + 1),
          ];
          const colorComponents = {
            hue: (colorSpace[0] + colorSpace[1]) / 2,
            saturation: 1.0,
            lightness: 0.5,
          };

          this.updateNodes([
            {
              id: node.id,
              color: this.colorComponentsToHSL(colorComponents),
              colorSpace,
              colorComponents,
            },
          ]);
        } else {
          // The parent is uncolored so we should color it first.
          this.colorNode(parent);
          this.colorNode(node);
        }
      }
    } else {
      // There is not a parent node so we color the node black.
      const colorSpace = [0.0, 1.0];
      const colorComponents = {
        hue: 0.0,
        saturation: 0.0,
        lightness: 0.0,
      };
      this.updateNodes([
        {
          id: node.id,
          color: this.colorComponentsToHSL(colorComponents),
          colorSpace,
          colorComponents,
        },
      ]);
    }
  }
  colorGraph() {
    this.visibleNodes
      .get()
      .filter((node: DataNode) => !node.colorComponents)
      .forEach((node: DataNode) => this.colorNode(node));
  }

  expandNode(id: string) {
    const node: DataNode | null = this.nodes.get(id);
    if (node) {
      this.updateNodes([{ id, expanded: true }]);
      this.showNodeChildren(id).then(() => this.colorGraph());
    }
  }
  collapseNode(id: string) {
    const node: DataNode | null = this.nodes.get(id);
    if (node) {
      this.updateNodes([{ id, expanded: false }]);
      this.hideNodeChildren(id).then(() => this.colorGraph());
    }
  }

  async initialize(force?: boolean, stopPrefetch?: boolean): Promise<boolean> {
    // Check if we need to actually perform the initialization.
    if (!force && this._initial) {
      this._request(this._initial.nodes, force, stopPrefetch);
      return false;
    }

    // Clear the existing data.
    this.nodes.clear();
    this.edges.clear();

    // Get the base graph data.
    const data = await this._buffer.request(async () => {
      return await DataApi.getGraphRootsAsync({
        source: this._source,
        resource: this._resource,
        parameters: this._parameters,
      });
    });

    // Set the initial identifiers for the graph.
    const initialNodes = data.nodes.map((node) => node.id);
    const initialEdges = data.edges.map((edge) => edge.id);
    this._initial = {
      nodes: initialNodes,
      edges: initialEdges,
    };

    // Add the data to the graph.
    this._update(data);
    this._request(initialNodes, force, stopPrefetch);
    this._show(initialNodes);
    return true;
  }
  async addNode(
    id: string,
    force?: boolean,
    stopPrefetch?: boolean
  ): Promise<boolean> {
    // Check if we need to actually add the node.
    this._request([id], force, stopPrefetch);
    if (!force && this.hasNode(id)) return false;
    if (!(this.properties && this.properties.dynamic)) return false;

    // Get the node data.
    this.nodes.update({ id, loading: 0 });
    const loadingInterval = setInterval(() => {
      const node = this.nodes.get(id);
      if (node) {
        if (typeof node.loading === "number")
          this.nodes.update({ id, loading: (node.loading as number) + 250 });
        else this.nodes.update({ id, loading: 0 });
      }
    }, 250);
    const data = await this._buffer.request(async () => {
      return await DataApi.getGraphVertexAsync({
        source: this._source,
        resource: this._resource,
        id,
        parameters: this._parameters,
      });
    });

    // Add the data to the graph.
    this._update(data);
    this.nodes.update({ id, loading: null });
    clearInterval(loadingInterval);
    return true;
  }
  async addNodeChildren(
    id: string,
    force?: boolean,
    stopPrefetch?: boolean
  ): Promise<boolean> {
    // Check if we need to actually add the node children.
    if (!force) {
      const node = this.nodes.get(id);
      if (node) {
        if (node.childrenLoaded === true) {
          this._request(
            this.edges
              .get()
              .filter((edge) => edge.from === id)
              .map((edge) => edge.to),
            force,
            stopPrefetch
          );
          return false;
        }
        if (node.childrenLoaded === false)
          return (await node.childrenLoader) as boolean;
      }
    }
    if (!(this.properties && this.properties.dynamic)) return false;

    // Create a promise that executes the data retrieval.
    const promise = new Promise<boolean>((res) => {
      // Get the node children data.
      this._buffer
        .request(async () => {
          return await DataApi.getGraphChildrenAsync({
            source: this._source,
            resource: this._resource,
            id,
            parameters: this._parameters,
          });
        })
        .then((data: Graph) => {
          // Add the data to the graph.
          this._request(
            data.nodes.map((node) => node.id),
            force,
            stopPrefetch
          );
          this._update(data);
          this.nodes.update({ id, childrenCount: data.nodes.length });
          return true;
        })
        .then((success: boolean) => res(success));
    });

    // Wait for the children to load.
    this.nodes.update({ id, childrenLoaded: false, childrenLoader: promise });
    const success = await promise;
    this.nodes.update({ id, childrenLoaded: true, childrenLoader: null });
    return success;
  }

  async showNode(id: string): Promise<void> {
    await this.addNode(id);
    this._show([id]);
    this.updateSelection();
  }
  async showNodeChildren(id: string) {
    await this.addNodeChildren(id);
    this._show(
      this.edges
        .get()
        .filter((edge) => id === edge.from)
        .map((edge) => edge.to)
    );
    this.updateSelection();
  }
  async hideNode(id: string): Promise<void> {
    this._hide([id]);
    this.updateSelection();
  }
  async hideNodeChildren(id: string): Promise<void> {
    // We compute the descendants of the specified node.
    const parentIds: string[] = [id];
    const childIds: string[] = [];
    while (parentIds.length > 0) {
      const parentId = parentIds.pop();
      const newChildIds = this.edges
        .get()
        .filter((edge) => edge.from === parentId)
        .map((edge) => edge.to);

      parentIds.push(...newChildIds);
      childIds.push(...newChildIds);
    }

    this._hide(childIds);
    this.updateSelection();
  }
  async updateNodes(
    nodes: { id: string; [key: string]: any }[]
  ): Promise<void> {
    this.nodes.update(nodes);
    this.updateSelection();
  }
}
