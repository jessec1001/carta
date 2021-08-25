import { FunctionComponent } from "react";

import "./card.css";

/** A component that renders the body of a {@link Card} component. */
const Body: FunctionComponent = ({ children }) => {
  return <div className="card-body">{children}</div>;
};

export default Body;
