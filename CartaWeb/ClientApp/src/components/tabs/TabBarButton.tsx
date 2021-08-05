import { FunctionComponent } from "react";
import classNames from "classnames";
import { TabProps } from "./Tab";

import "./tab.css";

/** A component that is a button located on the tab bar that allows for tab navigation. */
const TabBarButton: FunctionComponent<TabProps & { active?: boolean }> = ({
  active,
  title = "(Untitled)",
  status = "none",
  closeable = false,
  onClose,
  onFocus,
}) => {
  return (
    <div
      className={classNames("tab-bar-button", `status-${status}`, { active })}
    >
      <span className="normal-text">
        {title && (
          <span onClick={onFocus} className="normal-text">
            {title}
          </span>
        )}
        {closeable && (
          <span onClick={onClose} className="tab-close normal-text">
            ×
          </span>
        )}
      </span>
    </div>
  );
};

export default TabBarButton;
