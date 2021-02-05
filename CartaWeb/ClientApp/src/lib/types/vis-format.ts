import { Edge, Node } from "vis-network/standalone";

/** Represents a graph property of a graph node. */
export interface VisProperty {
    /** The name of the property. */
    name: string,
    /** The value type of the property. */
    type: string,
    /** The value assigned to the property. */
    value: unknown
}

/** Represents a graph node that can be directly imported into Vis.js. */
export interface VisNode extends Node {
    /** The unique identifier of this node. */
    id: string,

    /** The label of the node which appears under each node when visualized. */
    label?: string,
    /** The description of the node which appears when each node is hovered over as a popup. */
    title?: string,

    /** The expanded state of the node. */
    expanded?: boolean

    /** The properties that this node is assigned. */
    properties: Array<VisProperty>
}

/** Represents a graph edge that can be directly imported into Vis.js. */
export interface VisEdge extends Edge {
    /** The unique identifier of this edge. */
    id: number,
    
    /** The unique identifier for the start node of this edge. This order is irrelevant if the graph is not directed. */
    from: string,
    /** The unique identifier for the terminal node of this edge. This order is irrelevant if the graph is not directed. */
    to: string,
}

/** Represents graph data that can be directly imported into Vis.js. */
export interface VisGraph {
    /** Whether or not the edges of the graph are directed. */
    directed: boolean,

    /** The nodes contained in the graph. */
    nodes: Array<VisNode>,
    /** The edges contained in the graph. */
    edges: Array<VisEdge>
}