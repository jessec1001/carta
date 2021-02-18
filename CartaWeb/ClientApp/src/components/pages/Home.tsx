import React, { Component } from 'react';
import { Row, Col, Nav, NavItem, NavLink, Jumbotron, Container } from 'reactstrap';
import { Link, RouteComponentProps } from 'react-router-dom';
import './Home.css';

interface HomeProps extends RouteComponentProps { }

export class Home extends Component<HomeProps> {
  static displayName = Home.name;

  constructor(props : HomeProps) {
    super(props);
    this.handleHyperthoughtLink = this.handleHyperthoughtLink.bind(this);
  }

  handleHyperthoughtLink(event : any) {
    // Retrieve the HyperThought API key from the user and redirect.
    const key = prompt("Please paste your HyperThought API key.");
    if (key) {
      this.props.history.push({
        pathname: event.target.getAttribute("href"),
        search: `?api=${key}`
      });
    }
    event.preventDefault();
  }

  render() {
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
                These are randomly-generated, synthetic datasets fabricated by Carta on request.
              </p>
              <Nav vertical>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/synthetic/FiniteUndirectedGraph"
                  >
                    Finite Undirected
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/synthetic/InfiniteDirectedGraph"
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
                    to="/graph/hyperthought/CAMDEN.NAVAIR"
                    onClick={this.handleHyperthoughtLink}
                  >
                    NAVAIR
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/hyperthought/CAMDEN.TTCP"
                    onClick={this.handleHyperthoughtLink}
                  >
                    TTCP
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/hyperthought/CAMDEN.UDRI"
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
