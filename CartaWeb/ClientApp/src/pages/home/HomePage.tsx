import React, { Component } from "react";
import {
  Row,
  Col,
  Nav,
  NavItem,
  NavLink,
  Jumbotron,
  Container,
} from "reactstrap";
import { Link, RouteComponentProps } from "react-router-dom";
import "./HomePage.css";
import { Section, Subsection, Title } from "components/structure";

export interface HomePageProps extends RouteComponentProps {}
export interface HomePageState {}

export default class HomePage extends Component<HomePageProps, HomePageState> {
  static displayName = HomePage.name;

  render() {
    return (
      <div>
        <section>
          <Jumbotron fluid>
            <Container fluid className="text-center">
              <Title>Welcome to Carta!</Title>
              <p className="lead">
                Carta is a web-based API and application that provides
                graph-based tools for accessing, exploring, and transforming
                existing datasets and models.
              </p>
            </Container>
          </Jumbotron>
        </section>
        <Container>
          <Section title="Datasets">
            <Row>
              <Col xs="3">
                <Subsection title="Synthetic">
                  <p>
                    These are randomly-generated, synthetic datasets fabricated
                    by Carta on request.
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
                </Subsection>
              </Col>
              <Col xs="3">
                <Subsection title="HyperThought&trade;">
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
                      >
                        NAVAIR
                      </NavLink>
                    </NavItem>
                    <NavItem>
                      <NavLink tag={Link} to="/graph/HyperThought/CAMDEN.TTCP">
                        TTCP
                      </NavLink>
                    </NavItem>
                    <NavItem>
                      <NavLink tag={Link} to="/graph/HyperThought/CAMDEN.UDRI">
                        UDRI
                      </NavLink>
                    </NavItem>
                  </Nav>
                </Subsection>
              </Col>
            </Row>
          </Section>
        </Container>
      </div>
    );
  }
}
