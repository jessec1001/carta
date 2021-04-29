import React, { Component, createContext } from "react";
import { Route } from "react-router";
import { Layout } from "components/layouts/Layout";
import { HomePage, GraphPage, DocsPage, UserProfilePage } from "./pages";
import { UserApi } from "lib/api";
import "./custom.css";
import { User, UserManager } from "lib/api/user/types";

export const UserContext = createContext<{
  manager: UserManager;
  user: User | null;
}>({
  manager: new UserManager(),
  user: null,
});

export interface AppState {
  user: User | null;
}

export default class App extends Component<{}, AppState> {
  static displayName = App.name;

  userManager: UserManager;

  constructor(props: {}) {
    super(props);

    this.handleUserSignin = this.handleUserSignin.bind(this);
    this.handleUserSignout = this.handleUserSignout.bind(this);

    this.userManager = new UserManager();
    this.userManager.on("signin", this.handleUserSignin);
    this.userManager.on("signout", this.handleUserSignout);
    this.state = {
      user: null,
    };

    this.userManager.SignInAsync();
  }

  handleUserSignin(user: User) {
    this.setState({
      user: user,
    });
  }
  handleUserSignout() {
    this.setState({
      user: null,
    });
  }

  render() {
    return (
      <UserContext.Provider
        value={{ manager: this.userManager, user: this.state.user }}
      >
        <Layout>
          <Route exact path="/" component={HomePage} />
          <Route path="/graph" component={GraphPage} />
          <Route path="/docs" component={DocsPage} />
          <Route path="/user/profile" component={UserProfilePage} />
        </Layout>
      </UserContext.Provider>
    );
  }
}
