import React, { Component } from 'react';
import { Container, Row, Col } from 'reactstrap';
import {
    Sigma,
    RandomizeNodePositions,
    RelativeSize,
    LoadGEXF
} from 'react-sigma';
import ForceLink from 'react-sigma/lib/ForceLink';
import DragNodes from 'react-sigma/lib/DragNodes';
import { PropertyList } from "./Graph/PropertyList";
import './Graph.css';

export class Graph extends Component {
    static displayName = Graph.name;

    constructor(props) {
        super(props);
        this.state = { properties: {}};
        
        this.handleClickNode = this.handleClickNode.bind(this);
    }

    handleClickNode(event) {
        let node = event.data.node;
        this.setState({
            properties: node
        });
    }

    render() {
        return (
            <Row>
                <Col xs="8">
                        <Sigma
                            settings={{defaultNodeColor: '#334', scalingMode:'inside', animationsTime:1000}}
                            onClickNode={this.handleClickNode}
                            style={{ height: '100%' }}
                        >
                            <LoadGEXF path={'api/graph'}>
                                <RandomizeNodePositions/>
                                <RelativeSize initialSize={16}/>
                                <ForceLink worker autoStop={true} background={true} easing={"cubicInOut"} linLogMode />
                                <DragNodes />
                            </LoadGEXF>
                        </Sigma>
                </Col>
                <Col xs="4">
                    <PropertyList properties={this.state.properties}></PropertyList>
                </Col>
            </Row>
        );
    }
}