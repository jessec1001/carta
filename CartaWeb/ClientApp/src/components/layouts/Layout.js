import React, { Component } from 'react';
import { Navigation } from '../shared/Navigation';
import './Layout.css';

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
      <div>
        <Navigation />
        {this.props.children}
      </div>
    );
  }
}
