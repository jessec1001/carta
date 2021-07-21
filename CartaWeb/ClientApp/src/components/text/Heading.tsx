import { FunctionComponent } from "react";

/** A component that represents a heading for a block of text. */
const Heading: FunctionComponent = ({ children }) => {
  return <h2 className="heading">{children}</h2>;
};

export default Heading;
