import { FunctionComponent, HTMLProps } from "react";
import { Modify } from "types";
import classNames from "classnames";

import "./BlockButton.css";

/** The props used for the {@link BlockButton} component. */
interface BlockButtonProps {
  /** The type of button. This allows for the inclusion of the button in a form to not automatically submit the form. */
  type?: "submit" | "button";
  /** The color of the button. Defaults to primary. */
  color?:
    | "notify"
    | "info"
    | "warning"
    | "error"
    | "muted"
    | "primary"
    | "secondary";
}

/** A simple button component that is filled visually. */
const BlockButton: FunctionComponent<
  Modify<HTMLProps<HTMLButtonElement>, BlockButtonProps>
> = ({ type = "button", color = "primary", children, className, ...props }) => {
  return (
    <button
      className={classNames("BlockButton", color, className)}
      type={type}
      {...props}
    >
      {children}
    </button>
  );
};

export default BlockButton;
export type { BlockButtonProps };
