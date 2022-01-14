import { createContext, useCallback, useContext } from "react";

/**
 * The type of value used for the {@link TabsContext}.
 * This is meant to provide the minimal functionality to use tabs.
 * Additional features are defined in {@link ITabsActions}.
 */
interface ITabsContext {
  /** Whether the tabs in the tab bar are draggable. */
  draggableTabs: boolean;
  /** The source of a tab drag. */
  dragSource: string | number | null;
  /** The target of a tab drag. */
  dragTarget: string | number | null;
  /** Sets the source of a tab drag. */
  setDragSource: (tab: string | number | null) => void;
  /** Sets the source of a tab drag. */
  setDragTarget: (tab: string | number | null) => void;
  /** Finishes a tab drag. */
  finishDrag: () => void;

  /** The currently active tab. A value of `null` represents no tab. */
  activeTab: string | number | null;
  /** Sets the currently active tab. */
  setActiveTab: (tab: string | number | null) => void;
}
/**
 * Defines actions that can be performed on the {@link Tabs} component.
 * Extends the functionality of the {@link ITabsContext} interface.
 */
interface ITabsActions {
  /** Sets the active tab. */
  set: (tab: string | number | null) => void;
  /** Clears the active tab (sets it to `null`). */
  clear: () => void;
}
/**
 * Exposes the state of, and the actions performable on a {@link Tabs}.
 */
interface ITabs {
  /** Whether the tabs in the tab bar are draggable. */
  draggableTabs: boolean;
  /** The source of a tab drag. */
  dragSource: string | number | null;
  /** The target of a tab drag. */
  dragTarget: string | number | null;
  /** Sets the source of a tab drag. */
  setDragSource: (tab: string | number | null) => void;
  /** Sets the source of a tab drag. */
  setDragTarget: (tab: string | number | null) => void;
  /** Finishes a tab drag. */
  finishDrag: () => void;

  /** The currently active tab. A value of `null` represents no tab. */
  activeTab: string | number | null;
  /** Actions that can be performed on the tabs. */
  actions: ITabsActions;
}

/** The context used to expose information about the {@link Tabs} component. */
const TabsContext = createContext<ITabsContext | undefined>(undefined);

/**
 * Returns an object that allows for determining the state of tabs along with actions that allow changing the state of
 * the tabs.
 * @returns The state along with state-mutating actions.
 */
const useTabs = (): ITabs => {
  // Grab the context if it is defined.
  // If not defined, raise an error because the rest of this hook will not work.
  const context = useContext(TabsContext);
  if (!context) {
    throw new Error("Tabs context must be used within a tabs component.");
  }

  // We grab the original state from the context.
  const {
    draggableTabs,
    dragSource,
    dragTarget,
    setDragSource,
    setDragTarget,
    finishDrag,
    activeTab,
    setActiveTab,
  } = context;

  // From the original context functionality, we provide extensions.
  const set = setActiveTab;
  const clear = useCallback(() => setActiveTab(null), [setActiveTab]);

  // We return this modified context.
  return {
    draggableTabs,
    dragSource,
    dragTarget,
    setDragSource,
    setDragTarget,
    finishDrag,
    activeTab,
    actions: {
      set,
      clear,
    },
  };
};

export default TabsContext;
export { useTabs };
export type { ITabsContext, ITabsActions, ITabs };
