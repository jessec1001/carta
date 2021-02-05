import React, { Component } from 'react';
import { Vis } from '../shared/graphs/Vis';
import { VisGraph, VisProperty } from '../../lib/types/vis-format';
import './GraphExplorer.css';

interface GraphExplorerProps {
    request?: string,

    onPropertiesChanged?: (properties: Record<string, Array<VisProperty>>) => void
}

interface GraphExplorerState {
    graph?: VisGraph,
    loading: boolean
}

export class GraphExplorer extends Component<GraphExplorerProps, GraphExplorerState> {
    static displayName = GraphExplorer.name;

    constructor(props : GraphExplorerProps) {
        super(props);
        this.state = {
            loading: false
        };
        
        this.handleDoubleClick = this.handleDoubleClick.bind(this);
        this.handleSelectNode = this.handleSelectNode.bind(this);
        this.handleDeselectNode = this.handleDeselectNode.bind(this);
    }

    collectProperties(nodeIds: Array<string>) {
        // Collect all the properties from all of the nodes and organize them by name.
        let properties : Record<string, Array<VisProperty>> = {};
        this.state.graph?.nodes
            .filter(node => nodeIds.includes(node.id))
            .forEach(node => {
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
        this.state.graph?.nodes
            .filter(node => nodesIds.includes(node.id))
            .forEach(node => {
                // Expand/contract the node that was double clicked on.
                // Notice that this will add the expanded property as true if the property doesn't already exist.
                node.expanded = !node.expanded;
                node.color = node.expanded ? { background: '#FFFFFF'} : {};

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
            else this.setState({ graph: undefined });
        }
    }

    render() {
        return (
            <Vis
                graph={this.state.graph}
                options={{
                    interaction: {
                        multiselect: true
                    }
                }}
                onDoubleClick={this.handleDoubleClick}
                onSelectNode={this.handleSelectNode}
                onDeselectNode={this.handleDeselectNode}
            />
        );
    }

    prepareGraph(graph: VisGraph) {
        // We need edges to have unique IDs so we use Parent-#Child combination.
        graph.edges.forEach(edge => edge.id = `${edge.from}-${edge.to}`);

        return graph;
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
        this.setState(state => ({
            graph: this.prepareGraph(graph),
            loading: false
        }));
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
            if (state.graph) {
                graph.nodes = [
                    ...state.graph.nodes,
                    ...graph.nodes
                        .filter(nodeA =>
                            !state.graph?.nodes?.some(nodeB => nodeA.id === nodeB.id)
                        )
                    ];
                graph.edges = [
                    ...state.graph.edges,
                    ...graph.edges
                        .filter(edge => graph.nodes.some(node => edge.to === node.id))
                        .filter(edgeA => 
                            !state.graph?.edges?.some(edgeB => edgeA.from === edgeB.from && edgeA.to === edgeB.to)
                        )
                ];
            }

            return {
                graph: this.prepareGraph(graph),
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
            if (!state.graph) return { loading: false };

            // Obtain all of the descendents of the selected node.
            const connectedIds = new Set<string>();
            const descendantIds = new Set<string>([id]);

            while (descendantIds.size > 0) {
                const descendantId : string = descendantIds.values().next().value;
                const children = state.graph.edges
                    .filter(edge => edge.from === descendantId)
                    .map(edge => edge.to);
                children.forEach(childId => {
                    connectedIds.add(childId);
                    descendantIds.add(childId);
                });

                descendantIds.delete(descendantId);
            }

            // Remove descendant nodes and edges
            const graph = state.graph;
            graph.nodes = graph.nodes.filter(node => !connectedIds.has(node.id));
            graph.edges = graph.edges.filter(edge => !(connectedIds.has(edge.from) || connectedIds.has(edge.to)));

            return {
                graph: graph,
                loading: false
            };
        });
    }
}