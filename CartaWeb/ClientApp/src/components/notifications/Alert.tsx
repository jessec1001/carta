import classNames from "classnames";
import { ComponentProps, FC } from "react";
import { AppColors } from "app";
import { LogWidget, LogSeverity } from "library/logging";
import { CloseButton } from "components/buttons";
import { useNotifications } from "./Context";
import bgStyles from "styles/background.module.css";
import styles from "./Alert.module.css";

/** The props used for the {@link Alert} component. */
interface AlertProps extends Omit<ComponentProps<"div">, "id"> {
  /** The identifier of the notification within the logger. */
  id: number;
  /** The notification widget. */
  notification: LogWidget;
}

/** Renders a notification alert. */
const Alert: FC<AlertProps> = ({
  id,
  notification,
  className,
  children,
  ...props
}) => {
  // We need access to the logger to handle actions performed on the notification.
  const { logger } = useNotifications();

  // Determine the severity color.
  const severityColors: Record<LogSeverity, AppColors> = {
    [LogSeverity.Debug]: "muted",
    [LogSeverity.Info]: "info",
    [LogSeverity.Warning]: "warning",
    [LogSeverity.Error]: "error",
    [LogSeverity.None]: "normal",
  };
  const severityColor: AppColors = severityColors[notification.severity];

  return (
    <div className={classNames(styles.alert, className)} {...props}>
      <div className={styles.header}>
        <div className={classNames(styles.severity, bgStyles[severityColor])} />
        <div className={styles.title}>{notification.title}</div>
        <CloseButton onClick={() => logger.closeNotification(id)} />
      </div>
      {notification.widget ? notification.widget : notification.message}
    </div>
  );
};

export default Alert;
export type { AlertProps };
