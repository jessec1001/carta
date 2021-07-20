import { FunctionComponent } from "react";

import "./layout.css";

/** A component that wraps the content of a page. */
const Wrapper: FunctionComponent = ({ children }) => {
  return <div className="wrapper">{children}</div>;
};

export default Wrapper;
