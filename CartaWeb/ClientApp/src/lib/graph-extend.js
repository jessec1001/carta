export function toVis(jgf) {
    const graph = {};

    if (!jgf || !jgf.graph)
        return graph;

    if (jgf.graph.nodes) {
        graph.nodes = Object.keys(jgf.graph.nodes).map(id => {
            let node = {
                id: id,
                ...jgf.graph.nodes[id]
            };
            delete node.data;
            return node;
        });
    }
    if (jgf.graph.edges) {
        graph.edges = jgf.graph.edges.map((edge, index) => ({
            id: index.toString(),
            from: edge.source,
            to: edge.target,
            direction: 'to',
            arrows: 'to'
        }));
    }

    return graph;
};