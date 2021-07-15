import { FunctionComponent, useEffect } from "react";
import { Route, Switch } from "react-router";
import { HomePage, GraphPage, DocsPage, UserPage } from "pages";
import { UserWrapper, NotificationWrapper } from "components/utility";

import "styles/reset.css";
import "styles/globals.css";
import "styles/theme.css";
import "styles/app.css";

/**
 * The main app component which renders all other components.
 * Used to store routing information for each page.
 */
const App: FunctionComponent = () => {
  // TODO: Temporary effect.
  useEffect(() => {
    document.body.classList.add("theme-light");
  }, []);

  return (
    <UserWrapper>
      <NotificationWrapper>
        <Switch>
          <Route exact path="/" component={HomePage} />
          <Route path="/docs/:topic" component={DocsPage} />
          <Route path="/user" component={UserPage} />
          <Route path="/graph" component={GraphPage} />
        </Switch>
      </NotificationWrapper>
    </UserWrapper>
  );
};

export default App;
