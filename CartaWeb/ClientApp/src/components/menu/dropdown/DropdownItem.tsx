import { FunctionComponent } from "react";

import "./dropdown.css";

/** A component that represents an item in a dropdown menu. */
const DropdownItem: FunctionComponent = ({ children }) => {
  return <span className="dropdown-item">{children}</span>;
};

export default DropdownItem;
