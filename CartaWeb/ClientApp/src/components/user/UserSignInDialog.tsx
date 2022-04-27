import { FC, useContext, useState } from "react";
import { Button, ButtonGroup } from "components/buttons";
import { TextFieldInput } from "components/input";
import { Text } from "components/text";
import UserContext from "./UserContext";
import styles from "./UserSignInDialog.module.css";

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
  const [error, setError] = useState<string | null>(null);

  // We rely upon the user context to handle sign in and sign out requests.
  const { signIn } = useContext(UserContext);

  // This handles when the user presses the sign in button.
  const handleSignIn = async () => {
    // Check for valid username and password.
    if (username === "") {
      setError("Username is required.");
      return;
    }
    if (password === "") {
      setError("Password is required.");
      return;
    }
    try {
      await signIn(username, password);
      setError(null);
    } catch (error: any) {
      setError(error.message);
    }
  };

  return (
    <form
      className={styles.dialog}
      onSubmit={(e) => {
        e.preventDefault();
        handleSignIn();
      }}
    >
      {showTitle && (
        <Text size="large" justify="center">
          Sign In
        </Text>
      )}
      <label className={styles.dialogField}>
        <span>Username</span>
        <TextFieldInput
          value={username}
          onChange={setUsername}
          placeholder="Username"
        />
      </label>
      <label className={styles.dialogField}>
        <span>Password</span>
        <TextFieldInput
          value={password}
          onChange={setPassword}
          placeholder="Password"
          password
        />
      </label>
      {error && (
        <Text color="error" size="small" padding="center">
          {error}
        </Text>
      )}
      <ButtonGroup stretch>
        <Button type="submit" sizing="bulky" className={styles.dialogButton}>
          Sign In
        </Button>
      </ButtonGroup>
    </form>
  );
};

export default UserSignInDialog;
export type { UserSignInDialogProps };
