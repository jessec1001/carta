import React, { Component, createRef, RefObject } from 'react';
import { Network, NetworkEvents, Options } from 'vis-network/standalone';
import { DataSet } from 'vis-data/standalone';
import { VisGraph, VisEdge, VisNode } from '../../../lib/types/vis-format';
import './Vis.css';

interface VisGraphData {
    nodes: DataSet<VisNode>,
    edges: DataSet<VisEdge>
};

interface VisProps {
    graph: VisGraph,
    options: Options,

    onClick?:                        (params?: any) => void,
    onDoubleClick?:                  (params?: any) => void,
    onContext?:                      (params?: any) => void,
    onHold?:                         (params?: any) => void,
    onRelease?:                      (params?: any) => void,
    onSelect?:                       (params?: any) => void,
    onSelectNode?:                   (params?: any) => void,
    onSelectEdge?:                   (params?: any) => void,
    onDeselectNode?:                 (params?: any) => void,
    onDeselectEdge?:                 (params?: any) => void,
    onDragStart?:                    (params?: any) => void,
    onDragging?:                     (params?: any) => void,
    onDragEnd?:                      (params?: any) => void,
    onControlNodeDragging?:          (params?: any) => void,
    onControlNodeDragEnd?:           (params?: any) => void,
    onHoverNode?:                    (params?: any) => void,
    onBlurNode?:                     (params?: any) => void,
    onHoverEdge?:                    (params?: any) => void,
    onBlurEdge?:                     (params?: any) => void,
    onZoom?:                         (params?: any) => void,
    onShowPopup?:                    (params?: any) => void,
    onHidePopup?:                    (params?: any) => void,
    onStartStabilizing?:             (params?: any) => void,
    onStabilizationProgress?:        (params?: any) => void,
    onStabilizationIterationsDone?:  (params?: any) => void,
    onStabilized?:                   (params?: any) => void,
    onResize?:                       (params?: any) => void,
    onInitRedraw?:                   (params?: any) => void,
    onBeforeDrawing?:                (params?: any) => void,
    onAfterDrawing?:                 (params?: any) => void,
    onAnimationFinished?:            (params?: any) => void,
    onConfigChange?:                 (params?: any) => void
};

export class Vis extends Component<VisProps> {
    static displayName = Vis.name;

    ref : RefObject<HTMLDivElement>;
    data : VisGraphData;
    network: Network | null;

    constructor(props : VisProps) {
        super(props);

        this.ref = createRef<HTMLDivElement>();
        this.data = {
            nodes: new DataSet(this.props.graph.nodes),
            edges: new DataSet(this.props.graph.edges)
        };
        this.network = null;
    }

    registerEvents() {
        if (!this.network) return;

        if (this.props.onClick)
			this.network.on('click', this.props.onClick);
        if (this.props.onDoubleClick)
			this.network.on('doubleClick', this.props.onDoubleClick);
        if (this.props.onContext)
			this.network.on('oncontext', this.props.onContext);
        if (this.props.onHold)
			this.network.on('hold', this.props.onHold);
        if (this.props.onRelease)
			this.network.on('release', this.props.onRelease);
        if (this.props.onSelect)
			this.network.on('select', this.props.onSelect);
        if (this.props.onSelectNode)
			this.network.on('selectNode', this.props.onSelectNode);
        if (this.props.onSelectEdge)
			this.network.on('selectEdge', this.props.onSelectEdge);
        if (this.props.onDeselectNode)
			this.network.on('deselectNode', this.props.onDeselectNode);
        if (this.props.onDeselectEdge)
			this.network.on('deselectEdge', this.props.onDeselectEdge);
        if (this.props.onDragStart)
			this.network.on('dragStart', this.props.onDragStart);
        if (this.props.onDragging)
			this.network.on('dragging', this.props.onDragging);
        if (this.props.onDragEnd)
			this.network.on('dragEnd', this.props.onDragEnd);
        if (this.props.onControlNodeDragging)
			this.network.on('controlNodeDragging', this.props.onControlNodeDragging);
        if (this.props.onControlNodeDragEnd)
			this.network.on('controlNodeDragEnd', this.props.onControlNodeDragEnd);
        if (this.props.onHoverNode)
			this.network.on('hoverNode', this.props.onHoverNode);
        if (this.props.onBlurNode)
			this.network.on('blurNode', this.props.onBlurNode);
        if (this.props.onHoverEdge)
			this.network.on('hoverEdge', this.props.onHoverEdge);
        if (this.props.onBlurEdge)
			this.network.on('blurEdge', this.props.onBlurEdge);
        if (this.props.onZoom)
			this.network.on('zoom', this.props.onZoom);
        if (this.props.onShowPopup)
			this.network.on('showPopup', this.props.onShowPopup);
        if (this.props.onHidePopup)
			this.network.on('hidePopup', this.props.onHidePopup);
        if (this.props.onStartStabilizing)
			this.network.on('startStabilizing', this.props.onStartStabilizing);
        if (this.props.onStabilizationProgress)
			this.network.on('stabilizationProgress', this.props.onStabilizationProgress);
        if (this.props.onStabilizationIterationsDone)
			this.network.on('stabilizationIterationsDone', this.props.onStabilizationIterationsDone);
        if (this.props.onStabilized)
			this.network.on('stabilized', this.props.onStabilized);
        if (this.props.onResize)
			this.network.on('resize', this.props.onResize);
        if (this.props.onInitRedraw)
			this.network.on('initRedraw', this.props.onInitRedraw);
        if (this.props.onBeforeDrawing)
			this.network.on('beforeDrawing', this.props.onBeforeDrawing);
        if (this.props.onAfterDrawing)
			this.network.on('afterDrawing', this.props.onAfterDrawing);
        if (this.props.onAnimationFinished)
			this.network.on('animationFinished', this.props.onAnimationFinished);
        if (this.props.onConfigChange)
			this.network.on('configChange', this.props.onConfigChange);
    }

    componentDidMount() {
        // Create the data from the passed in graph properties.
        let graph = this.props.graph;
        this.data = {
            nodes: new DataSet(graph.nodes),
            edges: new DataSet(graph.edges)
        };

        // Create the network using these datasets to allow for dynamically changing the data.
        if (this.ref.current) {
            this.network = new Network(this.ref.current, this.data, this.props.options);
            this.registerEvents();
        }
    }
    componentDidUpdate(prevProps : VisProps) {
        // If the graph properties are not the same, we need to handle the change in data.
        if (this.props.graph !== prevProps.graph) {
            // Update the nodes and edges data.
            this.data.nodes.update(this.props.graph.nodes);
            this.data.edges.update(this.props.graph.edges);

            // Remove any excess nodes and edges left behind.
            let nodeIds = this.props.graph.nodes.map(node => node.id);
            let edgeIds = this.props.graph.edges.map(edge => edge.id);
            this.data.nodes.remove(
                this.data.nodes.getIds().filter(id => !nodeIds.includes(id as string))
            );
            this.data.nodes.remove(
                this.data.edges.getIds().filter(id => !edgeIds.includes(id as number))
            );
        }
        
        // If the graph options changed, just reset them.
        if (this.props.options !== prevProps.options) {
            if (this.network) {
                this.network.setOptions(this.props.options);
            }
        }
    }

    render() {
        return (
            <div className="graph-container" ref={this.ref} />
        );
    }
}