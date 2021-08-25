import { FunctionComponent } from "react";

import "./card.css";

/** A component that renders the footer of a {@link Card} component. */
const Footer: FunctionComponent = ({ children }) => {
  return <div className="card-footer">{children}</div>;
};

export default Footer;
