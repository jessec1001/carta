import React, { Component } from 'react';
import { Input, Row, Col, Button, Modal, ModalHeader, ModalBody } from 'reactstrap';
import { PropertyList } from './PropertyList';

export class Semantics extends Component {
    static displayName = Semantics.name;

    constructor(props) {
        super(props);
        this.state = {
            open: false,
            selected: null,
            semantics: {},
            sourceSearch: "",
            targetSearch: "",
            alias: ""
        };

        this.toggleModalOpen = this.toggleModalOpen.bind(this);
        this.handleClickSourceProperty = this.handleClickSourceProperty.bind(this);
        this.handleClickTargetProperty = this.handleClickTargetProperty.bind(this);
        this.handleAliasChanged = this.handleAliasChanged.bind(this);
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
        let alias = "";
        if (property !== this.state.selected)
        {
            if (property in this.state.semantics)
                alias = this.state.semantics[property];
            else
                alias = property
        }

        this.setState({
            selected: property === this.state.selected ? null : property,
            alias: alias
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
    handleAliasChanged(event) {
        let alias = event.target.value; 

        let semantics = this.state.semantics;
        semantics[this.state.selected] = alias;

        this.setState({
            alias: alias,
            semantics: semantics
        });

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

        const aliases = Object.keys(this.state.semantics)
            .filter(attribute => !(this.state.semantics[attribute] in this.props.attributes))
            .reduce((obj, attribute) => ({ ...obj, [attribute]: this.state.semantics[attribute]}), {});

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
                            <Col>
                                <PropertyList
                                    properties={sourceAttributes}
                                    semantics={aliases}
                                    selected={[this.state.selected]}
                                    onClickProperty={this.handleClickSourceProperty}
                                >
                                    <Row>
                                        <Col>
                                            <h3>Source</h3>
                                        </Col>
                                        <Col>
                                            <Input
                                                type="text"
                                                bsSize="sm"
                                                value={this.state.sourceSearch}
                                                onChange={event => this.setState({ sourceSearch: event.target.value })}
                                            />
                                        </Col>
                                    </Row>
                                </PropertyList>
                            </Col>
                            <Col>
                                <PropertyList
                                    properties={targetAttributes}
                                    semantics={aliases}
                                    selected={Object.keys(this.state.semantics)}
                                    onClickProperty={this.handleClickTargetProperty}
                                >
                                    <Row>
                                        <Col>
                                            <h3>Target</h3>
                                        </Col>
                                        <Col>
                                            <Input
                                                type="text"
                                                bsSize="sm"
                                                value={this.state.targetSearch}
                                                onChange={event => this.setState({ targetSearch: event.target.value })}
                                            />
                                        </Col>
                                    </Row>
                                </PropertyList>
                            </Col>
                        </Row>
                        <Row>
                            <Col>
                                <Input
                                    type="text"
                                    bsSize="sm"
                                    placeholder="Rename"
                                    disabled={this.state.selected === null}
                                    value={this.state.alias}
                                    onChange={this.handleAliasChanged}
                                />
                            </Col>
                        </Row>
                    </ModalBody>
                </Modal>
            </div>
        );
    }
}