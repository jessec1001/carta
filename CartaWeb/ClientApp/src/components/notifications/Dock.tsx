import classNames from "classnames";
import { ComponentProps, FC } from "react";
import NotificationAlert from "./Alert";
import { useNotifications } from "./Context";
import styles from "./Dock.module.css";

/** The props used for the {@link Dock} component. */
interface DockProps extends ComponentProps<"div"> {}

/** Renders a dock to display a list of notifications. */
const Dock: FC<DockProps> = ({ className, children, ...props }) => {
  // We need the notifications to render.
  const { notifications } = useNotifications();

  return (
    <div className={classNames(styles.dock, className)} {...props}>
      {Object.entries(notifications).map(([id, notification]) => (
        <NotificationAlert
          key={id}
          id={Number(id)}
          notification={notification}
        />
      ))}
    </div>
  );
};

export default Dock;
export type { DockProps };
