import { FunctionComponent } from "react";
import { CartaIcon } from "components/icons";
import {
  ThemeButton,
  UserIsAuthenticated,
  UserIsNotAuthenticated,
  UserSignIn,
  UserSignOut,
} from "components/utility";
import NavigationLink from "./NavigationLink";
import NavigationLinkList from "./NavigationLinkList";

/**
 * A component that represents the header navigation that allows for easy
 * navigation across the website.
 */
const Navigation: FunctionComponent = () => {
  // TODO: Remove temporary dropdown logic.
  // const [toggled, setToggled] = useState(false);

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
      <NavigationLink to="#">
        <UserIsNotAuthenticated>
          <UserSignIn>Sign In</UserSignIn>
        </UserIsNotAuthenticated>
        <UserIsAuthenticated>
          <UserSignOut>Sign Out</UserSignOut>
        </UserIsAuthenticated>
      </NavigationLink>
      {/* <div
        style={{
          position: "relative",
        }}
      >
        {toggled && (
          <div
            style={{
              position: "absolute",
              padding: "1rem",
              marginTop: "-1px",
              minWidth: "100%",
              top: "100%",
              right: "0%",
              borderRadius: "8px 0px 8px 8px",
              border: "1px solid var(--color-stroke-lowlight",
              zIndex: -100,
              background: "var(--color-fill-body)",
            }}
          >
            Test this functionality
          </div>
        )}
        <div
          style={{
            padding: "0.5rem 0.5rem 0rem 0.5rem",
            borderRadius: "8px 8px 0px 0px",
            border: toggled
              ? "1px solid var(--color-stroke-lowlight)"
              : "1px solid transparent",
            borderBottom: toggled
              ? "1px solid var(--color-fill-body)"
              : "1px solid transparent",
            background: toggled ? "var(--color-fill-body)" : "none",
          }}
        >
          <NavigationLink
            to="#"
            onClick={() => setToggled((prevToggled) => !prevToggled)}
          >
            Sign In
          </NavigationLink>
        </div>
      </div> */}

      {/* Render a theme button to change themes. */}
      <ThemeButton />
    </header>
  );
};

export default Navigation;
