import classNames from "classnames";
import { FunctionComponent, HTMLProps } from "react";

/** A button that represents removing an item. */
const RemoveButton: FunctionComponent<HTMLProps<HTMLSpanElement>> = ({
  children,
  className,
  style,
  ...props
}) => {
  return (
    <span
      className={classNames("icon-button", className)}
      style={{ color: "var(--color-deletion)", fontWeight: "bold", ...style }}
      {...props}
    >
      ×
    </span>
  );
};

// Export React component.
export default RemoveButton;
