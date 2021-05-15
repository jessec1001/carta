enum LogSeverity {
  Debug = 0,
  Info = 1,
  Warning = 2,
  Error = 3,
  None = 4,
}
interface LogEntry {
  severity: LogSeverity;
  source: string;
  title: string;
  message?: string;
  data?: any;
}
interface LogWidget extends LogEntry {
  widget: JSX.Element;
  sticky?: boolean;
}

export { LogSeverity };
export type { LogEntry, LogWidget };
