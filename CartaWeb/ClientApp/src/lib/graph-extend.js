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
            if (!node.title)
                delete node.title;
            delete node.data;
            return node;
        });
    }
    if (jgf.graph.edges) {
        graph.edges = jgf.graph.edges.map((edge, index) => ({
            id: index,
            from: edge.source,
            to: edge.target
        }));
    }

    let options = {
        nodes: {
            shape: 'dot',
            size: 15
        },
        edges: {}
    };
    if (jgf.graph.directed)
        options.edges = {
            arrows: 'to'
        };

    return {
        graph: graph,
        options: options
    };
};