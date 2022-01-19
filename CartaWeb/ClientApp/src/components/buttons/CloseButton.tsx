import { ComponentProps, FC } from "react";
import classNames from "classnames";
import baseStyles from "./ButtonBase.module.css";
import styles from "./CloseButton.module.css";

/** The props used for the {@link CloseButton} component. */
interface CloseButtonProps extends ComponentProps<"button"> {}

/** A general-purpose close button to indicate a closeable element. */
const CloseButton: FC<CloseButtonProps> = ({
  type = "button",
  children,
  className,
  ...props
}) => {
  return (
    <button
      type={type}
      className={classNames(baseStyles.button, styles.button, className)}
      {...props}
    >
      &times;
    </button>
  );
};

export default CloseButton;
export type { CloseButtonProps };
