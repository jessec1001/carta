import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import { NotificationAlert } from ".";
import { NotificationContext } from "App";
import "./NotificationCenter.css";

export interface NotificationCenterProps extends HTMLProps<HTMLDivElement> {}

export default class NotificationCenter extends Component<NotificationCenterProps> {
  render() {
    const { className, ...restProps } = this.props;
    return (
      <NotificationContext.Consumer>
        {({ manager, notifications }) => {
          return (
            <div
              className={classNames(className, `notification-center`)}
              {...restProps}
            >
              {notifications.map((notification) => (
                <NotificationAlert
                  key={notification.title}
                  notification={notification}
                />
              ))}
            </div>
          );
        }}
      </NotificationContext.Consumer>
    );
  }
}
