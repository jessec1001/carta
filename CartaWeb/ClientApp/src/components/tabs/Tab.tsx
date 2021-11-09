import { FunctionComponent, HTMLAttributes } from "react";
import classNames from "classnames";
import { Modify } from "types";
import { useTabs } from "./Context";

import "./Tab.css";

/**
 * The props used for the {@link Tab} component.
 */
interface TabProps {
  /** The unique identifier of the tab that this component corresponds to. */
  id: string | number;

  /** The status indicator used to decorate the tab navigation bar. */
  status?: "none" | "modified" | "unmodified" | "info" | "warning" | "error";

  /** Whether the tab is closeable by the user. */
  closeable?: boolean;

  /** The event handler for when the tab has requested to close. */
  onClose?: () => void;
  /** The event handler for when the tab has been focused. */
  onFocus?: () => void;
}

/** A component that renders a tab within a tab container with specified properties on the tab button. */
const Tab: FunctionComponent<Modify<HTMLAttributes<HTMLDivElement>, TabProps>> =
  ({ id, status, closeable, onClose, onFocus, children, ...props }) => {
    // We use the state of the tabs context to perform some rendering and functions of this component.
    const { activeTab, actions: tabActions } = useTabs();

    // These event handlers update the tab state while emitting relevant events.
    const handleFocus = () => {
      tabActions.set(id);
      onFocus && onFocus();
    };
    const handleClose = () => {
      if (activeTab === id) tabActions.clear();
      onClose && onClose();
    };

    return (
      <div
        className={classNames("Tab", `status-${status}`, {
          active: activeTab === id,
          closeable: closeable,
        })}
        onClick={handleFocus}
        {...props}
      >
        <div className="Tab-Content">{children}</div>
        {closeable && (
          <span className="Tab-Close" onClick={handleClose}>
            ×
          </span>
        )}
      </div>
    );
  };

export default Tab;
export type { TabProps };
