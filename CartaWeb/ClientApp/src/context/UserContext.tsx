import { createContext } from "react";
import { User, UserManager } from "library/api/user/types";

/** The type of value of {@link UserContext}. */
interface UserContextValue {
  /** The manager for the user account. */
  manager: UserManager;
  /** The user information if authenticated; otherwise null. */
  user: User | null;
  /** Whether the user is currently authenticated. */
  authenticated: boolean;
}

/** A context for user authentication that is updated when authentication is updated */
const UserContext = createContext<UserContextValue>({
  manager: new UserManager(),
  user: null,
  authenticated: false,
});

export default UserContext;
export type { UserContextValue };
