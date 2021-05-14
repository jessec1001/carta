import React, { Component, createContext } from "react";
import Logging, {
  LogSeverity,
  LogWidget,
  NotificationLogger,
} from "library/logging";

interface NotificationContextValue {
  logger: NotificationLogger;
  notifications: Record<number, LogWidget>;
}

const NotificationContext = createContext<NotificationContextValue>({
  logger: new NotificationLogger({ logLevel: LogSeverity.Debug }),
  notifications: {},
});

interface NotificationContextWrapperProps {}
interface NotificationContextWrapperState {
  notifications: Record<number, LogWidget>;
}

class NotificationContextWrapper extends Component<
  NotificationContextWrapperProps,
  NotificationContextWrapperState
> {
  static displayName = NotificationContextWrapper.name;

  private logger: NotificationLogger;

  constructor(props: NotificationContextWrapperProps) {
    super(props);

    this.handleNotificationsChanged =
      this.handleNotificationsChanged.bind(this);

    this.state = {
      notifications: {},
    };

    this.logger = Logging.notification;
    this.logger.on("notificationOpened", this.handleNotificationsChanged);
    this.logger.on("notificationClosed", this.handleNotificationsChanged);
  }

  private handleNotificationsChanged() {
    this.setState({
      notifications: this.logger.activeNotifications,
    });
  }

  render() {
    const { children } = this.props;
    const { notifications } = this.state;

    return (
      <NotificationContext.Provider
        value={{
          logger: this.logger,
          notifications,
        }}
      >
        {children}
      </NotificationContext.Provider>
    );
  }
}

export default NotificationContext;
export type { NotificationContextValue };
export { NotificationContextWrapper };
export type {
  NotificationContextWrapperProps,
  NotificationContextWrapperState,
};
