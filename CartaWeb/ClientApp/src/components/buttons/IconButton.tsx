import { FunctionComponent, HTMLProps } from "react";
import classNames from "classnames";

import "./IconButton.css";

/** The props used for the {@link IconButton} component. */
interface IconButtonProps {
  /** How the icon button should be shaped. By default, a circle. */
  shape?: "circle" | "square";
}

/** A component that represents a small round button with an icon in its center. */
const IconButton: FunctionComponent<
  HTMLProps<HTMLButtonElement> & IconButtonProps
> = ({ shape = "circle", children, type, className, ...props }) => {
  return (
    <button
      type={(type ?? "button") as any}
      className={classNames("IconButton", shape, className)}
      {...props}
    >
      {children}
    </button>
  );
};

export default IconButton;
