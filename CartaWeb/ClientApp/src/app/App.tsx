import { FunctionComponent } from "react";
import { Route, Switch } from "react-router";
import {
  HomePage,
  DocsPage,
  ProfilePage,
  TestPage,
  WorkspaceAreaPage,
  WorkspaceNewPage,
  WorkspaceListPage,
} from "pages";
import { UserWrapper } from "components/user";
import { NotificationWrapper } from "components/notifications";
import { ThemeWrapper } from "components/theme";

import "styles/reset.css";
import "styles/globals.css";
import "styles/theme.css";
import "styles/app.css";
/**
 * The main app component which renders all other components.
 * Used to store routing information for each page.
 */
const App: FunctionComponent = () => {
  return (
    <UserWrapper>
      <NotificationWrapper>
        <ThemeWrapper>
          <Switch>
            {/* Home page route. */}
            <Route exact path="/" component={HomePage} />

            <Route path="/profile" component={ProfilePage} />
            <Route path="/documentation/:topic" component={DocsPage} />
            <Route path="/test" component={TestPage} />

            {/* Workspace page routes. */}
            <Route exact path="/workspace/new" component={WorkspaceNewPage} />
            <Route exact path="/workspace/list" component={WorkspaceListPage} />
            <Route path="/workspace" component={WorkspaceAreaPage} />
          </Switch>
        </ThemeWrapper>
      </NotificationWrapper>
    </UserWrapper>
  );
};

export default App;
