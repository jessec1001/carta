import React, { FunctionComponent } from "react";

/**
 * The props used for the {@link Tab} component.
 * These props will be used by the {@link TabContainer} component.
 */
interface TabProps {
  /** The title element to render in the tab navigation bar. */
  title?: React.ReactNode;
  /** The status indicator used to decorate the tab navigation bar. */
  status?: "none" | "modified" | "unmodified" | "error";

  /** Whether the tab is closeable by the user. */
  closeable?: boolean;

  /** The event handler for when the tab has requested to close. */
  onClose?: () => void;
  /** The event handler for when the tab has been focussed. */
  onFocus?: () => void;
}

const Tab: FunctionComponent<TabProps> = ({ children }) => {
  // We just re-render the children here.
  // The remaining properties are handled by the tab container.
  return <React.Fragment>{children}</React.Fragment>;
};

export default Tab;
export type { TabProps };
