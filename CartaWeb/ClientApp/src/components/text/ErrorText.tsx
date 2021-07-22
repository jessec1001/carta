import { FunctionComponent } from "react";

/** A component that represents a string of error text. */
const ErrorText: FunctionComponent = ({ children }) => {
  return <span className="error">{children}</span>;
};

export default ErrorText;
