import { createContext } from "react";
import { User, UserManager } from "library/api/user/types";

/** The type of value of {@link UserContext}. */
interface UserContextValue {
  manager: UserManager;
  user: User | null;
  authenticated: boolean;
}

/** A context for user authentication that is updated when authentication is updated */
const UserContext = createContext<UserContextValue>({
  manager: new UserManager(),
  user: null,
  authenticated: false,
});

// Export React wrapper and context.
export default UserContext;
export type { UserContextValue };
