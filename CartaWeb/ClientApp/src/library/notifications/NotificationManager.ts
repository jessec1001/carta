import { EventEmitter } from "ee-ts";
import { Notification } from ".";

interface NotificationEvents {
  notificationAdded: (notification: Notification) => void;
  notificationRemoved: (notification: Notification) => void;
}

class NotificationManager extends EventEmitter<NotificationEvents> {
  notifications: Notification[];

  constructor() {
    super();
    this.notifications = [];
  }

  clearNotification() {
    this.notifications.forEach((notification) =>
      this.removeNotification(notification)
    );
  }
  addNotification(notification: Notification) {
    this.notifications.push(notification);
    this.emit("notificationAdded", notification);
    setTimeout(() => this.removeNotification(notification), 10000);
  }
  removeNotification(notification: Notification) {
    const index = this.notifications.findIndex((note) => note === notification);
    if (index >= 0) {
      this.notifications.splice(index, 1);
      this.emit("notificationRemoved", notification);
    }
  }
}

export default NotificationManager;
