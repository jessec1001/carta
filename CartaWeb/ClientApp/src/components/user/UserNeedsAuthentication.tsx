import Link from "components/link/Link";
import { Text } from "components/text";
import { FunctionComponent } from "react";

/** A component that displays a message that indicates that the user should be authenticated. */
const UserNeedsAuthentication: FunctionComponent = () => {
  return (
    <Text>
      You must <Link to="/signin">sign in</Link> to use this functionality.
    </Text>
  );
};

export default UserNeedsAuthentication;
