import { FC } from "react";
import { Button, ButtonGroup } from "components/buttons";
import { Link } from "components/link";
import { TextFieldInput } from "components/input";
import { Text } from "components/text";
import styles from "./UserAuthDialog.module.css";

/** The props for the {@link UserSignInDialogItems} component. */
interface UserSignInDialogItemsProps {
    username: string;
    password: string;
    error: string | null;
    onUsernameChanged?: (name: string) => void;
    onPasswordChanged?: (name: string) => void;
}

/** Items displayed for user sign in */
const UserSignInDialogItems: FC<UserSignInDialogItemsProps> = ({
    username,
    password,
    error,
    onUsernameChanged = () => { },
    onPasswordChanged = () => { },
}) => {

    return (
        <>
            <label className={styles.dialogField}>
                <span>Username</span>
                <TextFieldInput
                    value={username}
                    onChange={onUsernameChanged}
                    placeholder="Username"
                />
            </label>
            <label className={styles.dialogField}>
                <span>Password</span>
                <TextFieldInput
                    value={password}
                    onChange={onPasswordChanged}
                    placeholder="Password"
                    password
                />
            </label>
            {error && (
                <Text color="error" size="small" padding="center">
                    {error}
                </Text>
            )}
            <Text padding="bottom" justify="center">
                <Link to="/signin/forgotpassword">Forgot Password?</Link>
            </Text>
            <ButtonGroup stretch>
                <Button
                    type="submit"
                    sizing="bulky"
                    className={styles.dialogButton}
                >
                    Sign In
                </Button>
            </ButtonGroup>
        </>
    );

}

/** The props for the {@link UserSignInCompleteDialogItems} component. */
interface UserSignInCompleteDialogItemsProps {
    firstname: string;
    lastname: string;
    password: string;
    error: string | null;
    onFirstnameChanged?: (name: string) => void;
    onLastnameChanged?: (name: string) => void;
    onPasswordChanged?: (name: string) => void;
}

/** Items displayed for user sign in */
const UserSignInCompleteDialogItems: FC<UserSignInCompleteDialogItemsProps> = ({
    firstname,
    lastname,
    password,
    error,
    onFirstnameChanged = () => { },
    onLastnameChanged = () => { },
    onPasswordChanged = () => { },
}) => {

    return (
        <>
            <label className={styles.dialogField}>
                <span>First name</span>
                <TextFieldInput
                    value={firstname}
                    onChange={onFirstnameChanged}
                    placeholder="First Name"
                />
            </label>
            <label className={styles.dialogField}>
                <span>Last name</span>
                <TextFieldInput
                    value={lastname}
                    onChange={onLastnameChanged}
                    placeholder="Last Name"
                />
            </label>
            <label className={styles.dialogField}>
                <span>New Password</span>
                <TextFieldInput
                    value={password}
                    onChange={onPasswordChanged}
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
                    Sign In
                </Button>
            </ButtonGroup>
        </>
    );

}

export { UserSignInDialogItems, UserSignInCompleteDialogItems };
