import { LogEntry } from "../LogTypes";

function DefaultWidget(entry: LogEntry) {
  return <span>{entry.message}</span>;
}

export default DefaultWidget;
