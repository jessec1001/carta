import React, { Component } from 'react';
import { Row, Col } from 'reactstrap';
import { VisGraph } from '../shared/graphs/VisGraph';
import { PropertyList } from '../shared/properties/PropertyList';
import { Semantics } from "../forms/Semantics";
import { toVis } from '../../lib/graph-extend';
import './Graph.css';

export class Graph extends Component {
    static displayName = Graph.name;

    constructor(props) {
        super(props);
        this.state = { loading: true, properties: {}, semantics: {} };
        
        this.handleSingleClick = this.handleSingleClick.bind(this);
        this.handleDoubleClick = this.handleDoubleClick.bind(this);
        this.handleSelectNode = this.handleSelectNode.bind(this);
        this.handleSemanticsChanged = this.handleSemanticsChanged.bind(this);
    }

    handleSingleClick(event) {
        const nodes = event.nodes;
        if (nodes.length === 0) {
            // Show no properties if no nodes are selected.
            this.setState({
                properties: {}
            });
        }
    }
    handleDoubleClick(event) {
        const nodes = event.nodes;
        if (nodes.length === 1) {
            // Expand/contract the node that was double clicked on.
            // Notice that this will add the expanded property as true if the property doesn't already exist.
            const nodeId = nodes[0];
            const nodeData = this.state.data.graph.nodes[nodeId];
            nodeData.expanded = !nodeData.expanded;
            nodeData.color = nodeData.expanded ? { background: '#fff'} : {};
            
            // Add or remove child nodes based on expand/contract state.
            if (nodeData.expanded) {
                // Populate the children nodes if expanded.
                this.populateChildren(nodeId);
            } else {
                // Depopulate the children nodes if contracted.
                this.depopulateChildren(nodeId);
            }

        }
    }
    handleSelectNode(event) {
        const nodes = event.nodes;
        if (nodes.length === 1) {
            // Show properties only if exactly one node is selected.
            const nodeData = this.state.data.graph.nodes[nodes[0]];
            this.setState({
                properties: nodeData.data
            });
        } else if (nodes.length > 1) {
            // Show the combined properties if multiple nodes are selected.
            let properties = {};
            for (let k = 0; k < nodes.length; k++) {
                const node = this.state.data.graph.nodes[nodes[k]];
                Object.keys(node.data).forEach(property => {
                    if (property in properties) {
                        properties[property].occurrences++;
                    } else {
                        properties[property] = {
                            type: node.data[property].type,
                            occurrences: 1
                        };
                    }
                });
            }
            this.setState({
                properties: properties
            });
        }
    }
    handleSemanticsChanged(semantics) {
        this.setState({
            semantics: semantics
        });
    }

    componentDidMount() {
        this.requestURL = (
            this.props.location.pathname +
            this.props.location.search
        ).replace(this.props.match.path, '').substring(1);
        this.populateData();
    }

    render() {
        return (
            <Row>
                <Col xs="8">
                    <VisGraph
                        graph={this.state.vis}
                        options={{
                            ...this.state.options,
                            interaction: {
                                multiselect: true
                            }
                        }}
                        onClick={this.handleSingleClick}
                        onDoubleClick={this.handleDoubleClick}
                        onSelectNode={this.handleSelectNode}
                    />
                </Col>
                <Col xs="4">
                    <PropertyList properties={this.state.properties} semantics={this.state.semantics}>
                        <h2>Properties</h2>
                        <Semantics properties={this.state.properties} onSemanticsChanged={this.handleSemanticsChanged} />
                    </PropertyList>
                </Col>
            </Row>
        );
    }

    addQueryParameter(URL, parameter) {
        if (URL.includes('?'))
            return `${URL}&${parameter}`;
        else
            return `${URL}?${parameter}`;
    }

    async populateData() {
        const response = await fetch(`api/data/${this.requestURL}`);
        const data = await response.json();
        const vis = toVis(data);
        
        this.setState({
            data: data,
            vis: vis.graph,
            options: vis.options,
            loading: false
        });
    }

    async populateChildren(id) {
        const response = await fetch(`api/data/children/${this.addQueryParameter(this.requestURL, `uuid=${id}`)}`);
        const data = await response.json();
        
        const newEdges = Object.keys(data).map(childId => ({
            source: id,
            target: childId
        }));

        const combinedNodes = { ...this.state.data.graph.nodes, ...data };
        const combinedEdges = [ ...this.state.data.graph.edges, ...newEdges ];
        const newData = { ...this.state.data,
            graph: { ...this.state.data.graph,
                nodes: combinedNodes,
                edges: combinedEdges
            }
        };

        const vis = toVis(newData);
        this.setState({
            data: newData,
            vis: vis.graph,
            options: vis.options
        });
    }

    async depopulateChildren(id) {
        const scanIds = [ id ];
        const descendants = new Set();
        while (scanIds.length) {
            // Current node ID to scan.
            const scanId = scanIds.pop();

            // Find descendants.
            for (let k = 0; k < this.state.data.graph.edges.length; k++) {
                const edge = this.state.data.graph.edges[k];
                if (edge.source === scanId)
                {
                    descendants.add(edge.target);
                    scanIds.push(edge.target);
                }
            }
        }

        const filteredNodes = Object.keys(this.state.data.graph.nodes)
            .filter(nodeId => !descendants.has(nodeId))
            .reduce((obj, nodeId) => {
                return {
                    ...obj,
                    [nodeId]: this.state.data.graph.nodes[nodeId]
                };
            }, {});
        const filteredEdges = this.state.data.graph.edges
            .filter(edge => !descendants.has(edge.target));
        const newData = { ...this.state.data,
            graph: { ...this.state.data.graph,
                nodes: filteredNodes,
                edges: filteredEdges
            }
        };

        const vis = toVis(newData);
        this.setState({
            data: newData,
            vis: vis.graph,
            options: vis.options
        });
    }
}