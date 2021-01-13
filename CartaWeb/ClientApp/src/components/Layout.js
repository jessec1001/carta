import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <div className="flex-container">
        <NavMenu />
        <div className="content">
        <Container>
          {this.props.children}
        </Container>
        </div>
      </div>
    );
  }
}
