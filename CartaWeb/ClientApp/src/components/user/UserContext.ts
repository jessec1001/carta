import { createContext } from "react";
import { User } from "library/api";
import { CognitoUser } from "@aws-amplify/auth";
import { LocalCognitoCodeDelivery } from "./CognitoTypes";

/** The type of value of {@link UserContext}. */
interface UserContextValue {
  /** The user information if authenticated; otherwise null. */
  user: User | null;
  /** Whether the user is currently authenticated. */
  authenticated: boolean;

  /** Signs the user in. */
  signIn: (username: string, password: string) => Promise<CognitoUser | null>;
  /** Signs the user out. */
  signOut: (apiless?: boolean) => Promise<void>;
  /** Completes sign in. */
  completeSignIn: (
    user: CognitoUser | null,
    password: string,
    firstname: string,
    lastname: string
  ) => Promise<void>;
  /** Requests password reset. */
  forgotPassword: (
    username: string
  ) => Promise<LocalCognitoCodeDelivery | undefined>;
  /** Reset password. */
  resetPassword: (
    username: string,
    code: string,
    password: string
  ) => Promise<void>;
}

/** A context for user authentication that is updated when authentication is updated */
const UserContext = createContext<UserContextValue>({
  user: null,
  authenticated: false,
  signIn: async () => null,
  completeSignIn: async () => { },
  signOut: async () => { },
  forgotPassword: async () => undefined,
  resetPassword: async () => { },
});

export default UserContext;
export type { UserContextValue };
