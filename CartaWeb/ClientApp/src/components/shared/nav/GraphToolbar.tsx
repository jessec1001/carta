import React, { Component } from 'react';
import {
    Navbar,
    Nav,
    UncontrolledDropdown,
    DropdownToggle,
    DropdownMenu,
    DropdownItem
} from 'reactstrap';
import CSS from 'csstype';

import './GraphToolbar.css';

interface GraphToolbarProps {
    className?: string
    style?: CSS.Properties

    onSelect?: (selector: any) => void
}

export class GraphToolbar extends Component<GraphToolbarProps> {
    constructor(props: GraphToolbarProps) {
        super(props);

        this.handleSelectByName = this.handleSelectByName.bind(this);
        this.handleSelectByProperty = this.handleSelectByProperty.bind(this);
        this.handleSelectExpanded = this.handleSelectExpanded.bind(this);
        this.handleSelectCollapsed = this.handleSelectCollapsed.bind(this);
    }

    handleSelectByName() {
        let pattern = prompt("Enter a regular expression pattern to match by.");
        if (this.props.onSelect && pattern) {
            this.props.onSelect({
                type: "regex",
                pattern: pattern
            });
        }
    }
    handleSelectByProperty() {
        let property = prompt("Enter the property name to match by.");
        if (this.props.onSelect && property) {
            this.props.onSelect({
                type: "property",
                property: property
            });
        }
    }

    handleSelectExpanded() {
        if (this.props.onSelect) {
            this.props.onSelect("expanded");
        }
    }
    handleSelectCollapsed() {
        if (this.props.onSelect) {
            this.props.onSelect("collapsed");
        }
    }

    render() {
        return (
            <Navbar className={this.props.className} light style={this.props.style} expand="md">
                <Nav navbar>
                    <UncontrolledDropdown nav inNavbar>
                        <DropdownToggle nav>
                            Select
                        </DropdownToggle>
                        <DropdownMenu>
                            {/* <DropdownItem>
                                All
                            </DropdownItem> */}
                            <DropdownItem onClick={this.handleSelectByName}>
                                Select by Name
                            </DropdownItem>
                            <DropdownItem onClick={this.handleSelectByProperty}>
                                Select by Property
                            </DropdownItem>
                            <hr />
                            <DropdownItem onClick={this.handleSelectExpanded}>
                                Select Expanded
                            </DropdownItem>
                            <DropdownItem onClick={this.handleSelectCollapsed}>
                                Select Collapsed
                            </DropdownItem>
                            {/* <DropdownItem>
                                Descendants
                            </DropdownItem>
                            <DropdownItem>
                                Ancestors
                            </DropdownItem> */}
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
        );
    }
}