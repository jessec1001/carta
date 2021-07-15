import { useContext } from "react";
import { User } from "library/api/user/types";
import { UserContext } from "context";

/**
 * If a user is currently authenticated, returns a specified user claim value. If the user is not currently
 * authenticated, returns `null` instead.
 * @param claim The user claim to obtain.
 * @returns The user claim value or `null` depending on the authentication state.
 */
const useUserValue = <T extends keyof User>(claim: T): User[T] | null => {
  // We try to use the user context if it is available.
  // If one isn't available, the user is guaranteed to be null.
  const { user } = useContext(UserContext);

  // Return the claim or null depending on whether we have a user object.
  if (user === null) return null;
  else return user[claim];
};

export default useUserValue;
