import { Link } from "react-router-dom";
import { UserContext } from "context";

function AuthenticationWidget() {
  return (
    <UserContext.Consumer>
      {({ manager }) => (
        <span>
          You must be authenticated to perform this action. Please &nbsp;
          <Link to="#" onClick={manager.signInAsync}>
            sign in
          </Link>
          .
        </span>
      )}
    </UserContext.Consumer>
  );
}

export default AuthenticationWidget;
