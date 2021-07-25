import { FunctionComponent, HTMLProps } from "react";

import "./buttons.css";

/** A button that represents removing or deleting an object. */
const IconRemoveButton: FunctionComponent<HTMLProps<HTMLButtonElement>> = ({
  children,
  type,
  ...props
}) => {
  return (
    <button
      className="icon-button"
      type={(type ?? "button") as any}
      style={{ color: "#d83c3c" }}
      {...props}
    >
      ×
    </button>
  );
};

export default IconRemoveButton;
