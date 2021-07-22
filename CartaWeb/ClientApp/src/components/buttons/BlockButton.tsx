import { FunctionComponent, HTMLProps } from "react";
import { Modify } from "types";
import classNames from "classnames";

import "./buttons.css";

/** The props used for the {@link BlockButton} component. */
interface BlockButtonProps {
  type?: "submit" | "button";
  color?: "primary" | "secondary";
}

/** A simple button component that is filled visually. */
const BlockButton: FunctionComponent<
  Modify<HTMLProps<HTMLButtonElement>, BlockButtonProps>
> = ({ color, children, className, ...props }) => {
  return (
    <button
      className={classNames(className, "block-button", color ?? "primary")}
      {...props}
    >
      {children}
    </button>
  );
};

// Export component and underlying types.
export default BlockButton;
export type { BlockButtonProps };
