import { FunctionComponent } from "react";

import "./tab.css";

/** A component that contains the content for a tab. */
const TabContent: FunctionComponent = ({ children }) => {
  return <div className="tab-content">{children}</div>;
};

export default TabContent;
