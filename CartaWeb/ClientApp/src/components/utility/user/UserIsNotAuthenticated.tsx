import React, { FunctionComponent, useContext } from "react";
import { UserContext } from "context";

/** A component that only displays its children if the user is not authenticated. */
const UserIsNotAuthenticated: FunctionComponent = ({ children }) => {
  const { authenticated } = useContext(UserContext);
  return <React.Fragment>{authenticated ? null : children}</React.Fragment>;
};

export default UserIsNotAuthenticated;
