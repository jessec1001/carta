import { FunctionComponent, useContext } from "react";
import { UserContext } from "components/user";

/** A component that signs out the user when its children are clicked. */
const UserSignOut: FunctionComponent = ({ children }) => {
  const { signOut } = useContext(UserContext);
  return <span onClick={() => signOut()}>{children}</span>;
};

export default UserSignOut;
