import { FunctionComponent } from "react";

import "./structure.css";

/** A component that represents a contiguous row on the screen. */
const Row: FunctionComponent = ({ children }) => {
  return <div className="row">{children}</div>;
};

export default Row;
