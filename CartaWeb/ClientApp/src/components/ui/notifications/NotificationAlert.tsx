import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import { Notification } from "library/notifications";
import "./NotificationAlert.css";
import { NotificationContext } from "components/pages/App";

export interface NotificationAlertProps extends HTMLProps<HTMLDivElement> {
  notification: Notification;
}

export default class NotificationAlert extends Component<NotificationAlertProps> {
  render() {
    const { notification, className, ...restProps } = this.props;
    const severityColor = {
      debug: "#dddddd",
      info: "#00ac46",
      warning: "#fdc500",
      error: "#dc0000",
    }[notification.severity];
    return (
      <NotificationContext.Consumer>
        {({ manager, notifications }) => {
          return (
            <div
              className={classNames(className, `notification-alert`)}
              {...restProps}
            >
              <div className={`notification-alert-header`}>
                <div
                  style={{ backgroundColor: severityColor }}
                  className={`notification-alert-severity`}
                ></div>
                <span className={`notification-alert-title`}>
                  {notification.title}
                </span>
                <div
                  className={`notification-alert-close`}
                  onClick={() => manager.removeNotification(notification)}
                >
                  &times;
                </div>
              </div>
              <div className={`notification-alert-message`}>
                {notification.message}
              </div>
            </div>
          );
        }}
      </NotificationContext.Consumer>
    );
  }
}
