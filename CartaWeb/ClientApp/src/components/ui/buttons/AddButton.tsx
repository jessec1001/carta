import { FunctionComponent, HTMLProps } from "react";
import classNames from "classnames";

/** A button that represents adding an item. */
const AddButton: FunctionComponent<HTMLProps<HTMLSpanElement>> = ({
  children,
  className,
  style,
  ...props
}) => {
  return (
    <span
      className={classNames("icon-button", className)}
      style={{ color: "var(--color-addition)", fontWeight: "bold", ...style }}
      {...props}
    >
      +
    </span>
  );
};

// Export React component.
export default AddButton;
