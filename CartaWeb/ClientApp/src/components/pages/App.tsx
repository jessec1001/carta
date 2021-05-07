import React, { Component, createContext } from "react";
import { Route } from "react-router";
import { Layout } from "components/ui/layout/Layout";
import {
  HomePage,
  GraphPage,
  DocsPage,
  UserProfilePage,
} from "components/pages";
import "./custom.css";
import { Notification, NotificationManager } from "library/notifications";
import { NotificationCenter } from "components/ui/notifications";
import TestPage from "./TestPage";

import { UserContextWrapper } from "components/ui/user";

export const NotificationContext = createContext<{
  manager: NotificationManager;
  notifications: Notification[];
}>({
  manager: new NotificationManager(),
  notifications: [],
});

export interface AppState {
  notifications: Notification[];
}

export default class App extends Component<{}, AppState> {
  static displayName = App.name;

  notificationManager: NotificationManager;

  constructor(props: {}) {
    super(props);

    this.handleNotificationsChanged = this.handleNotificationsChanged.bind(
      this
    );

    this.notificationManager = new NotificationManager();
    this.notificationManager.on(
      "notificationAdded",
      this.handleNotificationsChanged
    );
    this.notificationManager.on(
      "notificationRemoved",
      this.handleNotificationsChanged
    );

    this.state = {
      notifications: [],
    };
  }

  handleNotificationsChanged() {
    this.setState({
      notifications: this.notificationManager.notifications,
    });
  }

  render() {
    return (
      <UserContextWrapper>
        <NotificationContext.Provider
          value={{
            manager: this.notificationManager,
            notifications: this.state.notifications,
          }}
        >
          <Layout>
            <Route exact path="/" component={HomePage} />
            <Route path="/graph" component={GraphPage} />
            <Route path="/docs" component={DocsPage} />
            <Route path="/test" component={TestPage} />
            <Route path="/user/profile" component={UserProfilePage} />
          </Layout>
          <NotificationCenter />
        </NotificationContext.Provider>
      </UserContextWrapper>
    );
  }
}
