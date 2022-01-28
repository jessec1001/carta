/** Represents an abstract element of a graph. */
interface Element {
  /** The unique identifier of the element. */
  id: string;
  /** The properties of the element. */
  properties?: Property[];
}

/** Represents a property of a vertex. */
interface Property extends Element {
  /** The value of the property. */
  value: any;
}

/** Represents a vertex in a graph. */
interface Vertex extends Element {
  /** The label of the node which appears under each node when visualized. */
  label?: string;
  /** The description of the node which appears when each node is hovered over as a popup. */
  title?: string;
}

/** Represents an edge between vertices in a graph. */
interface Edge extends Element {
  /** The unique identifier of this edge. */
  id: string;

  /** The unique identifier for the start node of this edge. This order is irrelevant if the graph is not directed. */
  from: string;
  /** The unique identifier for the terminal node of this edge. This order is irrelevant if the graph is not directed. */
  to: string;
}

/** The properties that define how a graph functions. */
interface GraphProperties {
  /** Whether or not the edges of the graph are directed. */
  directed: boolean;
  /** Whether or not the graph is dynamic. */
  dynamic: boolean;
}

/** Represents a graph structure. */
interface Graph extends GraphProperties, Element {
  /** The vertices contained in the graph. */
  vertices: Vertex[];
  /** The edges contained in the graph. */
  edges: Edge[];
}

export type { Element, Property, Vertex, Edge, GraphProperties, Graph };
