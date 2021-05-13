import React, { Component, createContext } from "react";
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

/** The props used for the {@link UserContextWrapper} component. */
interface UserContextWrapperProps {}
/** The state used for the {@link UserContextWrapper} component. */
interface UserContextWrapperState {
  user: User | null;
  authenticated: boolean;
}

/** A component that wraps a user context around its children components. */
class UserContextWrapper extends Component<
  UserContextWrapperProps,
  UserContextWrapperState
> {
  /** Used for debugging display name readability. */
  static displayName = UserContextWrapper.name;

  private manager: UserManager;

  /**
   * Creates an instance of the {@link UserContextWrapper} component.
   * @param props The prop values.
   */
  constructor(props: UserContextWrapperProps) {
    super(props);

    // Bind event handlers.
    this.handleUserSignin = this.handleUserSignin.bind(this);
    this.handleUserSignout = this.handleUserSignout.bind(this);

    // Set initial logged out state.
    this.state = {
      user: null,
      authenticated: false,
    };

    // Set up the user manager and attach the relevant event handlers.
    this.manager = new UserManager();
    this.manager.on("signin", this.handleUserSignin);
    this.manager.on("signout", this.handleUserSignout);
  }

  /**
   * Handles when the user is authenticated and signed-in.
   * @param user The signed in user.
   */
  private handleUserSignin(user: User) {
    this.setState({
      user: user,
      authenticated: true,
    });
  }
  /**
   * Handles when the user is unauthenticated and signed-out.
   */
  private handleUserSignout() {
    this.setState({
      user: null,
      authenticated: false,
    });
  }

  /** Renders the component into a DOM. */
  render() {
    // Get the relevant rendered values of this object.
    const { children } = this.props;
    const { user, authenticated } = this.state;

    // Simply wrap the children in a context provider.
    return (
      <UserContext.Provider
        value={{
          manager: this.manager,
          user,
          authenticated,
        }}
      >
        {children}
      </UserContext.Provider>
    );
  }
}

// Export React wrapper and context.
export default UserContext;
export type { UserContextValue };
export { UserContextWrapper };
export type { UserContextWrapperProps, UserContextWrapperState };
