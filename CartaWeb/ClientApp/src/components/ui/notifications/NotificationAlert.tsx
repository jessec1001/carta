import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import { LogWidget, LogSeverity } from "library/logging";
import NotificationContext from "../../../context/NotificationContext";
import "./NotificationAlert.css";

export interface NotificationAlertProps extends HTMLProps<HTMLDivElement> {
  index: number;
  notification: LogWidget;
}

export default class NotificationAlert extends Component<NotificationAlertProps> {
  render() {
    const { index, notification, className, ...restProps } = this.props;
    const severityColor = {
      [LogSeverity.Debug]: "#dddddd",
      [LogSeverity.Info]: "#00ac46",
      [LogSeverity.Warning]: "#fdc500",
      [LogSeverity.Error]: "#dc0000",
      [LogSeverity.None]: "000000",
    }[notification.severity];
    return (
      <NotificationContext.Consumer>
        {({ logger }) => {
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
                  onClick={() => logger.closeNotification(index)}
                >
                  &times;
                </div>
              </div>
              <div className={`notification-alert-message`}>
                {notification.widget}
              </div>
            </div>
          );
        }}
      </NotificationContext.Consumer>
    );
  }
}
