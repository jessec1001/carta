import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/layouts/Layout';
import { Home } from './components/pages/Home';
import { Graph } from './components/pages/Graph';
import { Docs } from './components/pages/Docs';
import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/graph' component={Graph} />
        <Route path='/docs' component={Docs} />
      </Layout>
    );
  }
}
