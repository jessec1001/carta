import React, { Component } from 'react';
import { Row, Col } from 'reactstrap';
import { VisGraph } from './Graph/VisGraph';
import { PropertyList } from './Graph/PropertyList';
import { Semantics } from "./Graph/Semantics";
import { toVis } from '../lib/graph-extend';
import './Graph.css';

export class Graph extends Component {
    static displayName = Graph.name;

    constructor(props) {
        super(props);
        this.state = { loading: true, properties: {}, attributes: {}, semantics: {} };
        
        this.handleSingleClick = this.handleSingleClick.bind(this);
        this.handleDoubleClick = this.handleDoubleClick.bind(this);
        this.handleSemanticsChanged = this.handleSemanticsChanged.bind(this);
    }

    handleSingleClick(event) {
        const nodes = event.nodes;
        if (nodes.length === 0) {
            // Show no properties if no nodes are selected.
            this.setState({
                properties: {}
            });
        } else if (nodes.length === 1) {
            // Show properties only if exactly one node is selected.
            const nodeData = this.state.data.graph.nodes[nodes[0]];
            this.setState({
                properties: nodeData.data
            });
        } else {
            // Show no properties if multiple nodes are selected.
            this.setState({
                properties: {}
            });
        }

        // Set the combined attributes of all the nodes.
        let attributes = {};
        for (let k = 0; k < nodes.length; k++) {
            const node = this.state.data.graph.nodes[nodes[k]];
            attributes = {
                ...attributes,
                ...Object.keys(node.data)
                    .reduce((obj, key) => {
                        return {
                            ...obj,
                            [key]: { type: node.data[key].type }
                        };
                    }, {})
            };
        }
        this.setState({
            attributes: attributes
        });
    }
    handleDoubleClick(event) {
        const nodes = event.nodes;
        if (nodes.length === 1) {
            // Expand/contract the node that was double clicked on.
            // Notice that this will add the expanded property as true if the property doesn't already exist.
            const nodeId = nodes[0];
            const nodeData = this.state.data.graph.nodes[nodeId];
            nodeData.expanded = !nodeData.expanded;
            nodeData.color = nodeData.expanded ? { background: '#FFFFFF'} : {};

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
    handleSemanticsChanged(semantics) {
        this.setState({
            semantics: semantics
        });
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        const propertyListProps = Object.keys(this.state.properties).length > 0 ?
            this.state.properties : this.state.attributes;
        const propertyListName = Object.keys(this.state.properties).length > 0 ?
            "Properties" : "Attributes";

        return (
            <Row>
                <Col xs="8">
                    <VisGraph
                        graph={this.state.vis}
                        options={{
                            interaction: {
                                multiselect: true
                            }
                        }}
                        onClick={this.handleSingleClick}
                        onDoubleClick={this.handleDoubleClick}
                    />
                </Col>
                <Col xs="4">
                    <PropertyList properties={propertyListProps} semantics={this.state.semantics}>
                        <h2>{propertyListName}</h2>
                        <Semantics attributes={this.state.attributes} onSemanticsChanged={this.handleSemanticsChanged} />
                    </PropertyList>
                </Col>
            </Row>
        );
    }

    async populateData() {
        const response = await fetch('api/data/synthetic');
        const data = await response.json();
        this.setState({ data: data, vis: toVis(data), loading: false });
    }

    async populateChildren(id) {
        const response = await fetch(`api/data/synthetic/children?id=${id}`);
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

        this.setState({
            data: newData,
            vis: toVis(newData)
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

        this.setState({
            data: newData,
            vis: toVis(newData)
        });
    }
}