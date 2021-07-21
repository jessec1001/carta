import { Link } from "components/common";
import { Paragraph } from "components/text";
import { FunctionComponent } from "react";
import { UserSignIn } from ".";

/** A component that displays a message that indicates that the user should be authenticated. */
const UserNeedsAuthentication: FunctionComponent = () => {
  return (
    <Paragraph>
      You must{" "}
      <Link to="#">
        <UserSignIn>sign in</UserSignIn>
      </Link>{" "}
      or{" "}
      <Link to="#">
        <UserSignIn>sign up</UserSignIn>
      </Link>{" "}
      to use this functionality.
    </Paragraph>
  );
};

export default UserNeedsAuthentication;
