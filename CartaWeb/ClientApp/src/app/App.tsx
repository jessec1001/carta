import { FunctionComponent } from "react";
import { Route, Switch } from "react-router";
import {
  HomePage,
  DocsPage,
  ProfilePage,
  TestPage,
  WorkspaceAreaPage,
  WorkspaceNewPage,
  SignInPage,
  ForgotPasswordPage,
  ResetPasswordPage,
} from "pages";
import { UserWrapper } from "components/user";
import { Notifications } from "components/notifications";
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
    <Notifications>
      <UserWrapper>
        <ThemeWrapper>
          <Switch>
            {/* Home page route. */}
            <Route exact path="/" component={HomePage} />

            <Route path="/profile" component={ProfilePage} />
            <Route path="/documentation/:topic" component={DocsPage} />
            <Route path="/test" component={TestPage} />

            {/* Workspace page routes. */}
            <Route exact path="/workspace/new" component={WorkspaceNewPage} />
            <Route path="/workspace" component={WorkspaceAreaPage} />

            {/* Authentication page routes. */}
            <Route exact path="/signin" component={SignInPage} />
            <Route exact path="/signin/forgotpassword" component={ForgotPasswordPage} />
            <Route exact path="/signin/resetpassword" component={ResetPasswordPage} />

          </Switch>
        </ThemeWrapper>
      </UserWrapper>
    </Notifications>
  );
};

export default App;
