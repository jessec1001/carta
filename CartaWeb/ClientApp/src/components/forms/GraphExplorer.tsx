import React, { Component } from 'react';
import { DataSet } from 'vis-data';
import { Id } from 'vis-data/declarations/data-interface';
import { Options } from 'vis-network/standalone';
import { Vis, VisGraphData } from '../shared/graph/Vis';
import { VisGraph, VisNode, VisEdge, VisProperty } from '../../lib/types/vis-format';

interface GraphExplorerBatch {
    size: number,
    active: number,
    queue: Array<string>
}

interface GraphExplorerProps {
    request?: string,
    selection: Array<string> | string,

    onPropertiesChanged?: (properties: Array<VisProperty>) => void,
    onSelection?: (selection: Array<string>) => void
}

interface GraphExplorerState {
    directed: boolean,
    graphData: VisGraphData,
    options: Options,
    loadCount: number,
    bufferData: VisGraphData
}

export class GraphExplorer extends Component<GraphExplorerProps, GraphExplorerState> {
    static displayName = GraphExplorer.name;

    loadBatch: GraphExplorerBatch
    expandQueue: Array<string>

    constructor(props : GraphExplorerProps) {
        super(props);
        this.state = {
            directed: false,
            graphData: {
                nodes: new DataSet<VisNode>(),
                edges: new DataSet<VisEdge>()
            },
            bufferData: {
                nodes: new DataSet<VisNode>(),
                edges: new DataSet<VisEdge>()
            },
            options: this.getDefaultOptions(),
            loadCount: 0
        };

        this.loadBatch = {
            size: 16,
            active: 0,
            queue: []
        };
        this.expandQueue = [];
        
        this.handleDoubleClick = this.handleDoubleClick.bind(this);
        this.handleSelectNode = this.handleSelectNode.bind(this);
        this.handleDeselectNode = this.handleDeselectNode.bind(this);
    }

    getDefaultOptions() {
        return {
            nodes: {
                borderWidth: 2,
                shape: 'dot',
                size: 15
            },
            edges: {},
            interaction: {
                multiselect: true
            }
        };
    }

    collectProperties(nodeIds: Array<string>) {
        // Collect all the observation per property from all of the nodes.
        let properties : Array<VisProperty> = [];
        nodeIds
            .map(id => this.state.graphData.nodes.get(id))
            .forEach(node => {
                node = node as VisNode;
                if (node.properties) {
                    node.properties.forEach(nodeProp => {
                        let property = properties.find(prop => nodeProp.id === prop.id);
                        if (!property) {
                            property = {
                                id: nodeProp.id,
                                observations: []
                            };
                            properties.push(property);
                        }
                        property.observations.push(...nodeProp.observations);
                    });
                }
            });

        // Set the properties in the state.
        if (this.props.onPropertiesChanged) this.props.onPropertiesChanged(properties);
    }

    handleDoubleClick(event: any) {
        const nodesIds : Array<string> = event.nodes;
        nodesIds
            .map(id => this.state.graphData.nodes.get(id))
            .forEach(node => {
                // Expand/contract the node that was double clicked on.
                // Notice that this will add the expanded property as true if the property doesn't already exist.
                node = node as VisNode;
                node.expanded = !node.expanded;
                node.color = node.expanded ? {
                    background: '#fff',
                    highlight: {
                        background: '#fff'
                    }
                } : {};

                // Add or remove child nodes based on expand/contract state.
                if (this.props.request) {
                    if (node.expanded) {
                        // Populate the children nodes if expanded.
                        this.expandNode(this.props.request, node.id);
                    } else {
                        // Depopulate the children nodes if collapsed.
                        this.collapseNode(node.id);
                    }
                }
            });
    }
    handleSelectNode(event: any) {
        this.collectProperties(event.nodes);
        if (this.props.onSelection)
            this.props.onSelection(event.nodes);
    }
    handleDeselectNode(event: any) {
        this.collectProperties(event.nodes);
        if (this.props.onSelection)
            this.props.onSelection(event.nodes);
    }

    componentDidMount() {
        if (this.props.request) this.fetchGraph(this.props.request);
    }
    componentDidUpdate(prevProps : GraphExplorerProps) {
        if (this.props.request !== prevProps.request) {
            if (this.props.request) this.fetchGraph(this.props.request);
        }

        if (this.props.selection !== prevProps.selection) {
            if (typeof this.props.selection === "string") {
                if (this.props.onSelection) {
                    let selected;
                    switch (this.props.selection) {
                        case "expanded":
                            selected = this.state.graphData.nodes
                                .get()
                                .filter(node => node.expanded)
                                .map(node => node.id);
                            this.props.onSelection(selected);
                            break;
                        case "collapsed":
                            selected = this.state.graphData.nodes
                                .get()
                                .filter(node => !node.expanded)
                                .map(node => node.id);
                            this.props.onSelection(selected);
                            break;
                    }
                }
            } else {
                if (this.props.selection && prevProps.selection) {
                    // Check that the selection is actually different.
                    let different = false;
                    for (let k = 0; k < this.props.selection.length; k++) {
                        if (this.props.selection[k] !== prevProps.selection[k]) {
                            different = true;
                            break;
                        }
                    }
                    if (different) this.collectProperties(this.props.selection);
                }
                if (this.props.selection) this.collectProperties(this.props.selection);
                else this.collectProperties([]);
            }
        }
    }

    render() {
        return (
            <Vis
                graph={this.state.graphData}
                options={this.state.options}
                selection={this.props.selection}
                onDoubleClick={this.handleDoubleClick}
                onSelectNode={this.handleSelectNode}
                onDeselectNode={this.handleDeselectNode}
            />
        );
    }

    colorGraph(graph: VisGraphData, ids: Array<Id> | undefined = undefined) {
        // We need to assign a colorspace to any node that does not have one.
        // Keep colorings nodes until none are left uncolored.
        let uncoloredIds = graph.nodes.get()
            .filter(node => !node.colorspace)
            .map(node => node.id);
        while (uncoloredIds.length > 0) {
            // Get the next uncolored node and work thought it until a node is colored.
            let uncoloredId: string | null = uncoloredIds[0] as string;
            while (uncoloredId) {
                // Get the parents of the uncolored node.
                const parentIds = graph.edges
                    .get()
                    .filter(edge => edge.to === uncoloredId) // eslint-disable-line no-loop-func
                    .map(edge => edge.from);

                if (parentIds.length > 0) {
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
                            // Check if child already had color set.
                            if (uncoloredIds.indexOf(childId) < 0) return;

                            // Calculate the partitions of child color space.
                            const childColorspace: [number, number] = [
                                parentColorspace[0] + partitionSize * index,
                                parentColorspace[0] + partitionSize * (index + 1)
                            ];
                            graph.nodes.update({
                                id: childId,
                                colorspace: childColorspace
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
                    graph.nodes.update({
                        id: uncoloredId,
                        colorspace: colorspace
                    });

                    // Remove this node from being uncolored.
                    uncoloredIds.splice(uncoloredIds.indexOf(uncoloredId), 1);
                    uncoloredId = null;
                }
            }
        }

        // Actually set the color styles on the nodes that need colored.
        let nodes = ids ?
            graph.nodes.get(ids) :
            graph.nodes.get();
        nodes.forEach(node => {
            // Get the colorspace and use it to color the nodes.
            if (!node.colorspace) return;
            const colorspace = node.colorspace;

            // Set the new coloration.
            const hue = (colorspace[0] + colorspace[1]) / 2;
            const sat = (colorspace[1] - colorspace[0] === 360) ? 0 : 100;

            const color = `hsl(${hue}, ${sat}%, 50%)`;
            const colorSelect = `hsl(${hue}, ${sat}%, 75%)`;
            
            const colorOptions = node.color as object;
            
            graph.nodes.update({
                id: node.id,
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
        });
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

    updateData(graph: VisGraph, bufferData: VisGraphData, getChildren: boolean = true) {
        // Update the nodes and edges data.
        bufferData.nodes.update(graph.nodes);
        bufferData.edges.update(graph.edges);

        // Check for nodes that are loaded that do not have children.
        if (getChildren) {
            const unloadedNodes = bufferData.nodes.get().filter(node => !node.loaded);
            unloadedNodes.forEach(node => {
                this.fetchChildren(this.props.request as string, node.id, bufferData);
            });
        }

        // Color the data.
        if (graph.directed) this.colorGraph(bufferData);
    }

    async fetchGraph(request: string) {
        // We are loading data.
        this.setState(state => ({
            loadCount: state.loadCount + 1
        }));

        // Grab the data from the server.
        const response = await fetch(`api/data/${request}`);
        const graph : VisGraph = await response.json();
        
        // Update the graph data.
        this.updateData(graph, this.state.bufferData, true);
        this.state.graphData.nodes.add(this.state.bufferData.nodes.get());
        this.state.graphData.edges.add(this.state.bufferData.edges.get());
        
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
                directed: graph.directed,
                options: options,
                loadCount: state.loadCount - 1
            }
        });
    }
    async fetchChildren(request: string, id: string | null, bufferData: VisGraphData, force: boolean = false) {
        // We are loading data.
        this.setState(state => ({
            loadCount: state.loadCount + 1
        }));

        // Add the requested ID to the buffer (if not null).
        if (id) {
            const index = this.loadBatch.queue.indexOf(id) as number;
            if (index >= 0) this.loadBatch.queue.splice(index, 1);
            this.loadBatch.queue.push(id);
        }

        // Try to get an ID that was requesting if we have active batches or are forced.
        let fetchId : string;
        if ((this.loadBatch.active < this.loadBatch.size && this.loadBatch.queue.length > 0) || force) {
            fetchId = this.loadBatch.queue.pop() as string;
            this.loadBatch.active++;
        } else return;

        // Grab the data from the server.
        const route = this.appendQueryParam(this.appendRoute(request, 'children'), 'id', fetchId);
        const response = await fetch(`api/data/${route}`);
        const graph : VisGraph = await response.json();

        // Update the nodes and edges data.
        bufferData.nodes.update({ id: fetchId, loaded: true });
        this.updateData(graph, bufferData, false);
        this.setState(state => ({
            loadCount: state.loadCount - 1
        }));

        // Recall the method and expand any queued nodes. once the active load count goes down.
        this.loadBatch.active--;
        this.fetchChildren(request, null, bufferData);
        this.expandNode(request, null);
    }
    async expandNode(request: string, id: string | null) {
        if (!id) {
            if (this.expandQueue.length > 0) id = this.expandQueue.pop() as string;
            else return;            
        }

        const parent = this.state.bufferData.nodes.get(id);
        if (parent)
        {
            if (parent.loaded)
            {
                // Find the unadded edges and nodes.
                const unaddedNodeEdges = this.state.bufferData.edges
                    .get()
                    .filter(edge => edge.from === parent.id)
                    .filter(edge => !this.state.graphData.nodes.get(edge.to));
                const nodeTotalIds = this.state.graphData.nodes.getIds();
                const nodeIds = unaddedNodeEdges.map(edge => edge.to);
                const edgeIds = unaddedNodeEdges.map(edge => edge.id);
                const unaddedNodes = this.state.bufferData.nodes.get(nodeIds);
                const unaddedEdges = this.state.bufferData.edges.get(edgeIds);
                const unaddedSiblingEdges = this.state.bufferData.edges.get(
                    this.state.bufferData.edges
                        .get()
                        .filter(edge => nodeIds.includes(edge.from))
                        .filter(edge => nodeTotalIds.includes(edge.to))
                        .map(edge => edge.id)
                );

                // Add the nodes and edges to the graph data.
                this.state.graphData.nodes.update(unaddedNodes);
                this.state.graphData.edges.update(unaddedEdges);
                this.state.graphData.edges.update(unaddedSiblingEdges);

                // Start fetching children of newly added nodes.
                unaddedNodes.forEach(node => {
                    if (!node.loaded) {
                        this.fetchChildren(request, node.id, this.state.bufferData);
                    }
                });
            } else {
                // Force preempting the current parent if not loaded.
                this.expandQueue.push(id);
                this.fetchChildren(request, id, this.state.bufferData, true);
            }
        }

        // Recolor the parent node.
        if (this.state.directed) this.colorGraph(this.state.graphData, [id]);
    }
    async collapseNode(id: string) {
        // Obtain all of the descendents of the selected node.
        const descendantIds: Array<string> = [];
        const openIds: Array<string> = [id];

        while (openIds.length > 0) {
            const descendantId: string = openIds.pop() as string;
            const children = this.state.graphData.edges
                .get()
                .filter(edge => edge.from === descendantId)
                .map(edge => edge.to);
            children.forEach(childId => {
                descendantIds.push(childId);
                openIds.push(childId);
            });
        }

        // Remove descendant nodes and edges.
        this.state.graphData.nodes.remove(descendantIds);
        this.state.graphData.edges.remove(
            this.state.graphData.edges.get().filter(edge =>
                descendantIds.includes(edge.from)
            )
        );

        // Recolor the parent node.
        if (this.state.directed) this.colorGraph(this.state.graphData, [id]);
    }
}