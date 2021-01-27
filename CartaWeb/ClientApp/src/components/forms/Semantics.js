import React, { Component } from 'react';
import { Input, Row, Col, Button, Modal, ModalHeader, ModalBody } from 'reactstrap';
import { PropertyList } from '../shared/properties/PropertyList';

export class Semantics extends Component {
    static displayName = Semantics.name;

    constructor(props) {
        super(props);
        this.state = {
            open: false,
            selected: null,
            semantics: {},
            sourceSearch: "",
            targetSearch: ""
        };

        this.toggleModalOpen = this.toggleModalOpen.bind(this);
        this.handleClickSourceProperty = this.handleClickSourceProperty.bind(this);
        this.handleClickTargetProperty = this.handleClickTargetProperty.bind(this);
    }

    toggleModalOpen() {
        this.setState({
            open: !this.state.open,
            selected: null,
            sourceSearch: "",
            targetSearch: ""
        });
    }
    handleClickSourceProperty(property) {
        this.setState({
            selected: property === this.state.selected ? null : property
        });
    }
    handleClickTargetProperty(property) {
        let semantics = this.state.semantics;
        if (property in this.state.semantics)
        {
            delete semantics[property];
            this.setState({
                semantics: semantics
            });
        }
        else {
            semantics = {
                ...semantics,
                [property]: this.state.selected
            }
            this.setState({
                semantics: semantics
            });
        }

        if (this.props.onSemanticsChanged)
            this.props.onSemanticsChanged(semantics);
    }

    findSourceAttributes() {
        return Object.keys(this.props.attributes)
            .filter(attribute => 
                (!(attribute in this.state.semantics) || !(this.state.semantics[attribute] in this.props.attributes)) &&
                (this.state.sourceSearch === "" || attribute.includes(this.state.sourceSearch) ||
                    (attribute in this.state.semantics && this.state.semantics[attribute].includes(this.state.sourceSearch))
                ) 
            )
            .reduce((obj, attribute) => {
                return {
                    ...obj,
                    [attribute]: this.props.attributes[attribute]
                };
            }, {});
    }
    findTargetAttributes() {
        const selected = this.state.selected;
        if (selected === null)
            return {};

        return Object.keys(this.props.attributes)
            .filter(attribute => 
                (attribute !== selected) &&
                (!(attribute in this.state.semantics) || this.state.semantics[attribute] === selected) &&
                (this.props.attributes[attribute].type === this.props.attributes[selected].type) &&
                (this.state.targetSearch === "" || attribute.includes(this.state.targetSearch)) 
            )
            .reduce((obj, attribute) => {
                return {
                    ...obj,
                    [attribute]: this.props.attributes[attribute]
                };
            }, {});
    }

    render() {
        const sourceAttributes = this.findSourceAttributes();
        const targetAttributes = this.findTargetAttributes();

        return (
            <div>
                <Button onClick={this.toggleModalOpen} color="success">Semantics</Button>
                <Modal
                    isOpen={this.state.open}
                    toggle={this.toggleModalOpen}
                    size={'lg'}
                >
                    <ModalHeader toggle={this.toggleModalOpen}>Semantics</ModalHeader>
                    <ModalBody>
                        <Row>
                            <Col xs="5">
                                <h3 className="text-center">This Property</h3>
                            </Col>
                            <Col xs="2" className="my-auto">
                                <p className="text-center text-muted">overrides</p>
                            </Col>
                            <Col xs="5">
                                <h3 className="text-center">These Properties</h3>
                            </Col>
                        </Row>
                        <Row>
                            <Col>
                                <PropertyList
                                    properties={sourceAttributes}
                                    selected={[this.state.selected]}
                                    onClickProperty={this.handleClickSourceProperty}
                                />
                            </Col>
                            <Col>
                                <PropertyList
                                    properties={targetAttributes}
                                    selected={Object.keys(this.state.semantics)}
                                    onClickProperty={this.handleClickTargetProperty}
                                />
                            </Col>
                        </Row>
                        <Row className="mt-4">
                            <Col>
                                <Input
                                    type="text"
                                    bsSize="sm"
                                    value={this.state.sourceSearch}
                                    placeholder={"Search this"}
                                    onChange={event => this.setState({ sourceSearch: event.target.value })}
                                />
                            </Col>
                            <Col>
                                <Input
                                    type="text"
                                    bsSize="sm"
                                    value={this.state.targetSearch}
                                    placeholder={"Search that"}
                                    onChange={event => this.setState({ targetSearch: event.target.value })}
                                />
                            </Col>
                        </Row>
                    </ModalBody>
                </Modal>
            </div>
        );
    }
}