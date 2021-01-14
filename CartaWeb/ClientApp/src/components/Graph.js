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
import { Properties } from "./Graph/Properties";

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
            <Container>
                <Row>
                    <Col xs="9">
                        <Sigma
                            settings={{defaultNodeColor: '#334', scalingMode:'inside', animationsTime:1000}}
                            onClickNode={this.handleClickNode}
                        >
                            <LoadGEXF path={'graphdata'}>
                                <RandomizeNodePositions/>
                                <RelativeSize initialSize={16}/>
                                <ForceLink worker autoStop={true} background={true} easing={"cubicInOut"} linLogMode />
                                <DragNodes />
                            </LoadGEXF>
                        </Sigma>
                    </Col>
                    <Col xs="3">
                        <Properties properties={this.state.properties}></Properties>
                    </Col>
                </Row>
            </Container>
        );
    }
}