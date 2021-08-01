import { FunctionComponent } from "react";

import "./scroll.css";

/** A component that allows the children of the component to be scrolled vertically. */
const VerticalScroll: FunctionComponent = ({ children }) => {
  return (
    <div className="scroll-container">
      <div className="scroll vertical">{children}</div>
    </div>
  );
};

export default VerticalScroll;
