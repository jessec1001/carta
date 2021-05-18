import { LogEntry, LogSeverity } from "./LogTypes";
import { EventEmitter } from "ee-ts";

interface LoggerOptions {
  logLevel: LogSeverity;
}

abstract class Logger<T = {}> extends EventEmitter<T> {
  public logLevel: LogSeverity;

  public constructor(options: LoggerOptions) {
    super();

    this.logLevel = options.logLevel;
  }

  public abstract log<TEntry extends LogEntry>(entry: TEntry): void;
}

// Export the logger and underlying types.
export default Logger;
export type { LoggerOptions };