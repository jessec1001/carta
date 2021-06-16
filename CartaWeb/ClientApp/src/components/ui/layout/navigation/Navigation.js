import React, { Component } from "react";
import { version } from "../../../../../package.json";
import {
  Collapse,
  Container,
  DropdownItem,
  DropdownMenu,
  DropdownToggle,
  Navbar,
  NavbarBrand,
  NavbarToggler,
  NavItem,
  NavLink,
  UncontrolledDropdown,
} from "reactstrap";
import { Link } from "react-router-dom";
import "./Navigation.css";
import UserContext from "components/ui/user";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faSignOutAlt,
  faUser,
  faUserCircle,
} from "@fortawesome/free-solid-svg-icons";

export class Navigation extends Component {
  static displayName = Navigation.name;

  constructor(props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true,
    };
  }

  toggleNavbar() {
    this.setState({
      collapsed: !this.state.collapsed,
    });
  }

  render() {
    return (
      <header className="header">
        <Navbar
          className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom"
          light
        >
          <Container>
            <NavbarBrand tag={Link} to="/">
              <img src="carta.svg" alt="Carta" style={{ height: "2rem" }} />
              &nbsp; <span className="text-muted version">v{version}</span>
            </NavbarBrand>
            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
            <Collapse
              className="d-sm-inline-flex flex-sm-row-reverse"
              isOpen={!this.state.collapsed}
              navbar
            >
              <ul className="navbar-nav flex-grow">
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/">
                    Home
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/graph">
                    Graph
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink tag={Link} className="text-dark" to="/docs">
                    Docs
                  </NavLink>
                </NavItem>
                <UserContext.Consumer>
                  {({ manager, authenticated, user }) => {
                    if (authenticated)
                      return (
                        <UncontrolledDropdown nav inNavbar className="ml-4">
                          <DropdownToggle
                            nav
                            className="text-dark"
                            style={{ fontSize: "1rem" }}
                          >
                            <FontAwesomeIcon
                              className="mr-2"
                              icon={faUserCircle}
                            />
                            {user.name}
                          </DropdownToggle>
                          <DropdownMenu right>
                            <DropdownItem>
                              <Link className="text-dark" to="/user">
                                <FontAwesomeIcon
                                  className="mr-2"
                                  icon={faUser}
                                />
                                Profile
                              </Link>
                            </DropdownItem>
                            <DropdownItem>
                              <Link
                                className="text-dark"
                                onClick={manager.signOutAsync}
                                to="#"
                              >
                                <FontAwesomeIcon
                                  className="mr-2"
                                  icon={faSignOutAlt}
                                />
                                Sign Out
                              </Link>
                            </DropdownItem>
                          </DropdownMenu>
                        </UncontrolledDropdown>
                      );
                    else
                      return (
                        <NavItem className="ml-4">
                          <NavLink
                            tag={Link}
                            className="text-dark"
                            onClick={manager.signInAsync}
                            to="#"
                          >
                            Sign In
                          </NavLink>
                        </NavItem>
                      );
                  }}
                </UserContext.Consumer>
              </ul>
            </Collapse>
          </Container>
        </Navbar>
      </header>
    );
  }
}
