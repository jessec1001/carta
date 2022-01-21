import { ComponentProps, FC } from "react";
import classNames from "classnames";
import styles from "./ButtonGroup.module.css";

/** The props used for the {@link ButtonGroup} component. */
interface ButtonGroupProps extends ComponentProps<"div"> {
  /** Whether the buttons in the group should be connected together. Defaults to false. */
  connected?: boolean;
  /** Whether the button group should be stretched. Defaults to false. */
  stretch?: boolean;
}

/** A component that renders a collection of buttons as a group. */
const ButtonGroup: FC<ButtonGroupProps> = ({
  connected = false,
  stretch = false,
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
          [styles.stretch]: stretch,
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
