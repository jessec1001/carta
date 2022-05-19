import { FC, useContext, useEffect, useState } from "react";
import { useHistory, useLocation } from "react-router-dom";
import { CognitoUser } from "@aws-amplify/auth";
import { Title } from "components/text";
import UserContext from "./UserContext";
import { UserSignInDialogItems, UserSignInCompleteDialogItems } from "./UserSignInDialogItems";
import styles from "./UserAuthDialog.module.css";

/** The props used for the {@link UserSignInDialog} component. */
interface UserSignInDialogProps {
  /** Whether to show the title of the sign in dialog. */
  showTitle?: boolean;
}

/** A component that renders a user sign-in dialog. */
const UserSignInDialog: FC<UserSignInDialogProps> = ({ showTitle }) => {
  // We use the username and password to call signin endpoints.
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  // At first sign-in, first and last name needs to be completed.
  const [cognitoUser, setCognitoUser] = useState<CognitoUser | null>(null);
  const [firstname, setFirstname] = useState("");
  const [lastname, setLastname] = useState("");

  // Set an error message if applicable
  const [error, setError] = useState<string | null>(null);

  // We rely upon the user context to handle sign in/
  const { signIn, completeSignIn } = useContext(UserContext);

  // We use history to go back to the home page when sign-in is successful
  const history = useHistory();

  // The user name may be set in a query parameter
  const location = useLocation();
  useEffect(() => {
    const queryUsername = new URLSearchParams(location.search).get("username");
    setUsername(queryUsername ?? "");
  }, [location]);

  // This handles when the user presses the sign in button.
  const handleSignIn = async (event: React.FormEvent) => {
    event.preventDefault();
    if (password === "") {
      setError("Password is required.");
      return;
    }
    if (cognitoUser !== null && firstname === "") {
      setError("Username is required.");
      return;
    }
    if (cognitoUser !== null && lastname === "") {
      setError("Password is required.");
      return;
    }
    if (cognitoUser === null) {
      try {
        const cognitoUserResponse = (await signIn(
          username,
          password
        )) as CognitoUser;
        setError(null);
        if (cognitoUserResponse === null) {
          history.push("/");
        } else {
          setCognitoUser(cognitoUserResponse);
          setPassword("");
        }
      } catch (error: any) {
        setError(error.message);
      }
      // This is the first time a user signs in, which means a new password is required
    } else {
      try {
        await completeSignIn(cognitoUser, password, firstname, lastname);
        setError(null);
        history.push("/");
      } catch (error: any) {
        setError(error.message);
      }
    }
  };

  return (
    <form
      className={styles.dialog}
      onSubmit={(event) => {
        handleSignIn(event);
      }}
    >
      {cognitoUser === null ? (
        <>
          {showTitle && <Title>Sign In</Title>}
          <UserSignInDialogItems
            username={username}
            password={password}
            error={error}
            onUsernameChanged={setUsername}
            onPasswordChanged={setPassword}
          />
        </>
      ) : (
        <>
          {showTitle && <Title>Complete Sign In</Title>}
          <UserSignInCompleteDialogItems
            firstname={firstname}
            lastname={lastname}
            password={password}
            error={error}
            onFirstnameChanged={setFirstname}
            onLastnameChanged={setLastname}
            onPasswordChanged={setPassword}
          />
        </>
      )}
    </form>
  );
};

export default UserSignInDialog;
export type { UserSignInDialogProps };
