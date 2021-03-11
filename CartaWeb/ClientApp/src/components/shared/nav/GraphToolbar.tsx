import React, { Component } from "react";
import {
  Navbar,
  Nav,
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
} from "reactstrap";
import CSS from "csstype";

import "./GraphToolbar.css";

interface GraphToolbarProps {
  className?: string;
  style?: CSS.Properties;

  onSelect?: (selector: any) => void;
}

export class GraphToolbar extends Component<GraphToolbarProps> {
  constructor(props: GraphToolbarProps) {
    super(props);

    this.handleSelectAll = this.handleSelectAll.bind(this);
    this.handleSelectNone = this.handleSelectNone.bind(this);
    this.handleSelectExpanded = this.handleSelectExpanded.bind(this);
    this.handleSelectCollapsed = this.handleSelectCollapsed.bind(this);
    this.handleSelectNodeName = this.handleSelectNodeName.bind(this);
    this.handleSelectPropertyName = this.handleSelectPropertyName.bind(this);
    this.handleSelectPropertyRange = this.handleSelectPropertyRange.bind(this);
    this.handleSelectDescendants = this.handleSelectDescendants.bind(this);
    this.handleSelectAncestors = this.handleSelectAncestors.bind(this);
  }

  handleSelectAll() {
    if (this.props.onSelect) {
      this.props.onSelect({
        type: "all",
      });
    }
  }
  handleSelectNone() {
    if (this.props.onSelect) {
      this.props.onSelect({
        type: "none",
      });
    }
  }

  handleSelectExpanded() {
    if (this.props.onSelect) {
      this.props.onSelect({
        type: "expanded",
      });
    }
  }
  handleSelectCollapsed() {
    if (this.props.onSelect) {
      this.props.onSelect({
        type: "collapsed",
      });
    }
  }

  handleSelectNodeName() {
    let pattern = prompt("Enter a regular expression that matches node names.");
    if (this.props.onSelect && pattern) {
      this.props.onSelect({
        type: "node",
        pattern: pattern,
      });
    }
  }
  handleSelectPropertyName() {
    let pattern = prompt(
      "Enter a regular expression that matches property names."
    );
    if (this.props.onSelect && pattern) {
      this.props.onSelect({
        type: "property",
        pattern: pattern,
      });
    }
  }
  handleSelectPropertyRange() {
    let property = prompt("Enter the property name.");
    let rangeMin = parseFloat(
      prompt("Enter the lower bound of the range.") ?? ""
    );
    let rangeMax = parseFloat(
      prompt("Enter the upper bound of the range.") ?? ""
    );
    if (this.props.onSelect && property) {
      this.props.onSelect({
        type: "range",
        property: property,
        min: rangeMin,
        max: rangeMax,
      });
    }
  }

  handleSelectDescendants() {
    if (this.props.onSelect) {
      this.props.onSelect({
        type: "descendants",
      });
    }
  }
  handleSelectAncestors() {
    if (this.props.onSelect) {
      this.props.onSelect({
        type: "ancestors",
      });
    }
  }

  render() {
    return (
      <Navbar
        className={this.props.className}
        light
        style={this.props.style}
        expand="md"
      >
        <Nav navbar>
          {/* <UncontrolledDropdown nav inNavbar>
            <DropdownToggle nav>Graph</DropdownToggle>
            <DropdownMenu>
              <DropdownItem>Open</DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown> */}
          <UncontrolledDropdown nav inNavbar>
            <DropdownToggle nav>Select</DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={this.handleSelectAll}>
                Select All
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectNone}>
                Select None
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleSelectExpanded}>
                Select Expanded
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectCollapsed}>
                Select Collapsed
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleSelectNodeName}>
                Select Node Name
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectPropertyName}>
                Select Property Name
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectPropertyRange}>
                Select Range
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleSelectDescendants}>
                Select Descendants
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectAncestors}>
                Select Ancestors
              </DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown>
          {/* <UncontrolledDropdown nav inNavbar>
            <DropdownToggle nav>Properties</DropdownToggle>
            <DropdownMenu>
              <DropdownItem>Naming</DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown> */}
        </Nav>
      </Navbar>
    );
  }
}
