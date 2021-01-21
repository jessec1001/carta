import React, { Component } from 'react';
import { Row, Col } from 'reactstrap';
import { VisGraph } from './Graph/VisGraph';
import { PropertyList } from './Graph/PropertyList';
import { toVis } from '../lib/graph-extend';
import './Graph.css';

export class Graph extends Component {
    static displayName = Graph.name;

    constructor(props) {
        super(props);
        this.state = { loading: true, properties: {} };
        
        this.handleSelectNode = this.handleSelectNode.bind(this);
    }

    handleSelectNode(event) {
        const nodes = event.nodes;
        if (nodes.length === 0)
            return;

        const node = nodes[0];
        this.setState({
            properties: this.state.data.graph.nodes[node].data
        });
    }

    componentDidMount() {
        this.populateData();
    }

    render() {
        return (
            <Row>
                <Col xs="8">
                    <VisGraph graph={this.state.vis} options={{}} onSelectNode={this.handleSelectNode} />
                </Col>
                <Col xs="4">
                    <PropertyList properties={this.state.properties}></PropertyList>
                </Col>
            </Row>
        );
    }

    async populateData() {
        const response = await fetch('api/data/synthetic');
        const data = await response.json();
        this.setState({ data: data, vis: toVis(data), loading: false });
    }
}