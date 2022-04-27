import { createContext } from "react";
import { User } from "library/api";

/** The type of value of {@link UserContext}. */
interface UserContextValue {
  /** The user information if authenticated; otherwise null. */
  user: User | null;
  /** Whether the user is currently authenticated. */
  authenticated: boolean;

  /** Signs the user in. */
  signIn: (username: string, password: string) => Promise<void>;
  /** Signs the user out. */
  signOut: (apiless?: boolean) => Promise<void>;
}

/** A context for user authentication that is updated when authentication is updated */
const UserContext = createContext<UserContextValue>({
  user: null,
  authenticated: false,
  signIn: async () => {},
  signOut: async () => {},
});

export default UserContext;
export type { UserContextValue };
