import { FunctionComponent } from "react";

import "./text.css";

/** A component that represents a paragraph. */
const Paragraph: FunctionComponent = ({ children }) => {
  return <p className="paragraph">{children}</p>;
};

export default Paragraph;
