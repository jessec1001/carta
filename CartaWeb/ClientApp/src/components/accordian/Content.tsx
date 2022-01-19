import { FunctionComponent, HTMLAttributes } from "react";
import classNames from "classnames";
import { useAccordian } from "./Context";
import styles from "./Accordian.module.css";

/** The props used for the {@link Content} component. */
interface ContentProps extends HTMLAttributes<HTMLDivElement> {}

/** A component that contains the content for an {@link Accordian} component. */
const Content: FunctionComponent<ContentProps> = ({
  children,
  className,
  ...props
}) => {
  // Get the state of the accordian and the actions to operate on it.
  const { toggled } = useAccordian();

  // We return a component that should be rendered conditionally based on the toggled state.
  return (
    <div
      className={classNames(styles.content, className, {
        [styles.toggled]: toggled,
      })}
      {...props}
    >
      {children}
    </div>
  );
};

export default Content;
export type { ContentProps };
