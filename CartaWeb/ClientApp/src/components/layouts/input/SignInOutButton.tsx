import { UserContext } from "App";
import React, { Component, HTMLProps } from "react";
import classNames from "classnames";

export interface SignInOutButtonProps extends HTMLProps<HTMLButtonElement> {}

export default class SignInOutButton extends Component<SignInOutButtonProps> {
  render() {
    const { className, children, type, ...restProps } = this.props;
    return (
      <UserContext.Consumer>
        {({ manager, user }) => {
          if (user === null) {
            return (
              <button
                onClick={() => manager.SignInAsync()}
                className={className}
                {...restProps}
              >
                Sign In
              </button>
            );
          } else {
            return (
              <button
                onClick={() => manager.SignOutAsync()}
                className={className}
                {...restProps}
              >
                Sign Out
              </button>
            );
          }
        }}
      </UserContext.Consumer>
    );
  }
}
