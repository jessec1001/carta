import { FunctionComponent } from "react";

import "./text.css";

/** A component that contains a title for a page. */
const Title: FunctionComponent = ({ children, ...props }) => {
  return (
    <h1 className="title" {...props}>
      {children}
    </h1>
  );
};

export default Title;
