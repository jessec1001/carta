import { createContext } from "react";
import { LogSeverity, LogWidget, NotificationLogger } from "library/logging";

/** The type of value of {@link NotificationContext}. */
interface NotificationContextValue {
  /** The logger for notifications. */
  logger: NotificationLogger;
  /** The active notifications to display. */
  notifications: Record<number, LogWidget>;
}

/** A context for notifications logged throughout the application. */
const NotificationContext = createContext<NotificationContextValue>({
  logger: new NotificationLogger({ logLevel: LogSeverity.Debug }),
  notifications: {},
});

export default NotificationContext;
export type { NotificationContextValue };
