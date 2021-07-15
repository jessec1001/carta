import { Component } from "react";
import Logging, { LogWidget, NotificationLogger } from "library/logging";
import { NotificationContext } from "context";

interface NotificationWrapperProps {}
interface NotificationWrapperState {
  notifications: Record<number, LogWidget>;
}

class NotificationWrapper extends Component<
  NotificationWrapperProps,
  NotificationWrapperState
> {
  static displayName = NotificationWrapper.name;

  private logger: NotificationLogger;

  constructor(props: NotificationWrapperProps) {
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

export default NotificationWrapper;
export type { NotificationWrapperProps, NotificationWrapperState };
