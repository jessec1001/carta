import { ComponentProps, FC } from "react";
import classNames from "classnames";
import styles from "./ButtonGroup.module.css";

/** The props used for the {@link ButtonGroup} component. */
interface ButtonGroupProps extends ComponentProps<"div"> {
  /** Whether the buttons in the group should be connected together. */
  connected?: boolean;
}

/** A component that renders a collection of buttons as a group. */
const ButtonGroup: FC<ButtonGroupProps> = ({
  connected,
  className,
  children,
  ...props
}) => {
  return (
    <div
      className={classNames(
        styles.buttonGroup,
        {
          [styles.connected]: connected,
        },
        className
      )}
      {...props}
    >
      {children}
    </div>
  );
};

export default ButtonGroup;
export type { ButtonGroup };
