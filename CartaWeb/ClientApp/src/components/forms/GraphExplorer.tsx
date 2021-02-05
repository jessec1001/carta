import React, { Component } from 'react';
import { DataSet } from 'vis-data';
import { Vis, VisGraphData } from '../shared/graphs/Vis';
import { VisGraph, VisNode, VisEdge, VisProperty } from '../../lib/types/vis-format';
import './GraphExplorer.css';
import { connected } from 'process';

interface GraphExplorerProps {
    request?: string,

    onPropertiesChanged?: (properties: Record<string, Array<VisProperty>>) => void
}

interface GraphExplorerState {
    graph: VisGraphData,
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

        return {
            nodes: state.graph.nodes,
            edges: state.graph.edges
        };
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
            graph: this.prepareGraph(graph, state),
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
            const connectedIds: Array<string> = [];
            const descendantIds: Array<string> = [];

            while (descendantIds.length > 0) {
                const descendantId: string = descendantIds.pop() as string;
                const children = state.graph.edges
                    .get()
                    .filter(edge => edge.from === descendantId)
                    .map(edge => edge.to);
                children.forEach(childId => {
                    connectedIds.push(childId);
                    descendantIds.push(childId);
                });
            }

            // Remove descendant nodes and edges
            this.state.graph.nodes.remove(connectedIds);
            this.state.graph.edges.remove(
                this.state.graph.edges.get().filter(edge =>
                    !(connectedIds.includes(edge.from) || connectedIds.includes(edge.to))
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