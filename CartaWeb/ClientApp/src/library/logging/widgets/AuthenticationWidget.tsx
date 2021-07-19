import { Link } from "components/common";
import { UserSignIn } from "components/utility";

function AuthenticationWidget() {
  return (
    <span>
      You must be authenticated to perform this action. Please &nbsp;{" "}
      <Link to="#">
        <UserSignIn>sign in</UserSignIn>
      </Link>
      .
    </span>
  );
}

export default AuthenticationWidget;
