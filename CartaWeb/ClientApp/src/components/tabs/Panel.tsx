import classNames from "classnames";
import { FunctionComponent } from "react";
import { useTabs } from "./Context";

import "./Panel.css";

/** The props used for the {@link Panel} component. */
interface PanelProps {
  /** The unique identifier of the tab that this component corresponds to. */
  id: string | number;
}

/** A component that contains the content for a tab. */
const Panel: FunctionComponent<PanelProps> = ({ id, children }) => {
  // Get the state of the tabs and check if this is the active tab.
  const { activeTab } = useTabs();
  const currentTab = activeTab === id;

  // We return a component that should be rendered conditionally based on the active tab.
  return (
    <div className={classNames("TabPanel", { hidden: !currentTab })}>
      {children}
    </div>
  );
};

export default Panel;
