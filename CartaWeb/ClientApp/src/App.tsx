import React, { Component } from "react";
import { Route } from "react-router";
import { Layout } from "components/layouts/Layout";
import { HomePage, GraphPage, DocsPage, UserProfilePage } from "./pages";
import { UserApi } from "lib/api";
import "./custom.css";

export default class App extends Component {
  static displayName = App.name;

  render() {
    UserApi.signInAsync({})
      .then(() => console.log("Logged in successfully."))
      .catch(() => console.log("Failed to login."));

    return (
      // <UserState.Provider value={1}>
      <Layout>
        <Route exact path="/" component={HomePage} />
        <Route path="/graph" component={GraphPage} />
        <Route path="/docs" component={DocsPage} />
        <Route path="/user/profile" component={UserProfilePage} />
      </Layout>
      // </UserState.Provider>
    );
  }
}
