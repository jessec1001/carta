import React, { Component } from "react";
import { Route } from "react-router";
import { Layout } from "./components/layouts/Layout";
import { HomePage, GraphPage, DocsPage, UserPage } from "./pages";
import "./custom.css";
import UserApi from "./lib/api/UserApi";

export default class App extends Component {
  static displayName = App.name;

  render() {
    UserApi.SignInAsync()
      .then(() => console.log("Logged in successfully."))
      .catch(() => console.log("Failed to login."));

    return (
      // <UserState.Provider value={1}>
      <Layout>
        <Route exact path="/" component={HomePage} />
        <Route path="/graph" component={GraphPage} />
        <Route path="/docs" component={DocsPage} />
        <Route path="/user" component={UserPage} />
      </Layout>
      // </UserState.Provider>
    );
  }
}
