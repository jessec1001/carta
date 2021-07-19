import React, { FunctionComponent } from "react";

import "./dropdown.css";

/** A component that displays the collapsible area of a dropdown menu. */
const DropdownArea: FunctionComponent = ({ children }) => {
  return (
    <ul role="navigation">
      {React.Children.map(children, (child) => (
        <li>{child}</li>
      ))}
    </ul>
  );
};

export default DropdownArea;
