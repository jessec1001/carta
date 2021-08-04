import {
  FunctionComponent,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { TabContext, TabId, TabProps } from "./Tab";
import TabBar from "./TabBar";
import TabBarButton from "./TabBarButton";

import "./tab.css";

/** A component that renders tabs in a tab bar and into a container.  */
const TabContainer: FunctionComponent = ({ children }) => {
  // This identifier refers to the next identifier to be automatically assigned to a tab.
  const id = useRef<number | null>(0);
  // This identifier is used to refer to the active tab.
  const [active, setActive] = useState<TabId | null>(null);
  // This stores all of the tabs that the container is aware of.
  const [tabs, setTabs] = useState<Map<TabId, TabProps>>(new Map());

  // When the tab children are changed, we need to verify that key is still valid.
  useEffect(() => {
    setActive((active) => {
      if (active === null) return null;
      const included = tabs.has(active);
      return included ? active : null;
    });
  }, [tabs]);

  // We create the methods used to update the tab container.
  const setTab = useCallback((tab: TabProps, tabId?: TabId): TabId => {
    // Set the identifier if not already set.
    const newTabId = tabId ?? id.current!++;

    // Set the tab on the tabs mapping.
    setTabs((tabs) => {
      const newTabs = new Map(tabs);
      newTabs.set(newTabId, tab);
      return newTabs;
    });

    return newTabId;
  }, []);
  const unsetTab = useCallback((tabId: TabId): void => {
    // Unset the tab on the tabs mapping.
    setTabs((tabs) => {
      const newTabs = new Map(tabs);
      newTabs.delete(tabId);
      return newTabs;
    });
  }, []);

  // We memoize the context value.
  const context = useMemo(
    () => ({
      set: setTab,
      unset: unsetTab,
      active,
    }),
    [active, setTab, unsetTab]
  );

  return (
    <div className="tab-container">
      {/* Render the tab bar with buttons for each tab child. */}
      {/* We modify some of the event handlers to also satisfy our needs. */}
      <TabBar>
        {Array.from(tabs.entries()).map(([id, tab]) => {
          const { onClose, onFocus, ...props } = tab;
          return (
            <TabBarButton
              key={id}
              {...props}
              active={id === active}
              onClose={() => {
                if (onClose) onClose();
                if (id === active) setActive(null);
              }}
              onFocus={() => {
                if (onFocus) onFocus();
                setActive(id);
              }}
            />
          );
        })}
      </TabBar>

      {/* Only render the tab content for the tab that is active. */}
      {/* Note that this check is performed in the tabs themselves in order to allow for more complex structures. */}
      <TabContext.Provider value={context}>{children}</TabContext.Provider>
    </div>
  );
};

export default TabContainer;
