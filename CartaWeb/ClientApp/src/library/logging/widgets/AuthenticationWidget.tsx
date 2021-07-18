import { Link } from "react-router-dom";
import { UserContext } from "context";

function AuthenticationWidget() {
  return (
    <UserContext.Consumer>
      {({ signIn }) => (
        <span>
          You must be authenticated to perform this action. Please &nbsp;
          <Link to="#" onClick={signIn}>
            sign in
          </Link>
          .
        </span>
      )}
    </UserContext.Consumer>
  );
}

export default AuthenticationWidget;
