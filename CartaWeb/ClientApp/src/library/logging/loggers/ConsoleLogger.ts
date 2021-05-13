import Logger from "../Logger";
import { LogEntry, LogSeverity } from "../LogTypes";

class ConsoleLogger extends Logger {
  public log(entry: LogEntry) {
    // Check that the log level is at least as severe as the log entry.
    if (this.logLevel <= entry.severity) {
      // Construct the log message and use the appropriate console function to display it.
      const logMessage = `[${entry.source}] ${entry.title}: ${entry.message}`;
      switch (entry.severity) {
        case LogSeverity.Debug:
          console.log(logMessage);
          break;
        case LogSeverity.Info:
          console.info(logMessage);
          break;
        case LogSeverity.Warning:
          console.warn(logMessage);
          break;
        case LogSeverity.Error:
          console.error(logMessage);
          break;
      }
    }
  }
}

export default ConsoleLogger;
