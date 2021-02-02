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

    findSourceProperties() {
        return Object.keys(this.props.properties)
            .filter(property => 
                (!(property in this.state.semantics) || !(this.state.semantics[property] in this.props.properties)) &&
                (this.state.sourceSearch === "" || property.includes(this.state.sourceSearch) ||
                    (property in this.state.semantics && this.state.semantics[property].includes(this.state.sourceSearch))
                ) 
            )
            .reduce((obj, property) => {
                return {
                    ...obj,
                    [property]: this.props.properties[property]
                };
            }, {});
    }
    findTargetProperties() {
        const selected = this.state.selected;
        if (selected === null)
            return {};

        return Object.keys(this.props.properties)
            .filter(property => 
                (property !== selected) &&
                (!(property in this.state.semantics) || this.state.semantics[property] === selected) &&
                (this.props.properties[property].type === this.props.properties[selected].type) &&
                (this.state.targetSearch === "" || property.includes(this.state.targetSearch)) 
            )
            .reduce((obj, property) => {
                return {
                    ...obj,
                    [property]: this.props.properties[property]
                };
            }, {});
    }

    render() {
        const sourceProperties = this.findSourceProperties();
        const targetProperties = this.findTargetProperties();

        return (
            <div>
                <Button onClick={this.toggleModalOpen} color="success">Naming</Button>
                <Modal
                    isOpen={this.state.open}
                    toggle={this.toggleModalOpen}
                    size={'lg'}
                >
                    <ModalHeader toggle={this.toggleModalOpen}>Semantics</ModalHeader>
                    <ModalBody>
                        <Row className="px-4 d-flex justify-content-around">
                            <h3 className="text-center" style={{flexGrow: 1}}>This property</h3>
                            <p className="text-muted my-auto">is equivalent to</p>
                            <h3 className="text-center" style={{flexGrow: 1}}>These properties</h3>
                        </Row>
                        <Row>
                            <Col>
                                <PropertyList
                                    properties={sourceProperties}
                                    selected={[this.state.selected]}
                                    onClickProperty={this.handleClickSourceProperty}
                                />
                            </Col>
                            <Col>
                                <PropertyList
                                    properties={targetProperties}
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
                                    placeholder={"Search"}
                                    onChange={event => this.setState({ sourceSearch: event.target.value })}
                                />
                            </Col>
                            <Col>
                                <Input
                                    type="text"
                                    bsSize="sm"
                                    value={this.state.targetSearch}
                                    placeholder={"Search"}
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