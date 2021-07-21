import { FunctionComponent } from "react";

import "./structure.css";

/** A component that represents a column within a row. */
const Column: FunctionComponent = ({ children }) => {
  return <div className="column">{children}</div>;
};

export default Column;
