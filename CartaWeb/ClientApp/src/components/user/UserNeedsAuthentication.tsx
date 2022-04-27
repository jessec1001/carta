import { Text } from "components/text";
import { FunctionComponent } from "react";
import { UserSignIn } from ".";

/** A component that displays a message that indicates that the user should be authenticated. */
const UserNeedsAuthentication: FunctionComponent = () => {
  return (
    <Text>
      You must <UserSignIn>sign in</UserSignIn> to use this functionality.
    </Text>
  );
};

export default UserNeedsAuthentication;
