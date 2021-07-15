import React, { FunctionComponent, useContext } from "react";
import { UserContext } from "context";

/** A component that only displays its children if the user is authenticated. */
const UserIsAuthenticated: FunctionComponent = ({ children }) => {
  const { authenticated } = useContext(UserContext);
  return <React.Fragment>{authenticated ? children : null}</React.Fragment>;
};

export default UserIsAuthenticated;
