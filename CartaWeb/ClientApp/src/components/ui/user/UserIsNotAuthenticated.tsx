import React, { Component } from "react";
import UserContext from "components/ui/user";

/** A component that only displays its children if the user is not authenticated. */
class UserIsNotAuthenticated extends Component {
  /** Renders the component into a DOM. */
  render() {
    const { children } = this.props;
    return (
      <UserContext.Consumer>
        {({ authenticated }) => !authenticated && children}
      </UserContext.Consumer>
    );
  }
}

// Export React component.
export default UserIsNotAuthenticated;
