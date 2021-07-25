import { FunctionComponent, HTMLProps } from "react";

import "./buttons.css";

/** A button that represents adding or creating an object. */
const IconAddButton: FunctionComponent<HTMLProps<HTMLButtonElement>> = ({
  children,
  type,
  ...props
}) => {
  return (
    <button
      className="icon-button"
      type={(type ?? "button") as any}
      style={{ color: "#2bb038" }}
      {...props}
    >
      +
    </button>
  );
};

export default IconAddButton;
