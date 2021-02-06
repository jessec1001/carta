import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Navigation } from '../shared/nav/Navigation';
import './Layout.css';

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <Container fluid className="h-100 d-flex flex-column">
        <Navigation />
        {this.props.children}
      </Container>
    );
  }
}
