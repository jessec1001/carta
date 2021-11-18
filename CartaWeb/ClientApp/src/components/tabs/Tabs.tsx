import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import TabsContext from "./Context";
import Panel from "./Panel";
import Tab from "./Tab";
import Area from "./Area";
import Bar from "./Bar";

/** The props used for the {@link Tabs} component. */
interface TabsProps {
  /** The currently active tab of the tabs. If not specified, the component is uncontrolled. */
  activeTab?: string | number | null;
  /** The initially active tab of the tabs. Used for uncontrolled components for setting the initial state. */
  initialTab?: string | number | null;
  /** The event handler that is called whenever the active tab changes. */
  onChangeTab?: (tab: string | number | null) => void;
}

/**
 * Defines the composition of the compound {@link Tabs} component.
 * @borrows Area as Area
 * @borrows Bar as Bar
 * @borrows Tab as Tab
 * @borrows Panel as Panel
 */
interface TabsComposition {
  Area: typeof Area;
  Bar: typeof Bar;
  Tab: typeof Tab;
  Panel: typeof Panel;
}

/**
 * A component that renders a tabbed environment by the use of tabs and panels. The tabs can be placed inside of a
 * standard tab bar for easy navigation. Each tab and each panel has an associated identifier. This identifier is used
 * to determine the currently active tab. There may only be a single or no active tab and its corresponding panel is the
 * only panel that will be rendered.
 *
 * This component has an optionally controlled active tab state that can be specified.
 * @example
 * ```jsx
 * <Tabs>
 *   <Tabs.Area direction="horizontal">
 *     <Tabs.Bar>
 *       <Tabs.Tab id={1} title={"First Tab"} />
 *       <Tabs.Tab id={2} title={"Second Tab"} color="error" />
 *     </TabBar>
 *     <Tabs.Panel id={1}>
 *       This content displays when the first tab is active.
 *     </Tabs>
 *     <Tabs.Panel id={2}>
 *       This content displays when the second tab is active.
 *     </Tabs>
 *   </Tabs.Area>
 * </Tabs>
 * ```
 */
const Tabs: FunctionComponent<TabsProps> & TabsComposition = ({
  activeTab,
  initialTab = null,
  onChangeTab,
  children,
  ...props
}) => {
  // We use an optionally controlled active tab.
  const [actualActiveTab, setActiveTab] = useControllableState(
    initialTab,
    activeTab,
    onChangeTab
  );

  // We wrap the child elements in a tabs context to provide the tabs functionality.
  return (
    <TabsContext.Provider
      value={{ activeTab: actualActiveTab, setActiveTab: setActiveTab }}
    >
      {children}
    </TabsContext.Provider>
  );
};
Tabs.Area = Area;
Tabs.Bar = Bar;
Tabs.Tab = Tab;
Tabs.Panel = Panel;

export default Tabs;
export type { TabsProps };
