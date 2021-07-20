import { UserContext } from "context";
import { FunctionComponent, useContext } from "react";

/** A component that signs out the user when its children are clicked. */
const UserSignOut: FunctionComponent = ({ children }) => {
  const { signOut } = useContext(UserContext);
  return <span onClick={signOut}>{children}</span>;
};

export default UserSignOut;