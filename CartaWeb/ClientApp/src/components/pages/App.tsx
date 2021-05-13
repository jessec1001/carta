import { Component } from "react";
import { Route } from "react-router";
import { Layout } from "components/ui/layout/Layout";
import { HomePage, GraphPage, DocsPage, UserPage } from "components/pages";
import { UserContextWrapper } from "components/ui/user";
import { NotificationContextWrapper } from "components/ui/notifications";
import { NotificationCenter } from "components/ui/notifications";
import "./custom.css";

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <UserContextWrapper>
        <NotificationContextWrapper>
          <Layout>
            <Route exact path="/" component={HomePage} />
            <Route path="/graph" component={GraphPage} />
            <Route path="/docs" component={DocsPage} />
            <Route path="/user" component={UserPage} />
          </Layout>
          <NotificationCenter />
        </NotificationContextWrapper>
      </UserContextWrapper>
    );
  }
}
