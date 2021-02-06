/** Represents a graph property of a graph node. */
export interface JgProperty {
    /** The value type of the property. */
    type: string,
    /** The value assigned to the property. */
    value: unknown
}

/** Represents a graph node. */
export interface JgNode {
    /** The label of the node which appears under each node when visualized. */
    label?: string,
    /** The description of the node which appears when each node is hovered over as a popup. */
    title?: string,

    /** The metadata containing property values that this node is assigned. */
    metadata: Record<string, JgProperty>
}

/** Represents a graph edge. */
export interface JgEdge {
    /** The unique identifier for the start node of this edge. This order is irrelevant if the graph is not directed. */
    source: string,
    /** The unique identifier for the terminal node of this edge. This order is irrelevant if the graph is not directed. */
    target: string
}

/** Represents a graph. */
export interface JgGraph {
    /** Whether or not the edges of the graph are directed. */
    directed: boolean,

    /** The nodes contained in the graph. */
    nodes: Record<string, JgNode>,
    /** The edges contained in the graph. */
    edges: Array<JgEdge>
}

/** Represents data that may contain a single graph or multiple graphs. */
export interface JgData {
    /** Data for a single graph. */
    graph?: JgGraph,
    /** Data for multiple graphs. */
    graphs?: Array<JgGraph>
}