import { Edge as VisEdge, Node as VisNode } from "vis-network/standalone";

/** Represents a graph observation of a graph property. */
export interface Observation {
  /** The type of the observation. */
  type: string;
  /** The value of the observation. */
  value: any;
}

/** Represents a graph property of a graph node. */
export interface Property {
  /** The id of the property. */
  id: string;

  /** The observations of the property. */
  observations: Array<Observation>;
}

/** Represents a graph node that can be directly imported into Vis.js. */
export interface Node extends VisNode {
  /** The unique identifier of this node. */
  id: string;

  /** The label of the node which appears under each node when visualized. */
  label?: string;
  /** The description of the node which appears when each node is hovered over as a popup. */
  title?: string;

  /** The properties that this node is assigned. */
  properties?: Array<Property>;
}

/** Represents a graph edge that can be directly imported into Vis.js. */
export interface Edge extends VisEdge {
  /** The unique identifier of this edge. */
  id: string;

  /** The unique identifier for the start node of this edge. This order is irrelevant if the graph is not directed. */
  from: string;
  /** The unique identifier for the terminal node of this edge. This order is irrelevant if the graph is not directed. */
  to: string;
}

/** Represents graph data that can be directly imported into Vis.js. */
export interface Graph {
  /** Whether or not the edges of the graph are directed. */
  directed: boolean;
  dynamic: boolean;

  /** The nodes contained in the graph. */
  nodes: Array<Node>;
  /** The edges contained in the graph. */
  edges: Array<Edge>;
}
