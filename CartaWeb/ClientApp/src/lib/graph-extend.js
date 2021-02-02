export function toVis(jgf) {
    const graph = {};

    if (!jgf || !jgf.graph)
        return graph;

    // Set heirarchy coloration if the graph is directed.
    if (jgf && jgf.graph && jgf.graph.nodes && jgf.graph.edges && jgf.graph.directed) {
        // Keep coloring nodes until there are none left to color.
        let uncolored = new Set(Object.keys(jgf.graph.nodes));
        while (uncolored.size > 0) {
            // Get the next key and work through it until a node is colored.
            let key = uncolored.values().next().value;
            while (key !== null) {
                // Get the parents of the current node.
                const parents = jgf.graph.edges
                    .filter(edge => key === edge.target)
                    .map(edge => edge.source);

                if (parents.length > 0) {
                    // There is a parent. (For now we assume only one.)
                    const parent = parents[0];

                    // Check if the parent has a color space.
                    if ('colorSpace' in jgf.graph.nodes[parent]) {
                        // We get the children of the parent node.
                        const children = jgf.graph.edges
                            .filter(edge => parent === edge.source)
                            .map(edge => edge.target);

                        // The parent is colored. We can color the child.
                        const colorSpace = jgf.graph.nodes[parent].colorSpace;
                        const index = children.indexOf(key);
                        const count = children.length ?? 1;
                        jgf.graph.nodes[key].colorSpace = {
                            range: colorSpace.range / count,
                            initial: colorSpace.initial + (colorSpace.range * index / count),
                            depth: colorSpace.depth + 1
                        };
                        const color = `hsl(${
                            jgf.graph.nodes[key].colorSpace.initial + jgf.graph.nodes[key].colorSpace.range / 2
                        }, ${
                            100 * (1 - Math.pow(1.5, -jgf.graph.nodes[key].colorSpace.depth))
                        }%, 50%)`;
                        const colorSelect = `hsl(${
                            jgf.graph.nodes[key].colorSpace.initial + jgf.graph.nodes[key].colorSpace.range / 2
                        }, ${
                            100 * (1 - Math.pow(1.5, -jgf.graph.nodes[key].colorSpace.depth))
                        }%, 75%)`;
                        jgf.graph.nodes[key].color = {
                            background: color,
                            border: color,
                            highlight: {
                                border: color,
                                background: colorSelect
                            },
                            ...jgf.graph.nodes[key].color
                        }
                        uncolored.delete(key);
                        key = null;
                    } else {
                        // The parent is not colored. We need to color it first.
                        key = parent;
                    }
                } else {
                    // There are no parents of this node so we set the color directly.
                    const color = 'hsl(0, 0%, 50%)';
                    const colorSelect = 'hsl(0, 0%, 75%)';
                    jgf.graph.nodes[key].color = {
                        background: color,
                        border: color,
                        highlight: {
                            border: color,
                            background: colorSelect
                        },
                        ...jgf.graph.nodes[key].color
                    };
                    jgf.graph.nodes[key].colorSpace = {
                        range: 360,
                        initial: 0,
                        depth: 0
                    };
                    uncolored.delete(key);
                    key = null;
                }
            }
        }
    }

    if (jgf.graph.nodes) {
        graph.nodes = Object.keys(jgf.graph.nodes).map((id, index) => {
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
            borderWidth: 2,
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