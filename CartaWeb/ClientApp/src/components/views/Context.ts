import React, {
  ReactElement,
  createContext,
  useContext,
  useCallback,
} from "react";
import View, { isContainerView } from "./View";

// TODO: Since we will already expose a method to add a view to history, we will allow the element to do
//       this on its own but every element can then use a utility component `View` that automatically implements this
//       functionality.

// TODO: Add documentation to this context.

/**
 * The type of value used for the {@link ViewsContext}.
 * This is meant to provide the minimal functionality to use views.
 * Additional features are defined in the {@link IViewsActions}.
 */
interface IViewsContext {
  /** The identifier of the root view. */
  rootId: number;
  /** The identifier of the current view. */
  viewId: number;

  addView: (view: View) => number;
  removeView: (id: number) => void;
  getView: (id: number) => View | null;
  setView: (id: number, view: View | ((view: View) => View)) => void;

  getHistory: () => number[];
  addHistory: (id: number) => void;
}

/**
 * Defines actions that can be performed on the {@link Views} component.
 * Extends the functionality of the {@link IViewsContext} interface.
 */
interface IViewsActions {
  /* Actions that retrieve or modify views specifically. */
  getView: (id: number) => View | null;
  setView: (id: number, view: View | ((view: View) => View)) => void;
  addView: (view: View) => number;
  removeView: (id: number) => void;
  getParentView: (id: number) => View | null;
  getChildViews: (id: number) => (View | null)[] | null;

  /**
   * Sets the specified options on the current view.
   * @param options The options to set.
   */
  setOptions: (options: {
    title?: React.ReactNode;
    closeable?: boolean;
    status?: "none" | "modified" | "unmodified" | "info" | "warning" | "error";
  }) => void;

  /* Actions that retrieve or modify the history of views. */
  getHistory: () => number[];
  addHistory: (id: number) => void;

  /* Actions that retrieve or modify view tags. */
  getActiveTag: (key: string) => any;
  getTag: (id: number, key: string) => any;
  setTag: (id: number, key: string, value: any) => void;
  unsetTag: (id: number, key: string) => void;

  /* Utility actions that mutate the hierarchy. */
  activateView: (id: number) => void;
  addElementToContainer: (
    containerId: number,
    element: ReactElement,
    active?: boolean,
    tags?: Record<string, any>
  ) => number | null;
}

/**
 * Exposes the state of, and the actions performable on a {@link Views}.
 */
interface IViews {
  /** The identifier of the root view. */
  rootId: number;
  /** The identifier of the current view. */
  viewId: number;

  /** Actions that can be performed on the views. */
  actions: IViewsActions;
}

const ViewsContext = createContext<IViewsContext | undefined>(undefined);

/**
 * Returns an object that allows for determining the state of views along with actions that allow changing the state of
 * the views.
 * @returns The state along with state-mutating actions.
 */
const useViews = (): IViews => {
  // Grab the context if it is defined.
  // If not defined, raise an error because the rest of this hook will not work.
  const context = useContext(ViewsContext);
  if (context === undefined) {
    throw new Error("Views context must be used within a views component.");
  }
  const { rootId, viewId } = context;
  const { addView, removeView, getView, setView, addHistory, getHistory } =
    context;

  // Define all of the additional actions that can be performed on the views.
  const getParentView = useCallback(
    (id: number): View | null => {
      const view = getView(id);
      if (view === null || view.parentId === null) return null;
      return getView(view.parentId);
    },
    [getView]
  );
  const getChildViews = useCallback(
    (id: number): (View | null)[] | null => {
      const view = getView(id);
      if (view === null || !isContainerView(view)) return null;
      return view.childIds.map((childId) => getView(childId));
    },
    [getView]
  );

  const setOptions = useCallback(
    ({
      title,
      closeable,
      status,
    }: {
      title?: React.ReactNode;
      closeable?: boolean;
      status?:
        | "none"
        | "modified"
        | "unmodified"
        | "info"
        | "warning"
        | "error";
    }): void => {
      let view = getView(viewId);
      if (view === null) return;
      if (title !== undefined && view.title !== title)
        setView(viewId, (view) => ({ ...view, title }));
      if (closeable !== undefined && view.closeable !== closeable)
        setView(viewId, (view) => ({ ...view, closeable }));
      if (status !== undefined && view.status !== status)
        setView(viewId, (view) => ({ ...view, status }));
    },
    [getView, setView, viewId]
  );

  const getTag = useCallback(
    (id: number, key: string): any => {
      const view = getView(id);
      if (view === null) return undefined;
      return view.tags[key];
    },
    [getView]
  );
  const setTag = useCallback(
    (id: number, key: string, value: any): void => {
      setView(id, (view) => {
        if (view.tags[key] === value) return view;
        const newView = {
          ...view,
          tags: { ...view.tags, [key]: value },
        };
        return newView;
      });
    },
    [setView]
  );
  const unsetTag = useCallback(
    (id: number, key: string): void => {
      const view = getView(id);
      if (view === null || view.tags[key] === undefined) return;
      setView(id, (view) => {
        const newView = {
          ...view,
          tags: { ...view.tags },
        };
        delete newView.tags[key];
        return newView;
      });
    },
    [getView, setView]
  );
  const getActiveTag = useCallback(
    (key: string): any => {
      const historyIds = getHistory();
      for (let i = historyIds.length - 1; i >= 0; i--) {
        const historyId = historyIds[i];
        const tag = getTag(historyId, key);
        if (tag !== undefined) return tag;
      }
      return undefined;
    },
    [getHistory, getTag]
  );

  // TODO: Ensure a proper hierarchy is maintained when adding elements.
  //       That is, Splits > Tabs > Elements.
  // TODO: Also ensure that the sizes of children of split views are properly updated.
  const activateView = useCallback(
    (id: number): void => {
      const parent = getParentView(id);
      if (parent !== null && parent.type === "tab") {
        setView(parent.currentId, (parent) => ({
          ...parent,
          activeId: id,
        }));
      }
      addHistory(id);
    },
    [addHistory, getParentView, setView]
  );
  const addElementToContainer = useCallback(
    (
      containerId: number,
      element: ReactElement,
      active?: boolean,
      tags?: Record<string, any>
    ): number | null => {
      // Ensure that the container exists.
      const container = getView(containerId);
      if (container === null || !isContainerView(container)) return null;

      // Create the new view.
      const newViewId = addView({
        title: "View",
        closeable: true,
        status: "none",
        type: "element",
        currentId: containerId, // Any value will do here.
        parentId: containerId,
        element,
        tags: tags || {},
      });

      // If it has been specified that the element should be active, activate it using its parent.
      if (active) {
        if (container.type === "tab") {
          setView(container.currentId, (container) => ({
            ...container,
            activeId: newViewId,
          }));
          addHistory(newViewId);
        }
      }

      return newViewId;
    },
    [getView, addView, setView, addHistory]
  );

  return {
    rootId,
    viewId,

    actions: {
      addView,
      removeView,
      getView,
      setView,
      getHistory,
      addHistory,
      getParentView,
      getChildViews,
      getActiveTag,
      setOptions,
      getTag,
      setTag,
      unsetTag,
      activateView,
      addElementToContainer,
    },
  };
};

export default ViewsContext;
export { useViews };
export type { IViewsContext, IViewsActions, IViews };
