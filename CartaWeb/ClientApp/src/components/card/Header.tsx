import { FunctionComponent } from "react";

import "./card.css";

/** A component that renders the header of a {@link Card} component. */
const Header: FunctionComponent = ({ children }) => {
  return <div className="card-header">{children}</div>;
};

export default Header;
