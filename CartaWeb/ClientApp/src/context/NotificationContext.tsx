import { createContext } from "react";
import { LogSeverity, LogWidget, NotificationLogger } from "library/logging";

interface NotificationContextValue {
  logger: NotificationLogger;
  notifications: Record<number, LogWidget>;
}

const NotificationContext = createContext<NotificationContextValue>({
  logger: new NotificationLogger({ logLevel: LogSeverity.Debug }),
  notifications: {},
});

export default NotificationContext;
export type { NotificationContextValue };
