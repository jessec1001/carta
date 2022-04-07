import classNames from "classnames";
import { ComponentProps, forwardRef, useEffect } from "react";
import { useViews } from "./Context";
import styles from "./Container.module.css";

/** The props used for the {@link Container} component. */
interface ContainerProps extends Omit<ComponentProps<"div">, "title"> {
  /** The title of the view to display in the tab for the view. */
  title?: React.ReactNode;
  /** Whether the view should be closeable or not. Defaults to true. */
  closeable?: boolean;
  /** The status of the view to display in the tab for the view. */
  status?: "none" | "modified" | "unmodified" | "info" | "warning" | "error";

  /** Whether the view container should be padded. */
  padded?: boolean;
  /** The direction that the view container should be scrollable or filled. */
  direction?: "horizontal" | "vertical" | "fill";
}

/** A container for a particular view in a views context. */
const Container = forwardRef<HTMLDivElement, ContainerProps>(
  (
    {
      title,
      closeable,
      status,
      padded,
      direction = "fill",
      children,
      ...props
    },
    ref
  ) => {
    // We set the options on the view when specified.
    const { actions } = useViews();
    useEffect(() => {
      actions.setOptions({ title, closeable, status });
    }, [title, closeable, actions, status]);

    // TODO: Auto-add history based on some prop.
    return (
      <div
        ref={ref}
        className={classNames(
          styles.container,
          { [styles.padded]: padded },
          styles[direction]
        )}
        {...props}
      >
        <div className={styles.containerInternal}>{children}</div>
      </div>
    );
  }
);

export default Container;
