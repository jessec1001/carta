import {
  ConsoleLogger,
  NotificationLogger,
  SessionStorageLogger,
} from "./loggers";
import { LogSeverity, LogEntry } from "./LogTypes";

class Logging {
  public static console: ConsoleLogger = new ConsoleLogger({
    logLevel: LogSeverity.Debug,
  });
  public static sessionStorage: SessionStorageLogger = new SessionStorageLogger(
    { logLevel: LogSeverity.Info }
  );
  public static notification: NotificationLogger = new NotificationLogger({
    logLevel: LogSeverity.Info,
  });

  public static get loggers() {
    return [Logging.console, Logging.sessionStorage, Logging.notification];
  }

  public static log(entry: LogEntry) {
    Logging.loggers.forEach((logger) => logger.log(entry));
  }
}

export default Logging;
