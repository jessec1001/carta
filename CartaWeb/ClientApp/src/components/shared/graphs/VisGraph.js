import React, { Component, createRef } from 'react';
import { Network } from 'vis-network/standalone';
import { DataSet } from 'vis-data/standalone';
import './VisGraph.css';

export class VisGraph extends Component {
    static displayName = VisGraph.name;
    static events = [
        'click',
        'doubleClick',
        'context',
        'hold',
        'release',
        'select',
        'selectNode',
        'selectEdge',
        'deselectNode',
        'deselectEdge',
        'dragStart',
        'dragging',
        'dragEnd',
        'controlNodeDragStart',
        'controlNodeDragging',
        'controlNodeDragEnd',
        'hoverNode',
        'blurNode',
        'hoverEdge',
        'blurEdge',
        'zoom',
        'showPopup',
        'hidePopup',
        'startStabilizing',
        'stabilizationProgess',
        'stabilizationIterationsDone',
        'stabilized',
        'resize',
        'initRedraw',
        'beforeDrawing',
        'afterDrawing',
        'animationFinished',
        'configChange'
    ];

    constructor(props) {
        super(props);

        this.graphRef = createRef();
    }

    componentDidMount() {
        // Create the data from the passed in graph properties.
        let graph = this.props.graph || { nodes: [], edges: [] };
        this.data = {
            nodes: new DataSet(graph.nodes),
            edges: new DataSet(graph.edges)
        };
        // Create the network using these datasets to allow for dynamically changing the data.
        this.network = new Network(this.graphRef.current, this.data, this.props.options);

        // Register all of the events for the Vis Network.
        VisGraph.events.forEach(eventName => {
            const handlerName = 'on' + eventName.charAt(0).toUpperCase() + eventName.slice(1);
            this.network.on(eventName, (event) => {
                if (this.props[handlerName])
                    this.props[handlerName](event);
            });
        });
    }
    componentDidUpdate(prevProps) {
        // If the graph properties are not the same, we need to handle the change in data.
        if (this.props.graph !== prevProps.graph) {
            // Update the datasets.
            let graph = this.props.graph || { nodes: [], edges: [] };

            // Update nodes.
            graph.nodes
                .filter(node => this.data.nodes.get(node.id))
                .forEach(node => this.data.nodes.update(node));

            // Add and remove nodes.
            let nodeIds = graph.nodes.map(node => node.id);
            this.data.nodes.add(
                graph.nodes.filter(node => !this.data.nodes.get(node.id))
            );
            this.data.nodes.remove(
                this.data.nodes.getIds().filter(id => !nodeIds.includes(id))
            );

            // Add and remove edges.
            let edgeIds = graph.edges.map(edge => edge.id);
            this.data.edges.add(
                graph.edges.filter(edge => !this.data.edges.get(edge.id))
            );
            this.data.edges.remove(
                this.data.edges.getIds().filter(id => !edgeIds.includes(id))
            );
        }
        
        // If the graph options changed, just reset them.
        if (this.props.options !== prevProps.options) {
            this.network.setOptions(this.props.options);
        }
    }

    render() {
        return (
            <div className="graph-container" ref={this.graphRef} />
        );
    }
}