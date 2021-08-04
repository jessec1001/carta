import { FunctionComponent } from "react";

import "./tab.css";

/** The props used for the {@link TabContent} component. */
interface TabContentProps {
  active?: boolean;
}

/** A component that contains the content for a tab. */
const TabContent: FunctionComponent<TabContentProps> = ({
  active,
  children,
}) => {
  return (
    <div
      className="tab-content"
      style={{
        display: active ? "inherit" : "none",
      }}
    >
      {children}
    </div>
  );
};

export default TabContent;
