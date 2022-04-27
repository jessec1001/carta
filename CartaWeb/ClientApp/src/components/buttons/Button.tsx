import { ComponentProps, FC } from "react";
import classNames from "classnames";
import { AppColors } from "app";
import styles from "./Button.module.css";
import baseStyles from "./ButtonBase.module.css";
import fgStyles from "styles/foreground.module.css";
import bgStyles from "styles/background.module.css";

/** The props used for the {@link Button} component. */
interface ButtonProps extends ComponentProps<"button"> {
  /** The color of the button. Defaults to primary. */
  color?: AppColors;
  /** Whether the button should be rendered as an outline. Defaults to false. */
  outline?: boolean;
  /** The sizing of the butotn. */
  sizing?: "normal" | "bulky";
}

/** A simple button component that is filled visually. */
const Button: FC<ButtonProps> = ({
  type = "button",
  color = "primary",
  outline = false,
  sizing = "normal",
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
          [styles.bulky]: sizing === "bulky",
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

export default Button;
export type { ButtonProps };
