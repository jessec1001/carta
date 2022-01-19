import { ComponentProps, FC } from "react";
import classNames from "classnames";
import baseStyles from "./ButtonBase.module.css";
import styles from "./IconButton.module.css";

/** The props used for the {@link IconButton} component. */
interface IconButtonProps extends ComponentProps<"button"> {
  /** How the icon button should be shaped. By default, a circle. */
  shape?: "circle" | "square";
}

/** A component that represents a small round button with an icon in its center. */
const IconButton: FC<IconButtonProps> = ({
  type = "button",
  shape = "circle",
  children,
  className,
  ...props
}) => {
  return (
    <button
      type={type}
      className={classNames(
        baseStyles.button,
        styles.iconButton,
        styles[shape],
        className
      )}
      {...props}
    >
      {children}
    </button>
  );
};

export default IconButton;
