/** Represents a graph property of a graph node. */
interface VisProperty {
    /** The name of the property. */
    name: string,
    /** The value type of the property. */
    type: string,
    /** The value assigned to the property. */
    value: unknown
}

/** Represents a graph node that can be directly imported into Vis.js. */
interface VisNode {
    /** The unique identifier of this node. */
    id: string,

    /** The label of the node which appears under each node when visualized. */
    label?: string,
    /** The description of the node which appears when each node is hovered over as a popup. */
    description?: string,

    /** The properties that this node is assigned. */
    properties: VisProperty[]
}

/** Represents a graph edge that can be directly imported into Vis.js. */
interface VisEdge {
    /** The unique identifier of this edge. */
    id: number,
    
    /** The unique identifier for the start node of this edge. This is irrelevant if the graph is not directed. */
    from: string,
    /** The unique identifier for the terminal node of this edge. This is irrelevant if the graph is not directed. */
    to: string,
}

/** Represents graph data that can be directly imported into Vis.js. */
interface VisGraph {
    /** Whether or not the edges of the graph are directed. */
    directed: boolean,

    /** The nodes contained in the graph. */
    nodes: VisNode[],
    /** The edges contained in the graph. */
    edges: VisEdge[]
}