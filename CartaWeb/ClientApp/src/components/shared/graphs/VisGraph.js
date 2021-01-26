import React, { Component, createRef } from 'react';
import { Network } from 'vis-network/standalone';
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
        this.network = new Network(this.graphRef.current, this.props.graph, this.props.options);

        VisGraph.events.forEach(eventName => {
            const handlerName = 'on' + eventName.charAt(0).toUpperCase() + eventName.slice(1);
            this.network.on(eventName, (event) => {
                if (this.props[handlerName])
                    this.props[handlerName](event);
            });
        });
    }
    componentDidUpdate(prevProps) {
        if (this.props.graph !== prevProps.graph) {
            this.network.setData(this.props.graph);
        }
    }

    render() {
        return (
            <div className="graph-container" ref={this.graphRef} />
        );
    }
}