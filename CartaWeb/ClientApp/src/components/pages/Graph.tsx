import React, { Component } from "react";
import { Row, Col } from 'reactstrap';
import { RouteComponentProps } from "react-router-dom";
import { PropertyList } from "../shared/properties/PropertyList";
import { GraphExplorer } from '../forms/GraphExplorer';
import { Semantics } from '../forms/Semantics';
import { VisProperty } from '../../lib/types/vis-format';

interface GraphProps extends RouteComponentProps { }
interface GraphState {
    properties: Record<string, Array<VisProperty>>,
    semantics: Record<string, string>
}

export class Graph extends Component<GraphProps, GraphState> {
    request: string;

    constructor(props : GraphProps) {
        super(props);
        
        this.request = (
            this.props.location.pathname +
            this.props.location.search
        ).replace(this.props.match.path, '').substring(1);
        this.state = {
            properties: {},
            semantics: {}
        };

        this.handleSemanticsChanged = this.handleSemanticsChanged.bind(this);
    }

    handleSemanticsChanged(semantics : Record<string, string>) {
        this.setState({
            semantics: semantics
        });
    }

    render() {
        return (
            <Row>
                <Col xs="8">
                    <GraphExplorer properties={this.state.properties} request={this.request} />
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