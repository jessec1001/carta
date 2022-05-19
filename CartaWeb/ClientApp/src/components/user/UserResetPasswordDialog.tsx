import React, { FC, useContext, useState, useEffect } from "react";
import { useHistory, useLocation } from "react-router-dom";
import queryString from "query-string";
import { Button, ButtonGroup } from "components/buttons";
import { TextFieldInput } from "components/input";
import { Link } from "components/link";
import { Text, Title } from "components/text";
import UserContext from "./UserContext";
import styles from "./UserAuthDialog.module.css";

/** The props used for the {@link UserResetPasswordDialog} component. */
interface UserResetPasswordDialogProps {
    /** Whether to show the title of the sign in dialog. */
    showTitle?: boolean;
}

/** A component that renders a user reset password dialog. */
const UserResetPasswordDialog: FC<UserResetPasswordDialogProps> = ({
    showTitle,
}) => {
    // We use the username, verification code and new password to call reset password endpoints.
    const [username, setUsername] = useState("");
    const [code, setCode] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);

    // We rely upon the user context to handle reset password requests.
    const { resetPassword } = useContext(UserContext);

    // We use history to go back to the sign in page when password reset is successful
    const history = useHistory();

    // The code may be set in a query parameter
    const location = useLocation();
    useEffect(() => {
        const queryCode = new URLSearchParams(location.search).get("code");
        setCode(queryCode ?? "");
    }, [location]);

    // This handles when the user presses the sign in button.
    const handleForgotPassword = async (event: React.FormEvent) => {
        event.preventDefault();
        // Check for valid username and password.
        if (username === "") {
            setError("Username is required.");
            return;
        }
        if (code === "") {
            setError("Verification code is required.");
            return;
        }
        if (password === "") {
            setError("New password is required.");
            return;
        }
        try {
            await resetPassword(
                username,
                code,
                password);
            setError(null);
            const signinUrl = queryString.stringifyUrl({
                url: "signin",
                query: { username: username, },
            });
            history.push("/signin?username=" + username);
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
                    Reset Password
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
            <label className={styles.dialogField}>
                <span>Verification Code</span>
                <TextFieldInput
                    value={code}
                    onChange={setCode}
                    placeholder="######"
                    password
                />
            </label>
            <Text padding="center">
                If you do not have a code, you can request one <Link to="/signin/forgotpassword">here</Link>.
            </Text>
            <label className={styles.dialogField}>
                <span>New Password</span>
                <TextFieldInput
                    value={password}
                    onChange={setPassword}
                    placeholder="New Password"
                    password
                />
            </label>
            {error && (
                <Text color="error" size="small" padding="center">
                    {error}
                </Text>
            )}
            <ButtonGroup stretch>
                <Button
                    type="submit"
                    sizing="bulky"
                    className={styles.dialogButton}
                >
                    Reset Password
                </Button>
            </ButtonGroup>
        </form>
    );
};

export default UserResetPasswordDialog;
export type { UserResetPasswordDialogProps };
