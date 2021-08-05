import { FunctionComponent } from "react";

import "./text.css";

/** A component that represents a paragraph. */
const SeparatedText: FunctionComponent = ({ children }) => {
  return <div className="separated">{children}</div>;
};

export default SeparatedText;
