import Logger, { LoggerOptions } from "../Logger";
import { LogEntry, LogWidget } from "../LogTypes";

interface SessionStorageLoggerOptions extends LoggerOptions {
  maxEntries?: number;
  storageKey?: string;
}

class SessionStorageLogger extends Logger {
  maxEntries: number;
  storageKey: string;

  public constructor(options: SessionStorageLoggerOptions) {
    super(options);

    this.maxEntries = options.maxEntries ?? 256;
    this.storageKey = options.storageKey ?? "log";
  }

  public log(entry: LogEntry) {
    if (this.logLevel <= entry.severity) {
      this.append(entry);
    }
  }

  private append(entry: LogEntry) {
    const storageEntries = sessionStorage.getItem(this.storageKey);
    const parsedEntries =
      storageEntries === null ? [] : (JSON.parse(storageEntries) as LogEntry[]);
  }
}

export default SessionStorageLogger;
