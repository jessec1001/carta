import { FC, useContext, useState } from "react";
import { Button, ButtonGroup } from "components/buttons";
import { TextFieldInput } from "components/input";
import { Link } from "components/link";
import { Text, Title } from "components/text";
import UserContext from "./UserContext";
import styles from "./UserAuthDialog.module.css";

/** The props used for the {@link UserForgotPasswordDialog} component. */
interface UserForgotPasswordDialogProps {
  /** Whether to show the title of the sign in dialog. */
  showTitle?: boolean;
}

/** A component that renders a user reset password dialog. */
const UserForgotPasswordDialog: FC<UserForgotPasswordDialogProps> = ({
  showTitle
}) => {
  // We use the username and verifification to call forgot password endpoints.
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  // We rely upon the user context to handle forgot password requests.
  const { forgotPassword } = useContext(UserContext);

  // This handles when the user presses the sign in button.
  const handleForgotPassword = async (event: React.FormEvent) => {
    event.preventDefault();
    // Check for valid username and password.
    if (username === "") {
      setError("Username is required.");
      return;
    }
    try {
      const codeDelivery = await forgotPassword(username);
      if (codeDelivery) setEmail(codeDelivery.destination);
      setError(null);
    } catch (error: any) {
      setError(error.message);
    }
  };

  return (
    <form
      className={styles.dialog}
      onSubmit={(event) => {
        handleForgotPassword(event);
      }}
    >
      {showTitle && (
        <Title>
          Forgot Password
        </Title>
      )}
      <label className={styles.dialogField}>
        <span>Username</span>
        <TextFieldInput
          value={username}
          onChange={setUsername}
          placeholder="Username"
        />
      </label>
      {error && (
        <Text color="error" size="small" padding="center">
          {error}
        </Text>
      )}
      {email && (
        <Text size="small" padding="center">
          Your verification code has been emailed to {email}
        </Text>
      )}
      <Text padding="center">
        Enter your username to receive a password reset email.
      </Text>
      <Text padding="bottom" justify="center">
        <Link to="/signin/resetpassword">
          I have a verification code.
        </Link>
      </Text>
      <ButtonGroup stretch>
        <Button
          type="submit"
          sizing="bulky"
          className={styles.dialogButton}
        >
          Email Code
        </Button>
      </ButtonGroup>
    </form>
  );
};

export default UserForgotPasswordDialog;
export type { UserForgotPasswordDialogProps };
