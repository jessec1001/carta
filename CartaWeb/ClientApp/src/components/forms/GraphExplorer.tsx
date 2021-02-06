import React, { Component } from 'react';
import { DataSet } from 'vis-data';
import { Options } from 'vis-network/standalone';
import { Vis, VisGraphData } from '../shared/graph/Vis';
import { VisGraph, VisNode, VisEdge, VisProperty } from '../../lib/types/vis-format';

interface GraphExplorerProps {
    request?: string,

    onPropertiesChanged?: (properties: Record<string, Array<VisProperty>>) => void
}

interface GraphExplorerState {
    graph: VisGraphData,
    options: Options
    loading: boolean
}

export class GraphExplorer extends Component<GraphExplorerProps, GraphExplorerState> {
    static displayName = GraphExplorer.name;

    constructor(props : GraphExplorerProps) {
        super(props);
        this.state = {
            graph: {
                nodes: new DataSet<VisNode>(),
                edges: new DataSet<VisEdge>()
            },
            options: {
                nodes: {
                    borderWidth: 2,
                    shape: 'dot',
                    size: 15
                },
                edges: {},
                interaction: {
                    multiselect: true
                }
            },
            loading: false
        };
        
        this.handleDoubleClick = this.handleDoubleClick.bind(this);
        this.handleSelectNode = this.handleSelectNode.bind(this);
        this.handleDeselectNode = this.handleDeselectNode.bind(this);
    }

    collectProperties(nodeIds: Array<string>) {
        // Collect all the properties from all of the nodes and organize them by name.
        let properties : Record<string, Array<VisProperty>> = {};
        nodeIds
            .map(id => this.state.graph.nodes.get(id))
            .forEach(node => {
                node = node as VisNode;
                node.properties.forEach(property => {
                    if (!(property.name in properties)) {
                        properties[property.name] = [];
                    }
                    properties[property.name].push(property);
                });
            });

        // Set the properties in the state.
        if (this.props.onPropertiesChanged) this.props.onPropertiesChanged(properties);
    }

    handleDoubleClick(event: any) {
        const nodesIds : Array<string> = event.nodes;
        nodesIds
            .map(id => this.state.graph.nodes.get(id))
            .forEach(node => {
                // Expand/contract the node that was double clicked on.
                // Notice that this will add the expanded property as true if the property doesn't already exist.
                node = node as VisNode;
                node.expanded = !node.expanded;
                node.color = node.expanded ? { background: '#fff'} : {};

                // Add or remove child nodes based on expand/contract state.
                if (this.props.request) {
                    if (node.expanded) {
                        // Populate the children nodes if expanded.
                        this.populateChildren(this.props.request, node.id);
                    } else {
                        // Depopulate the children nodes if contracted.
                        this.depopulateChildren(node.id);
                    }
                }
            });
    }
    handleSelectNode(event: any) {
        this.collectProperties(event.nodes);
    }
    handleDeselectNode(event: any) {
        this.collectProperties(event.nodes);
    }

    componentDidMount() {
        if (this.props.request) this.populateData(this.props.request);
    }
    componentDidUpdate(prevProps : GraphExplorerProps) {
        if (this.props.request !== prevProps.request) {
            if (this.props.request) this.populateData(this.props.request);
        }
    }

    render() {
        return (
            <Vis
                graph={this.state.graph}
                options={this.state.options}
                onDoubleClick={this.handleDoubleClick}
                onSelectNode={this.handleSelectNode}
                onDeselectNode={this.handleDeselectNode}
            />
        );
    }

    colorGraph(graph: VisGraphData) {
        // Keep colorings nodes until none are left uncolored.
        let uncoloredIds = graph.nodes.getIds();
        while (uncoloredIds.length > 0) {
            // Get the next uncolored node and work thought it until a node is colored.
            let uncoloredId: string | null = uncoloredIds[0] as string;
            while (uncoloredId) {
                // Get the parents of the uncolored node.
                const parentIds = graph.edges
                    .get()
                    .filter(edge => edge.to === uncoloredId) // eslint-disable-line no-loop-func
                    .map(edge => edge.from);

                if (parentIds.length === 1) {
                    // There is a parent - for now we assume only one.
                    const parentId = parentIds[0];
                    const parent = graph.nodes.get(parentId);

                    // If the parent has a color space, we can color the children.
                    if (parent?.colorspace) {
                        // Get the children of the colored parent node.
                        const childrenIds = graph.edges
                            .get()
                            .filter(edge => edge.from === parentId)
                            .map(edge => edge.to);

                        // Color each child one-by-one.
                        const parentColorspace = parent.colorspace;
                        const partitionSize = (parentColorspace[1] - parentColorspace[0]) / (childrenIds.length || 1);
                        childrenIds.forEach((childId, index) => { // eslint-disable-line no-loop-func
                            // Calculate the partitions of child color space.
                            const childColorspace: [number, number] = [
                                parentColorspace[0] + partitionSize * index,
                                parentColorspace[0] + partitionSize * (index + 1)
                            ];

                            // Set the new coloration.
                            const hue = (childColorspace[0] + childColorspace[1]) / 2;

                            const color = `hsl(${hue}, 100%, 50%)`;
                            const colorSelect = `hsl(${hue}, 100%, 75%)`;
                            
                            const colorOptions = graph.nodes.get(childId)?.color as object;
                            
                            graph.nodes.update({
                                id: childId as string,
                                colorspace: childColorspace,
                                color: {
                                    background: color,
                                    border: color,
                                    highlight: {
                                        border: color,
                                        background: colorSelect
                                    },
                                    ...colorOptions
                                }
                            });

                            // Remove the child node from being uncolored.
                            uncoloredIds.splice(uncoloredIds.indexOf(childId), 1);
                            uncoloredId = null;
                        });
                    } else {
                        // The parent isn't colored so we should color it.
                        uncoloredId = parentId;
                    }
                } else if (parentIds.length === 0) {
                    // There are no parents of this node so we color it gray.
                    const colorspace: [number, number] = [0, 360];

                    // Set the new coloration.
                    const color = 'hsl(0, 0%, 50%)';
                    const colorSelect = 'hsl(0, 0%, 75%)';

                    const colorOptions = graph.nodes.get(uncoloredId)?.color as object;

                    graph.nodes.update({
                        id: uncoloredId as string,
                        colorspace: colorspace,
                        color: {
                            background: color,
                            border: color,
                            highlight: {
                                border: color,
                                background: colorSelect
                            },
                            ...colorOptions
                        }
                    });

                    // Remove this node from being uncolored.
                    uncoloredIds.splice(uncoloredIds.indexOf(uncoloredId), 1);
                    uncoloredId = null;
                }
            }
        }
    }
    prepareGraph(graph: VisGraph, state: GraphExplorerState, remove: boolean = true) {
        // We need edges to have unique IDs so we use Parent-#Child combination.
        graph.edges.forEach(edge => edge.id = `${edge.from}-${edge.id}`);

        // Update the nodes and edges data.
        state.graph.nodes.update(graph.nodes);
        state.graph.edges.update(graph.edges);

        // Remove any excess nodes and edges left behind.
        if (remove) {
            let nodeIds = graph.nodes.map(node => node.id);
            let edgeIds = graph.edges.map(edge => edge.id);
            state.graph.nodes.remove(
                state.graph.nodes.getIds().filter(id => !nodeIds.includes(id as string))
            );
            state.graph.edges.remove(
                state.graph.edges.getIds().filter(id => !edgeIds.includes(id as string))
            );
        }

        // Set the data state.
        const data: VisGraphData = {
            nodes: state.graph.nodes,
            edges: state.graph.edges
        };

        // Color the data.
        if (graph.directed) this.colorGraph(data);

        return data;
    }

    appendQueryParam(url: string, key: string, value: string) {
        // Encode the key and the value.
        key = encodeURIComponent(key);
        value = encodeURIComponent(value);
        
        // Return the appropriate response based on whether there are already query parameters.
        if (url.split('?').length > 1)
            return `${url}&${key}=${value}`;
        else
            return `${url}?${key}=${value}`;
    }
    appendRoute(url: string, route: string) {
        // Get the location to place to insert the route.
        let searchIndex = url.indexOf('?');
        searchIndex = searchIndex < 0 ? url.length : searchIndex;

        return `${url.substring(0, searchIndex)}/${route}${url.substring(searchIndex)}`;
    }

    async populateData(request: string) {
        // Set loading state to true.
        this.setState({
            loading: true
        });

        // Grab the data from the server.
        const response = await fetch(`api/data/${request}`);
        const graph : VisGraph = await response.json();

        
        // Set loading state to false and set data.
        this.setState(state => {
            // Get the options set up correctly.
            const options = {...state.options};
            if (!options.edges) options.edges = {};
            if (graph.directed)
                options.edges.arrows = 'to';
            else
                delete options.edges.arrows;
            

            return {
                graph: this.prepareGraph(graph, state),
                options: options,
                loading: false
            }
        });
    }
    async populateChildren(request: string, id: string) {
        // Set loading state to true.
        this.setState({
            loading: true
        });

        // Grab the data from the server.
        const route = this.appendQueryParam(this.appendRoute(request, 'children'), 'uuid', id);
        const response = await fetch(`api/data/${route}`);
        const graph : VisGraph = await response.json();
        
        // Set loading state to false and update data.
        // Note that we need to check for duplicate nodes and edges.
        this.setState(state => {
            return {
                graph: this.prepareGraph(graph, state, false),
                loading: false
            };
        });
    }
    async depopulateChildren(id: string) {
        // Set loading state to true.
        this.setState({
            loading: true
        });

        // Set loading state to false and update data.
        this.setState(state => {
            // Obtain all of the descendents of the selected node.
            const descendantIds: Array<string> = [];
            const openIds: Array<string> = [id];

            while (openIds.length > 0) {
                const descendantId: string = openIds.pop() as string;
                const children = state.graph.edges
                    .get()
                    .filter(edge => edge.from === descendantId)
                    .map(edge => edge.to);
                children.forEach(childId => {
                    descendantIds.push(childId);
                    openIds.push(childId);
                });
            }

            // Remove descendant nodes and edges.
            this.state.graph.nodes.remove(descendantIds);
            this.state.graph.edges.remove(
                this.state.graph.edges.get().filter(edge =>
                    descendantIds.includes(edge.from) || descendantIds.includes(edge.to)
                )
            )

            return {
                graph: {
                    nodes: this.state.graph.nodes,
                    edges: this.state.graph.edges
                },
                loading: false
            };
        });
    }
}