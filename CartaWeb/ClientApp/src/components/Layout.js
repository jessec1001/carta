import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Navigation } from './Navigation';
import './Layout.css';

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <div className="growbox">
        <Navigation />
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}
