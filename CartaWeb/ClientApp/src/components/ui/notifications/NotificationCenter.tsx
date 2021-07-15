import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import NotificationContext from "../../../context/NotificationContext";
import NotificationAlert from "./NotificationAlert";
import "./NotificationCenter.css";

export interface NotificationCenterProps extends HTMLProps<HTMLDivElement> {}

export default class NotificationCenter extends Component<NotificationCenterProps> {
  render() {
    const { className, ...restProps } = this.props;
    return (
      <NotificationContext.Consumer>
        {({ notifications }) => {
          return (
            <div
              className={classNames(className, `notification-center`)}
              {...restProps}
            >
              {Object.entries(notifications).map(([index, notification]) => (
                <NotificationAlert
                  key={index}
                  index={Number(index)}
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
