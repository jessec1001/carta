import { FunctionComponent } from "react";

/** A component that signs in the user when its children are clicked. */
const UserSignIn: FunctionComponent = ({ children }) => {
  // TODO: Update to use the sign in dialog as a modal.
  return <span>{children}</span>;
};

export default UserSignIn;
