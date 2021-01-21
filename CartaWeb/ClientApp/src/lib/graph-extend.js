export function toVis(jgf) {
    const graph = {};

    if (!jgf || !jgf.graph)
        return graph;

    if (jgf.graph.nodes) {
        graph.nodes = Object.keys(jgf.graph.nodes).map(id => ({
            id: id,
            label: jgf.graph.nodes[id].label
        }));
    }
    if (jgf.graph.edges) {
        graph.edges = jgf.graph.edges.map((edge, index) => ({
            id: index.toString(),
            from: edge.source,
            to: edge.target
        }));
    }

    return graph;
};