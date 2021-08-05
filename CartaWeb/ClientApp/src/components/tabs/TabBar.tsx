import { FunctionComponent } from "react";

import "./tab.css";

/** The bar containing interactive tab bar buttons. */
const TabBar: FunctionComponent = ({ children }) => {
  return <div className="tab-bar">{children}</div>;
};

export default TabBar;
