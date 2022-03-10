import { FC, useEffect, useRef, useState } from "react";
import Logging, { LogWidget } from "library/logging";
import NotificationsContext from "./Context";
import Alert from "./Alert";
import Dock from "./Dock";

// TODO: Make the notifications component more configurable.
/** The props used for the {@link Notifications} component. */
interface NotificationsProps {}

/**
 * Defines the composition of the compound {@link Notifications} component.
 * @borrows Dock as Dock
 * @borrows Alert as Alert
 */
interface NotificationsComposition {
  Alert: typeof Alert;
  Dock: typeof Dock;
}

/** A component that renders a collection of ordered notifications. */
const Notifications: FC<NotificationsProps> & NotificationsComposition = ({
  children,
  ...props
}) => {
  // Store the notifications in the state.
  const logger = useRef(Logging.notification);
  const [notifications, setNotifications] = useState<Record<number, LogWidget>>(
    {}
  );

  // Whenever the notifications logger changes, update the notifications state.
  useEffect(() => {
    const currentLogger = logger.current;
    const updateNotifications = () => {
      setNotifications(currentLogger.activeNotifications);
    };

    currentLogger.on("notificationOpened", updateNotifications);
    currentLogger.on("notificationClosed", updateNotifications);
    return () => {
      currentLogger.off("notificationOpened", updateNotifications);
      currentLogger.off("notificationClosed", updateNotifications);
    };
  }, []);

  return (
    <NotificationsContext.Provider
      value={{ logger: logger.current, notifications: notifications }}
    >
      {children}
    </NotificationsContext.Provider>
  );
};
Notifications.Alert = Alert;
Notifications.Dock = Dock;

export default Notifications;
export type { NotificationsProps };
