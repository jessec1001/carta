import { FunctionComponent, useCallback, useEffect, useState } from "react";
import { Auth } from "aws-amplify";
import { CognitoUser } from "@aws-amplify/auth";
import { User } from "library/api";
import { LogSeverity } from "library/logging";
import { UserContext } from "components/user";
import { useNotifications } from "components/notifications";

/** Represents the information stored about local users. */
interface LocalCognitoUser extends CognitoUser {
  /** Attributes of the user stored in the Cognito user pool. */
  attributes: {
    /** The organization that the user belongs to. */
    "custom:tenantId": string;
    /** The email address of the user. */
    email: string;
    /** The first name of the user. */
    given_name: string;
    /** The last name of the user. */
    family_name: string;
    /** The unique user identifier. */
    sub: string;
  };
  /** The username of the user. */
  username: string;
}

/** A component that wraps a user context around its children components. */
const UserWrapper: FunctionComponent = ({ children }) => {
  // We use this state to store information about whether the user is authenticated and their relevant information.
  const [user, setUser] = useState<User | null>(null);

  // This method updates the user state.
  const { logger } = useNotifications();
  const updateAuthUser = useCallback(
    async (logErrors?: boolean) => {
      try {
        const user =
          (await Auth.currentAuthenticatedUser()) as LocalCognitoUser;
        setUser({
          name: user.username,
          firstName: user.attributes.given_name,
          lastName: user.attributes.family_name,
          email: user.attributes.email,
          id: user.attributes.sub,
          groups: [],
        });
      } catch (error: any) {
        if (logErrors) {
          logger.log({
            source: "User Wrapper",
            title: "Failed to fetch user information",
            severity: LogSeverity.Error,
            message: error.message,
          });
        } else setUser(null);
      }
    },
    [logger]
  );

  // When this component is mounted, we configure authentication.
  useEffect(() => {
    const fetchConfig = async () => {
      const response = await fetch("/auth/");
      const config = await response.json();
      Auth.configure(config);
      updateAuthUser();
    };
    fetchConfig();
  }, [updateAuthUser]);

  // Get the user information if they appear to be authenticated.
  useEffect(() => {
    updateAuthUser();
  }, [updateAuthUser]);

  // Prepare functions to handle sign in and sign out requests.
  const handleSignIn = async (username: string, password: string) => {
    // Execute sign in request.
    const user = (await Auth.signIn(username, password)) as LocalCognitoUser;
    setUser({
      name: user.username,
      firstName: user.attributes.given_name,
      lastName: user.attributes.family_name,
      email: user.attributes.email,
      id: user.attributes.sub,
      groups: [],
    });
  };
  const handleSignOut = async (apiless?: boolean) => {
    // Execute sign out request.
    try {
      if (!apiless) await Auth.signOut();
      setUser(null);
    } catch (error: any) {
      logger.log({
        source: "User Wrapper",
        title: "Failed to sign out",
        severity: LogSeverity.Error,
        message: error.message,
      });
    }
  };

  // Setup a timer to check when the tokens expire.
  useEffect(() => {
    let timeout: NodeJS.Timeout;
    const checkSession = async () => {
      try {
        if (!user) return;
        const session = await Auth.currentSession();
        if (!session.isValid()) {
          setUser(null);
        }
        timeout = setTimeout(
          checkSession,
          session.getAccessToken().getExpiration() * 1000
        );
      } catch (error: any) {
        logger.log({
          source: "User Wrapper",
          title: "Failed to check session",
          severity: LogSeverity.Error,
          message: error.message,
        });
      }
    };
    timeout = setTimeout(checkSession, 0);
    return () => clearTimeout(timeout);
  }, [logger, user]);

  return (
    <UserContext.Provider
      value={{
        user: user,
        authenticated: user !== null,

        signIn: handleSignIn,
        signOut: handleSignOut,
      }}
    >
      {children}
    </UserContext.Provider>
  );
};

export default UserWrapper;
