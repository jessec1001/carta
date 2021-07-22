import { FunctionComponent, useContext } from "react";
import { UserContext } from "context";

/** A component that signs in the user when its children are clicked. */
const UserSignIn: FunctionComponent = ({ children }) => {
  const { signIn } = useContext(UserContext);
  return <span onClick={signIn}>{children}</span>;
};

export default UserSignIn;
