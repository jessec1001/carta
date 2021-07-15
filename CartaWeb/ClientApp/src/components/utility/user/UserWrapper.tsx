import { Component } from "react";
import { UserContext } from "context";
import { User, UserManager } from "library/api/user/types";

/** The props used for the {@link UserContextWrapper} component. */
interface UserWrapperProps {}
/** The state used for the {@link UserContextWrapper} component. */
interface UserWrapperState {
  user: User | null;
  authenticated: boolean;
}

/** A component that wraps a user context around its children components. */
class UserWrapper extends Component<UserWrapperProps, UserWrapperState> {
  /** Used for debugging display name readability. */
  static displayName = UserWrapper.name;

  private manager: UserManager;

  /**
   * Creates an instance of the {@link UserContextWrapper} component.
   * @param props The prop values.
   */
  constructor(props: UserWrapperProps) {
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

export default UserWrapper;
export type { UserWrapperProps, UserWrapperState };
