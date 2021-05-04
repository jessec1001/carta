export default interface Notification {
  severity: "debug" | "info" | "warning" | "error";
  title: string;
  message: string;
}
