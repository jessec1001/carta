import React, { FunctionComponent, useEffect, useState } from "react";
import Tab, { TabProps } from "./Tab";
import TabBar from "./TabBar";
import TabBarButton from "./TabBarButton";
import TabContent from "./TabContent";

import "./tab.css";

/** A component that  */
const TabContainer: FunctionComponent = ({ children }) => {
  // The key is used to refer to the active tab.
  const [key, setKey] = useState<React.Key | null>(null);

  // We are only going to render the tab-component children elements.
  const tabChildren = React.Children.toArray(children).filter(
    (child) => React.isValidElement<TabProps>(child) && child.type === Tab
  ) as React.ReactElement<TabProps>[];

  // When the tab children are changed, we need to verify that key is still valid.
  useEffect(() => {
    setKey((key) => {
      const included = tabChildren.some((tab) => tab.key === key);
      return included ? key : null;
    });
  }, [tabChildren]);

  return (
    <div className="tab-container">
      {/* Render the tab bar with buttons for each tab child. */}
      <TabBar>
        {tabChildren.map((tab) => {
          const { onClose, onFocus, ...props } = tab.props;
          return (
            <TabBarButton
              {...props}
              onClose={() => {
                if (onClose) onClose();
                if (tab.key === key) setKey(null);
              }}
              onFocus={() => {
                if (onFocus) onFocus();
                setKey(tab.key);
              }}
            />
          );
        })}
      </TabBar>

      {/* Only render the tab content for the tab that is active. */}
      {/* TODO: Make non-selected tabs display:none rather than not render at all. */}
      <TabContent>
        {tabChildren.map((tab) => (tab.key === key ? tab : null))}
      </TabContent>
    </div>
  );
};

export default TabContainer;

/** How will this component be used?
 *  <TabContainer>
 *    <Tab title={
 *        <span>
 *          <DatabaseIcon />
 *          Datasets
 *        </span>
 *      }
 *      status="modified"
 *      closeable
 *    >
 *      Some stuff
 *      <VisNetworkRenderer />
 *    </Tab>
 *    <Tab title={
 *        <span>
 *          <GraphIcon />
 *          Graph #1
 *        </span>
 *      }
 *      status="none"
 *      closeable
 *    >
 *      Some graph stuff
 *    </Tab>
 *  </TabContainer>
 */
