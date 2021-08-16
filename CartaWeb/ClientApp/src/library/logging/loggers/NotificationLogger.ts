import Logger, { LoggerOptions } from "../Logger";
import { LogEntry, LogWidget } from "../LogTypes";
import { DefaultWidget } from "../widgets";

interface NotificationLoggerOptions extends LoggerOptions {
  maxEntries?: number;
  maxTimeout?: number;
}
interface NotificationLoggerEvents {
  notificationOpened: (widget: LogWidget, index: number) => void;
  notificationClosed: (widget: LogWidget, index: number) => void;
}

class NotificationLogger extends Logger<NotificationLoggerEvents> {
  private notificationIndex: number;
  private notifications: Record<number, LogWidget>;
  private visible: number[];

  public maxEntries: number;
  public maxTimeout: number;

  public get activeNotifications() {
    return this.visible
      .map((index) => ({ [index]: this.notifications[index] }))
      .reduce((accumulate, value) => ({ ...accumulate, ...value }), {});
  }
  public get hiddenNotifications() {
    return Object.keys(this.notifications)
      .map(Number)
      .filter((index) => !this.visible.includes(index))
      .reduce(
        (accumulate, index) => ({
          ...accumulate,
          index: this.notifications[index],
        }),
        {}
      );
  }

  public constructor(options: NotificationLoggerOptions) {
    super(options);

    this.notificationIndex = 0;
    this.notifications = {};
    this.visible = [];

    this.maxEntries = options.maxEntries ?? 4;
    this.maxTimeout = options.maxTimeout ?? 5000;
  }

  public log(entry: LogEntry | LogWidget) {
    if (this.logLevel <= entry.severity) {
      // Assign a widget if there is not one already.
      if (!("widget" in entry))
        entry = { ...entry, widget: DefaultWidget(entry) };
      this.appendNotification(entry);
    }
  }

  private appendNotification(entry: LogWidget) {
    let index = this.notificationIndex++;
    this.notifications[index] = entry;

    if (this.visible.length < this.maxEntries) this.openNotification(index);
  }
  private nextNotification() {
    const indices = Object.keys(this.notifications).map(Number);
    if (indices.length === 0) return;

    const index = Math.min(...indices);
    this.openNotification(index);
  }

  public openNotification(index: number) {
    const notification = this.notifications[index];
    if (notification !== undefined) {
      this.visible.unshift(index);
      if (!notification.sticky) {
        setTimeout(() => this.closeNotification(index), this.maxTimeout);
      }
      this.notify("notificationOpened", notification, index);
    }
  }
  public closeNotification(index: number) {
    const notification = this.notifications[index];
    if (notification !== undefined) {
      this.visible = this.visible.filter(
        (visibleIndex) => visibleIndex !== index
      );
      delete this.notifications[index];
      this.notify("notificationClosed", notification, index);
    }

    if (this.visible.length < this.maxEntries) this.nextNotification();
  }
  public clearNotifications() {
    Object.keys(this.notifications)
      .map(Number)
      .forEach((index) => this.closeNotification(index));
  }
}

export default NotificationLogger;
