import React, { Component } from 'react';
import { Row, Col, Nav, NavItem, NavLink, Jumbotron, Container } from 'reactstrap';
import { Link } from 'react-router-dom';
import './Home.css';

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
      <div>
        <section>
          <Jumbotron fluid>
            <Container fluid className="text-center">
              <h1 className="display-4">Welcome to Carta!</h1>
              <p className="lead">Carta is a web-based API and application that provides graph-based tools for accessing, exploring, and transforming existing datasets and models.</p>
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
                These are randomly-generated, synthetic datasets fabricatedd by Carta on request.
              </p>
              <Nav vertical>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/synthetic/RandomFiniteUndirectedGraph"
                  >
                    Finite Undirected
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/synthetic/RandomInfiniteDirectedGraph"
                  >
                    Infinite Directed
                  </NavLink>
                </NavItem>
              </Nav>
            </Col>
            <Col xs="3">
              <h3>HyperThought</h3>
              <p>
                These are workflow datasets that exist on <a href="https://www.hyperthought.io">HyperThought</a> retrieved by its API. 
              </p>
              <Nav vertical>
              <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/hyperthought/"
                    disabled
                  >
                    CAMDEN
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
