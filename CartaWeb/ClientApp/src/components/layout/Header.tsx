import { FunctionComponent, useContext } from "react";
import { UserContext } from "components/user";
import { CartaIcon } from "components/icons";
import { ThemeButton } from "components/theme";
import {
  UserIsAuthenticated,
  UserIsNotAuthenticated,
  UserSignOut,
  UserSignInDialog,
} from "components/user";
import { Dropdown } from "components/dropdown";
import { NavigationLink, NavigationLinkList } from "components/navigation";

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
        <NavigationLink lower to="/">
          Home
        </NavigationLink>
        <NavigationLink lower to="/documentation">
          Documentation
        </NavigationLink>
      </NavigationLinkList>

      {/* This link is used to sign in when the user is unauthenticated. */}
      <UserIsNotAuthenticated>
        <Dropdown side="bottom-left">
          <Dropdown.Toggler>
            <NavigationLink lower to="#">
              Sign In
            </NavigationLink>
          </Dropdown.Toggler>
          <Dropdown.Area>
            <UserSignInDialog />
          </Dropdown.Area>
        </Dropdown>
      </UserIsNotAuthenticated>

      {/* This user dropdown is used to navigate to authenticated pages. */}
      <UserIsAuthenticated>
        <Dropdown side="bottom-left">
          <Dropdown.Toggler caret>
            <NavigationLink lower to="/profile">
              {username}
            </NavigationLink>
          </Dropdown.Toggler>
          <Dropdown.Area>
            <Dropdown.Item>
              <NavigationLink lower to="/profile">
                Profile
              </NavigationLink>
            </Dropdown.Item>
            <Dropdown.Item>
              <NavigationLink lower to="#">
                <UserSignOut>Sign Out</UserSignOut>
              </NavigationLink>
            </Dropdown.Item>
          </Dropdown.Area>
        </Dropdown>
      </UserIsAuthenticated>

      {/* Render a theme button to change themes. */}
      <ThemeButton />
    </header>
  );
};

export default Header;
