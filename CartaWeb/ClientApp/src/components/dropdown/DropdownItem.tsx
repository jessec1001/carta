import { FunctionComponent, HTMLAttributes } from "react";

import "./dropdown.css";

/** A component that represents an item in a dropdown menu. */
const DropdownItem: FunctionComponent<HTMLAttributes<HTMLSpanElement>> = ({
  children,
  ...props
}) => {
  return (
    <span className="dropdown-item" {...props}>
      {children}
    </span>
  );
};

export default DropdownItem;
