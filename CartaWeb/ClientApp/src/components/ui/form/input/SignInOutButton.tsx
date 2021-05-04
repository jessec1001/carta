import { NotificationContext, UserContext } from "components/pages/App";
import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import { ApiException } from "library/exceptions";

export interface SignInOutButtonProps extends HTMLProps<HTMLButtonElement> {}

export default class SignInOutButton extends Component<SignInOutButtonProps> {
  render() {
    const { className, children, type, ...restProps } = this.props;
    return (
      <UserContext.Consumer>
        {(userContext) => (
          <NotificationContext.Consumer>
            {(notificationsContext) => {
              const user = userContext.user;
              if (user === null) {
                return (
                  <button
                    onClick={() => {
                      userContext.manager
                        .SignInAsync()
                        .then(() =>
                          notificationsContext.manager.addNotification({
                            severity: "info",
                            title: "Signed In",
                            message: "You were successfully signed in",
                          })
                        )
                        .catch((err: ApiException) =>
                          notificationsContext.manager.addNotification({
                            severity: "error",
                            title: `Failed Sign-in - Error ${err.status}`,
                            message: err.message ?? "",
                          })
                        );
                    }}
                    className={className}
                    {...restProps}
                  >
                    Sign In
                  </button>
                );
              } else {
                return (
                  <button
                    onClick={() =>
                      userContext.manager
                        .SignOutAsync()
                        .then(() =>
                          notificationsContext.manager.addNotification({
                            severity: "info",
                            title: "Signed Out",
                            message: "You were successfully signed out",
                          })
                        )
                        .catch((err: ApiException) =>
                          notificationsContext.manager.addNotification({
                            severity: "error",
                            title: `Failed Sign-out - Error ${err.status}`,
                            message: err.message ?? "",
                          })
                        )
                    }
                    className={className}
                    {...restProps}
                  >
                    Sign Out
                  </button>
                );
              }
            }}
          </NotificationContext.Consumer>
        )}
      </UserContext.Consumer>
    );
  }
}
