import { FunctionComponent, useEffect, useState } from "react";
import { useAPI, useStoredState } from "hooks";
import { UserContext } from "context";
import { User } from "library/api";

/** A component that wraps a user context around its children components. */
const UserWrapper: FunctionComponent = ({ children }) => {
  // The maximum amount of time the user can be authenticated (30 minutes).
  const maxAuthTimespan = 30 * 60 * 1000;

  // We need a reference to the user API to execute calls.
  const { userAPI } = useAPI();

  // We use this state to store information about whether the user is authenticated and their relevant information.
  const [user, setUser] = useState<User | null>(null);
  const [authTimestamp, setAuthTimestamp] = useStoredState(0, "authTimestamp");
  const [authenticated, setAuthenticated] = useState<boolean>(
    Date.now() - authTimestamp < maxAuthTimespan
  );

  // Get the user information if they appear to be authenticated.
  useEffect(() => {
    if (Date.now() - authTimestamp < maxAuthTimespan) {
      (async () => {
        if (await userAPI.isAuthenticated()) {
          setUser(await userAPI.getUserInfo());
          setAuthenticated(true);
        }
      })();
    }
  }, [userAPI, authTimestamp, maxAuthTimespan]);

  // Set a timer to automatically deauthenticate user when their session appears to expire.
  useEffect(() => {
    // We only setup/teardown a timer if the user is authenticated.
    const authenticationRefreshId = setTimeout(() => {
      // When the timer callback is hit, we clear the user.
      setUser(null);
      setAuthenticated(false);
    }, Math.max(0, maxAuthTimespan - (Date.now() - authTimestamp)));
    return () => clearTimeout(authenticationRefreshId);
  }, [authTimestamp, maxAuthTimespan]);

  // Prepare functions to handle sign in and sign out requests.
  const handleSignIn = async () => {
    // Execute sign in request.
    await userAPI.signIn();

    // We must check that we are authenticated before we can retrieve user information.
    if (await userAPI.isAuthenticated()) {
      setUser(await userAPI.getUserInfo());
      setAuthenticated(true);
      setAuthTimestamp(Date.now());
    }
  };
  const handleSignOut = async () => {
    // Execute sign out request.
    userAPI.signOut();

    // We assume that authentication information has been cleared.
    setUser(null);
    setAuthenticated(false);
    setAuthTimestamp(0);
  };

  return (
    <UserContext.Provider
      value={{
        user,
        authenticated,

        signIn: handleSignIn,
        signOut: handleSignOut,
      }}
    >
      {children}
    </UserContext.Provider>
  );
};

export default UserWrapper;
