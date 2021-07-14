import { FunctionComponent } from "react";
import { Route } from "react-router";
import { HomePage, GraphPage, DocsPage, UserPage } from "pages";
import { Layout } from "components/ui/layout/Layout";
import { UserContextWrapper } from "components/ui/user";
import {
  NotificationCenter,
  NotificationContextWrapper,
} from "components/ui/notifications";

import "./custom.css";

/**
 * The main app component which renders all other components.
 * Used to store routing information for each page.
 */
const App: FunctionComponent = () => {
  return (
    <UserContextWrapper>
      <NotificationContextWrapper>
        <Layout>
          <Route exact path="/" component={HomePage} />
          <Route path="/graph" component={GraphPage} />
          <Route path="/docs/:topic" component={DocsPage} />
          <Route path="/user" component={UserPage} />
        </Layout>
        <NotificationCenter />
      </NotificationContextWrapper>
    </UserContextWrapper>
  );
};

export default App;
