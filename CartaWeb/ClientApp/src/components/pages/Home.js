import React, { Component } from 'react';
import { Row, Col, Nav, NavItem, NavLink, Jumbotron, Container } from 'reactstrap';
import { Link } from 'react-router-dom';
import './Home.css';

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);

    this.state = {
      hyperthoughtKey: ""
    };

    this.handleHyperthoughtLink = this.handleHyperthoughtLink.bind(this);
  }

  handleHyperthoughtLink(event) {
    const key = prompt("Please paste your HyperThought API key.");
    this.props.history.push({
      pathname: event.target.getAttribute("href"),
      search: `?api=${key}`
    });
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
                    to="/graph/hyperthought/254fdd9f-264a-4301-b372-46810b51f39b"
                    onClick={this.handleHyperthoughtLink}
                  >
                    NAVAIR
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/hyperthought/1badcce7-ac18-4664-962a-f730559223fc"
                    onClick={this.handleHyperthoughtLink}
                  >
                    TTCP
                  </NavLink>
                </NavItem>
                <NavItem>
                  <NavLink
                    tag={Link}
                    to="/graph/hyperthought/f494e24a-a2e8-4a11-a34a-dad36bb70515"
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
