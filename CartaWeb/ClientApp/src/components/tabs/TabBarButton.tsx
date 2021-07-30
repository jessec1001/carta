import { FunctionComponent } from "react";
import classNames from "classnames";
import { TabProps } from "./Tab";

import "./tab.css";

const TabBarButton: FunctionComponent<TabProps> = ({
  title = "(Untitled)",
  status = "none",
  closeable = false,
  onClose,
  onFocus,
}) => {
  return (
    <div className={classNames("tab-bar-button", `status-${status}`)}>
      <span className="normal-text">
        {title && <span onClick={onFocus}>{title}</span>}
        {closeable && <span onClick={onClose}>×</span>}
      </span>
    </div>
  );
};

export default TabBarButton;
