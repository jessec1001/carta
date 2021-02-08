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

interface GraphToolbarProps {
    className?: string
    style?: CSS.Properties
}

export class GraphToolbar extends Component<GraphToolbarProps> {
    render() {
        return (
            <Navbar className={this.props.className} light style={this.props.style} expand="md">
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
                                By Name
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
        );
    }
}