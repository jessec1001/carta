import { ReactElement, createContext, useContext, useCallback } from "react";
import View, { isContainerView } from "./View";

// TODO: We could consider having multiple active views that have a specific type associated to them.
//       That is, we could use the view tags of focused views to update some global tags mapping.
//       The rationale here is that we are usually looking for a view of a specific type that was most recently selected.

// TODO: Since we will already expose a method to set a view to be the active view, we will allow the element to do
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
  setView: (id: number, view: View) => void;

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
  setView: (id: number, view: View) => void;
  addView: (view: View) => number;
  removeView: (id: number) => void;
  getParentView: (id: number) => View | null;
  getChildViews: (id: number) => (View | null)[] | null;

  /* Actions that retrieve or modify the history of views. */
  getHistory: () => number[];
  addHistory: (id: number) => void;

  /* Actions that retrieve or modify view tags. */
  getActiveTag: (key: string) => any;
  getTag: (id: number, key: string) => any;
  setTag: (id: number, key: string, value: any) => void;
  unsetTag: (id: number, key: string) => void;

  /* Utility actions that mutate the hierarchy. */
  addElementToContainer: (
    containerId: number,
    element: ReactElement,
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
      const view = getView(id);
      console.log("SET TAG", view, key, value);
      if (view === null || view.tags[key] === value) return;

      const viewCopy = { ...view, tags: { ...view.tags, [key]: value } };
      setView(id, viewCopy);
    },
    [getView, setView]
  );
  const unsetTag = useCallback(
    (id: number, key: string): void => {
      const view = getView(id);
      console.log("UNSET TAG", view, key);
      if (view === null || view.tags[key] === undefined) return;

      const viewCopy = { ...view, tags: { ...view.tags } };
      delete viewCopy.tags[key];
      setView(id, viewCopy);
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
  // TODO: Add an activate action that will set an active view. This action should synergize with tabs to set the active tab.

  const addElementToContainer = useCallback(
    (
      containerId: number,
      element: ReactElement,
      tags?: Record<string, any>
    ): number | null => {
      const container = getView(containerId);
      if (container === null || !isContainerView(container)) return null;

      return addView({
        type: "element",
        currentId: containerId, // Any value will do here.
        parentId: containerId,
        element,
        tags: tags || {},
      });
    },
    [addView, getView]
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
      getTag,
      setTag,
      unsetTag,
      addElementToContainer,
    },
  };
};

export default ViewsContext;
export { useViews };
export type { IViewsContext, IViewsActions, IViews };
