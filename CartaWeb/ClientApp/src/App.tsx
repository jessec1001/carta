import React, { Component } from "react";
import { Route } from "react-router";
import { Layout } from "./components/layouts/Layout";
import { HomePage, GraphPage, DocsPage, UserPage } from "./pages";
import "./custom.css";

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <Layout>
        <Route exact path="/" component={HomePage} />
        <Route path="/graph" component={GraphPage} />
        <Route path="/docs" component={DocsPage} />
        <Route path="/user" component={UserPage} />
      </Layout>
    );
  }
}
