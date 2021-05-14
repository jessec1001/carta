import Logger, { LoggerOptions } from "../Logger";
import { LogEntry } from "../LogTypes";

interface SessionStorageLoggerOptions extends LoggerOptions {
  maxEntries?: number;
  storageKey?: string;
}

class SessionStorageLogger extends Logger {
  public maxEntries: number;
  public storageKey: string;

  public constructor(options: SessionStorageLoggerOptions) {
    super(options);

    this.maxEntries = options.maxEntries ?? 256;
    this.storageKey = options.storageKey ?? "log";
  }

  public log(entry: LogEntry) {
    if (this.logLevel <= entry.severity) {
      this.appendEntry(entry);
    }
  }

  private appendEntry(entry: LogEntry) {
    const storageEntries = sessionStorage.getItem(this.storageKey);
    const parsedEntries =
      storageEntries === null ? [] : (JSON.parse(storageEntries) as LogEntry[]);

    parsedEntries.splice(this.maxEntries - 1);
    parsedEntries.unshift(entry);

    sessionStorage.setItem(this.storageKey, JSON.stringify(parsedEntries));
  }
}

export default SessionStorageLogger;
