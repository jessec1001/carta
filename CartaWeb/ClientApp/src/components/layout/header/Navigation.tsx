import { FunctionComponent } from "react";
import { CartaIcon } from "components/icons";
import NavigationLink from "./NavigationLink";
import NavigationLinkList from "./NavigationLinkList";

/**
 * A component that represents the header navigation that allows for easy
 * navigation across the website.
 */
const Navigation: FunctionComponent = () => {
  return (
    <header className="header">
      {/* Render the Carta icon as a home link. */}
      <NavigationLink to="/">
        <CartaIcon />
      </NavigationLink>

      {/* Render navigation links to common pages. */}
      <NavigationLinkList>
        <NavigationLink to="/workspaces">Workspaces</NavigationLink>
        <NavigationLink to="/documentation">Documentation</NavigationLink>
      </NavigationLinkList>

      {/* Render sign in or profile link depending on authentication state. */}
      <NavigationLink to="#">Sign In</NavigationLink>
    </header>
  );
};

export default Navigation;
