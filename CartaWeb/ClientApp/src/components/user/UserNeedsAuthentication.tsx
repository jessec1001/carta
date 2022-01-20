import { Link } from "components/link";
import { Text } from "components/text";
import { FunctionComponent } from "react";
import { UserSignIn } from ".";

/** A component that displays a message that indicates that the user should be authenticated. */
const UserNeedsAuthentication: FunctionComponent = () => {
  return (
    <Text>
      You must{" "}
      <Link to="#">
        <UserSignIn>sign in</UserSignIn>
      </Link>{" "}
      or{" "}
      <Link to="#">
        <UserSignIn>sign up</UserSignIn>
      </Link>{" "}
      to use this functionality.
    </Text>
  );
};

export default UserNeedsAuthentication;
