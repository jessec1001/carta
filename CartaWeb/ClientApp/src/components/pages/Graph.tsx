import React, { Component } from "react";
import { Row, Col } from 'reactstrap';
import { PropertyList } from "../shared/properties/PropertyList";
import { GraphExplorer } from '../forms/GraphExplorer';
import { Semantics } from '../forms/Semantics';

interface GraphProps {

}
interface GraphState {
    properties: Record<string, any>
}

export class Graph extends Component<GraphProps, GraphState> {
    constructor(props : GraphProps) {
        super(props);

        this.handleSemanticsChanged = this.handleSemanticsChanged.bind(this);
    }

    handleSemanticsChanged(semantics) {
        this.setState({
            semantics: semantics
        });
    }

    render() {
        return (
            <Row>
                <Col xs="8">
                    <GraphExplorer />
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
}