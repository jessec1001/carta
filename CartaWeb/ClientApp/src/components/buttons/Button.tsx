import { ComponentProps, FC } from "react";
import classNames from "classnames";
import { AppColors } from "app";
import styles from "./Button.module.css";
import baseStyles from "./ButtonBase.module.css";
import fgStyles from "styles/foreground.module.css";
import bgStyles from "styles/background.module.css";

/** The props used for the {@link BlockButton} component. */
interface BlockButtonProps extends ComponentProps<"button"> {
  /** The color of the button. Defaults to primary. */
  color?: AppColors;
  /** Whether the button should be rendered as an outline. Defaults to false. */
  outline?: boolean;
}

/** A simple button component that is filled visually. */
const BlockButton: FC<BlockButtonProps> = ({
  type = "button",
  color = "primary",
  outline = false,
  children,
  className,
  ...props
}) => {
  return (
    <button
      type={type}
      className={classNames(
        baseStyles.button,
        styles.button,
        {
          [styles.outline]: outline,
          [fgStyles[color]]: outline,
          [bgStyles[color]]: !outline,
        },
        className
      )}
      {...props}
    >
      {children}
    </button>
  );
};

export default BlockButton;
export type { BlockButtonProps };
