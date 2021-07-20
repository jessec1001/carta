import { FunctionComponent, useContext } from "react";
import { CartaIcon } from "components/icons";
import {
  ThemeButton,
  UserIsAuthenticated,
  UserIsNotAuthenticated,
  UserSignIn,
  UserSignOut,
} from "components/utility";
import {
  Dropdown,
  DropdownToggler,
  DropdownArea,
  DropdownItem,
} from "components/dropdown";
import { NavigationLink, NavigationLinkList } from "components/navigation";
import { UserContext } from "context";

/**
 * A component that represents the header navigation that allows for easy
 * navigation across the website.
 */
const Header: FunctionComponent = () => {
  // We use the username to display a dropdown when the user is logged in.
  const { user } = useContext(UserContext);
  const username = user?.name;

  return (
    <header className="header">
      {/* Render the Carta icon as a home link. */}
      <NavigationLink to="/">
        <CartaIcon />
      </NavigationLink>

      {/* Render navigation links to common pages. */}
      <NavigationLinkList>
        <NavigationLink to="/workspace">Workspaces</NavigationLink>
        <NavigationLink to="/documentation">Documentation</NavigationLink>
      </NavigationLinkList>

      {/* This link is used to sign in when the user is unauthenticated. */}
      <UserIsNotAuthenticated>
        <NavigationLink to="#">
          <UserSignIn>Sign In</UserSignIn>
        </NavigationLink>
      </UserIsNotAuthenticated>

      {/* This user dropdown is used to navigate to authenticated pages. */}
      <UserIsAuthenticated>
        <Dropdown side="bottom-left">
          <DropdownToggler caret>
            <NavigationLink to="/profile">{username}</NavigationLink>
          </DropdownToggler>
          <DropdownArea>
            <DropdownItem>
              <NavigationLink to="/profile">Profile</NavigationLink>
            </DropdownItem>
            <hr />
            <DropdownItem>
              <NavigationLink to="#">
                <UserSignOut>Sign Out</UserSignOut>
              </NavigationLink>
            </DropdownItem>
          </DropdownArea>
        </Dropdown>
      </UserIsAuthenticated>

      {/* Render a theme button to change themes. */}
      <ThemeButton />
    </header>
  );
};

export default Header;
