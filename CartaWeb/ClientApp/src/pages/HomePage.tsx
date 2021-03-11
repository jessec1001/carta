import React, { Component } from "react";
import {
  Row,
  Col,
  Nav,
  NavItem,
  NavLink,
  Jumbotron,
  Container,
  Input,
  Button,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
} from "reactstrap";
import { Link, RouteComponentProps } from "react-router-dom";

import "./HomePage.css";

export interface HomePageProps extends RouteComponentProps {}

export interface HomePageState {
  hyperthoughtModalOpen: boolean;
  hyperthoughtKey: string;
  hyperthoughtRedirect?: { pathname: string; };
}

export default class HomePage extends Component<HomePageProps, HomePageState> {
  static displayName = HomePage.name;

  constructor(props: HomePageProps) {
    super(props);

    this.state = {
      hyperthoughtModalOpen: false,
      hyperthoughtKey: localStorage.getItem("hyperthoughtKey") ?? "",
    };

    this.handleHyperthoughtLink = this.handleHyperthoughtLink.bind(this);
    this.handleHyperthoughtModalToggle = this.handleHyperthoughtModalToggle.bind(
      this
    );
    this.handleHyperthoughtKeyChanged = this.handleHyperthoughtKeyChanged.bind(
      this
    );
    this.handleHyperthoughtKeySaved = this.handleHyperthoughtKeySaved.bind(
      this
    );
  }

  handleHyperthoughtLink(event: any) {
    // Retrieve the HyperThought API key from the user and redirect.
    const key = localStorage.getItem("hyperthoughtKey");
    if (!key) {
      this.setState({
        hyperthoughtModalOpen: true,
        hyperthoughtRedirect: {
          pathname: event.target.getAttribute("href")
        }
      });
    } else {
      this.props.history.push({
        pathname: event.target.getAttribute("href")
      });
    }
    event.preventDefault();
  }
  handleHyperthoughtModalToggle() {
    this.setState({
      hyperthoughtModalOpen: !this.state.hyperthoughtModalOpen,
    });
  }
  handleHyperthoughtKeyChanged(event: any) {
    this.setState({
      hyperthoughtKey: event.target.value,
    });
  }
  handleHyperthoughtKeySaved() {
    const key = this.state.hyperthoughtKey;
    localStorage.setItem("hyperthoughtKey", key);
    this.setState({
      hyperthoughtModalOpen: false,
    });
    if (this.state.hyperthoughtRedirect && key) {
      this.props.history.push(this.state.hyperthoughtRedirect);
    }
  }

  render() {
    return (
      <div>
        <Modal
          isOpen={this.state.hyperthoughtModalOpen}
          toggle={this.state.hyperthoughtModalOpen}
        >
          <ModalHeader>HyperThought&trade; Access</ModalHeader>
          <ModalBody>
            <p>
              You are trying to access HyperThought&trade; data but you do not
              have an API access key set. Please set your API access key below.
            </p>
            <Input
              type="text"
              placeholder="API Access Key"
              value={this.state.hyperthoughtKey}
              onChange={this.handleHyperthoughtKeyChanged}
            />
          </ModalBody>
          <ModalFooter>
            <Button onClick={this.handleHyperthoughtKeySaved}>Save</Button>
          </ModalFooter>
        </Modal>
        <section>
          <Jumbotron fluid>
            <Container fluid className="text-center">
              <h1 className="display-4">Welcome to Carta!</h1>
              <p className="lead">
                Carta is a web-based API and application that provides
                graph-based tools for accessing, exploring, and transforming
                existing datasets and models.
              </p>
            </Container>
          </Jumbotron>
        </section>
        <section>
          <Container>
            <h2>Datasets</h2>
            <hr />
            <Row>
              <Col xs="3">
                <h3>Synthetic</h3>
                <p>
                  These are randomly-generated, synthetic datasets fabricated by
                  Carta on request.
                </p>
                <Nav vertical>
                  <NavItem>
                    <NavLink
                      tag={Link}
                      to="/graph/Synthetic/FiniteUndirectedGraph"
                    >
                      Finite Undirected
                    </NavLink>
                  </NavItem>
                  <NavItem>
                    <NavLink
                      tag={Link}
                      to="/graph/Synthetic/InfiniteDirectedGraph"
                    >
                      Infinite Directed
                    </NavLink>
                  </NavItem>
                </Nav>
              </Col>
              <Col xs="3">
                <h3>HyperThought</h3>
                <p>
                  These are workflow datasets that exist on{" "}
                  <a href="https://www.hyperthought.io">HyperThought</a>{" "}
                  retrieved by its API.
                </p>
                <Nav vertical>
                  <NavItem>
                    <NavLink
                      tag={Link}
                      to="/graph/HyperThought/CAMDEN.NAVAIR"
                      onClick={this.handleHyperthoughtLink}
                    >
                      NAVAIR
                    </NavLink>
                  </NavItem>
                  <NavItem>
                    <NavLink
                      tag={Link}
                      to="/graph/HyperThought/CAMDEN.TTCP"
                      onClick={this.handleHyperthoughtLink}
                    >
                      TTCP
                    </NavLink>
                  </NavItem>
                  <NavItem>
                    <NavLink
                      tag={Link}
                      to="/graph/HyperThought/CAMDEN.UDRI"
                      onClick={this.handleHyperthoughtLink}
                    >
                      UDRI
                    </NavLink>
                  </NavItem>
                </Nav>
              </Col>
            </Row>
          </Container>
        </section>
      </div>
    );
  }
}
