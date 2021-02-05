import React, { Component } from 'react';
import { 
    Row,
    Col,
    Navbar,
    Nav,
    UncontrolledDropdown,
    DropdownToggle,
    DropdownMenu,
    DropdownItem
} from 'reactstrap';
import Split from 'react-split-grid';
import { RouteComponentProps } from "react-router-dom";
import { PropertyList } from "../shared/properties/PropertyList";
import { GraphExplorer } from '../forms/GraphExplorer';
import { Semantics } from '../forms/Semantics';
import { VisProperty } from '../../lib/types/vis-format';
import './Graph.css';

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
            <div className="page">
                <div className="w-75">
                    <Navbar className="toolbar" color="light" light expand="md">
                        <Nav navbar>
                            <UncontrolledDropdown nav inNavbar>
                                <DropdownToggle nav>
                                    Select
                                </DropdownToggle>
                                <DropdownMenu>
                                    <DropdownItem>
                                        All
                                    </DropdownItem>
                                    <DropdownItem>
                                        None
                                    </DropdownItem>
                                    <DropdownItem>
                                        Descendants
                                    </DropdownItem>
                                    <DropdownItem>
                                        Ancestors
                                    </DropdownItem>
                                </DropdownMenu>
                            </UncontrolledDropdown>
                            <UncontrolledDropdown nav inNavbar>
                                <DropdownToggle nav>
                                    Properties
                                </DropdownToggle>
                                <DropdownMenu>
                                    <DropdownItem>
                                        Naming
                                    </DropdownItem>
                                </DropdownMenu>
                            </UncontrolledDropdown>
                        </Nav>
                    </Navbar>
                    <GraphExplorer request={this.request} />
                </div>
                <div className="w-25">
                    <PropertyList properties={this.state.properties} semantics={this.state.semantics}>
                        <h2>Properties</h2>
                        <Semantics properties={this.state.properties} onSemanticsChanged={this.handleSemanticsChanged} />
                    </PropertyList>
                </div>
            </div>
        );
    }
}