import { FunctionComponent, useContext } from "react";
import { CartaIcon } from "components/icons";
import { ThemeButton } from "components/theme";
import {
  UserIsAuthenticated,
  UserIsNotAuthenticated,
  UserSignIn,
  UserSignOut,
} from "components/user";
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
        {/* <NavigationLink to="/workspace">Workspaces</NavigationLink> */}
        <NavigationLink lower to="/documentation">
          Documentation
        </NavigationLink>
      </NavigationLinkList>

      {/* This link is used to sign in when the user is unauthenticated. */}
      <UserIsNotAuthenticated>
        <NavigationLink lower to="#">
          <UserSignIn>Sign In</UserSignIn>
        </NavigationLink>
      </UserIsNotAuthenticated>

      {/* This user dropdown is used to navigate to authenticated pages. */}
      <UserIsAuthenticated>
        <Dropdown side="bottom-left">
          <DropdownToggler caret>
            <NavigationLink lower to="/profile">
              {username}
            </NavigationLink>
          </DropdownToggler>
          <DropdownArea>
            <DropdownItem>
              <NavigationLink lower to="/profile">
                Profile
              </NavigationLink>
            </DropdownItem>
            <DropdownItem>
              <NavigationLink lower to="#">
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
