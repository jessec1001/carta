import { createContext, useContext } from "react";
import { LogSeverity, LogWidget, NotificationLogger } from "library/logging";

/** The type of value used for the {@link NotificationsContext} */
interface INotificationsContext {
  /** The logger for notifications. */
  logger: NotificationLogger;
  /** The active notifications to display. */
  notifications: Record<number, LogWidget>;
}

/** A context for notifications logged throughout the application. */
const NotificationsContext = createContext<INotificationsContext | undefined>({
  logger: new NotificationLogger({ logLevel: LogSeverity.Debug }),
  notifications: {},
});

/**
 * Returns the context used to log notifications.
 * @return The state of the notifications system.
 */
const useNotifications = (): INotificationsContext => {
  const context = useContext(NotificationsContext);
  if (context === undefined) {
    throw new Error(
      "Notications context must be used within a notifications component."
    );
  }
  return context;
};

export default NotificationsContext;
export { useNotifications };
export type { INotificationsContext };
