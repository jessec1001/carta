import { FunctionComponent } from "react";

import "./scroll.css";

/** A component that allows the children of the component to be scrolled horizontally. */
const HorizontalScroll: FunctionComponent = ({ children }) => {
  return (
    <div className="scroll-container">
      <div className="scroll horizontal">{children}</div>
    </div>
  );
};

export default HorizontalScroll;
