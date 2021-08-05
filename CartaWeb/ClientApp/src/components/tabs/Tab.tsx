import React, {
  createContext,
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from "react";
import TabContent from "./TabContent";

import "./tab.css";

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

/** Represents the type of identifier for a tab. */
type TabId = string | number;
/** The type of value of the {@link TabContext} context. */
interface TabContextValue {
  set: (props: TabProps, id?: TabId) => TabId;
  unset: (id: TabId) => void;

  active: TabId | null;
}

/** A context that allows tabs to be registered with tab containers and displayed appropriately. */
const TabContext = createContext<TabContextValue>({
  set: () => 0,
  unset: () => {},

  active: null,
});

/** A component that renders a tab within a tab container with specified properties on the tab button. */
const Tab: FunctionComponent<TabProps> = ({ children, ...props }) => {
  const { active, set, unset } = useContext(TabContext);
  const [id, setId] = useState<TabId | undefined>(undefined);

  // We extract all of the props individually here so we may compare individually.
  const { title, status, closeable, onClose, onFocus } = props;

  // This effect updates the tab whenever the component is mounted or changed.
  useEffect(() => {
    setId((id) =>
      set(
        {
          title,
          status,
          closeable,
          onClose,
          onFocus,
        },
        id
      )
    );
  }, [set, title, status, closeable, onClose, onFocus]);

  // This effect tears down the tab when the component is unmounted.
  useEffect(() => {
    return () => {
      if (id !== undefined) unset(id);
    };
  }, [unset, id]);

  // We just re-render the children here if the tab is active.
  // The remaining properties are handled by the tab container.
  return <TabContent active={id === active}>{children}</TabContent>;
};

export default Tab;
export type { TabProps, TabId };
export { TabContext };
export type { TabContextValue };
