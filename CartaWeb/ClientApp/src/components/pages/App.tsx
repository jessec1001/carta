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
import { User, UserManager } from "library/api/user/types";
import { Notification, NotificationManager } from "library/notifications";
import { NotificationCenter } from "components/ui/notifications";

export const UserContext = createContext<{
  manager: UserManager;
  user: User | null;
}>({
  manager: new UserManager(),
  user: null,
});
export const NotificationContext = createContext<{
  manager: NotificationManager;
  notifications: Notification[];
}>({
  manager: new NotificationManager(),
  notifications: [],
});

export interface AppState {
  user: User | null;
  notifications: Notification[];
}

export default class App extends Component<{}, AppState> {
  static displayName = App.name;

  userManager: UserManager;
  notificationManager: NotificationManager;

  constructor(props: {}) {
    super(props);

    this.handleUserSignin = this.handleUserSignin.bind(this);
    this.handleUserSignout = this.handleUserSignout.bind(this);
    this.handleNotificationsChanged = this.handleNotificationsChanged.bind(
      this
    );

    this.userManager = new UserManager();
    this.userManager.on("signin", this.handleUserSignin);
    this.userManager.on("signout", this.handleUserSignout);

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
      user: null,
      notifications: [],
    };
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

  handleNotificationsChanged() {
    this.setState({
      notifications: this.notificationManager.notifications,
    });
  }

  render() {
    return (
      <UserContext.Provider
        value={{ manager: this.userManager, user: this.state.user }}
      >
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
            <Route path="/user/profile" component={UserProfilePage} />
          </Layout>
          <NotificationCenter />
        </NotificationContext.Provider>
      </UserContext.Provider>
    );
  }
}
