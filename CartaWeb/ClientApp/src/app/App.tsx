import { FunctionComponent } from "react";
import { Route, Switch } from "react-router";
import { HomePage, GraphPage, DocsPage, UserPage } from "pages";
import {
  UserWrapper,
  NotificationWrapper,
  ThemeWrapper,
} from "components/utility";

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
            <Route exact path="/" component={HomePage} />
            <Route path="/documentation/:topic" component={DocsPage} />
            <Route path="/user" component={UserPage} />
            <Route path="/graph" component={GraphPage} />
          </Switch>
        </ThemeWrapper>
      </NotificationWrapper>
    </UserWrapper>
  );
};

export default App;
