import { FunctionComponent, useEffect, useRef, useState } from "react";
import { UserContext } from "context";
import { UserAPI, User } from "library/api";

/** A component that wraps a user context around its children components. */
const UserWrapper: FunctionComponent = ({ children }) => {
  // We need a reference to the user API to execute calls.
  const userApiRef = useRef(new UserAPI());
  const userApi = userApiRef.current;

  // We use this state to store information about whether the user is authenticated and their relevant information.
  // TODO: How might we communicate this authentication information with other tabs when the state changes?
  const [user, setUser] = useState<User | null>(null);
  const [authenticated, setAuthenticated] = useState<boolean>(false);

  // We check for authentication when this component is initialized.
  useEffect(() => {
    (async () => {
      if (await userApi.isAuthenticated()) {
        setUser(await userApi.getUserInfo());
        setAuthenticated(true);
      }
    })();
  }, [userApi]);

  // Prepare functions to handle sign in and sign out requests.
  const handleSignIn = async () => {
    // Execute sign in request.
    await userApi.signIn();

    // We must check that we are authenticated before we can retrieve user information.
    if (await userApi.isAuthenticated()) {
      setUser(await userApi.getUserInfo());
      setAuthenticated(true);
    }
  };
  const handleSignOut = async () => {
    // Execute sign out request.
    userApi.signOut();

    // We assume that authentication information has been cleared.
    setUser(null);
    setAuthenticated(false);
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
