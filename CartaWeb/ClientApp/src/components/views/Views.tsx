import { FunctionComponent, useCallback, useRef, useState } from "react";
import Container from "./Container";
import ViewsContext from "./Context";
import Renderer from "./Renderer";
import View, { isContainerView } from "./View";

/** The props used for the {@link Views} component. */
interface ViewsProps {}

/**
 * Defines the composition of the compound {@link Views} component.
 * @borrows Renderer as Renderer
 * @borrows Container as Container
 */
interface ViewsComposition {
  Renderer: typeof Renderer;
  Container: typeof Container;
}

const Views: FunctionComponent<ViewsProps> & ViewsComposition = ({
  children,
  ...props
}) => {
  // TODO: Make the root simply a container rather than a split view or tab view.
  // We use a counter to keep track of the last identifier that was assigned to an element.
  // We initialize the views with a single root view.
  // We keep track of the most recently (most recent at end) activated views so that we can extract tags from them.
  const idRef = useRef<number>(0);
  const [views, setViews] = useState<Map<number, View>>(
    new Map([
      [
        -1,
        {
          currentId: -1,
          parentId: null,
          title: "root",
          closeable: false,
          status: "none",
          tags: {},
          type: "split",
          direction: "horizontal",
          sizes: [1],
          childIds: [0],
        },
      ],
      [
        0,
        {
          currentId: 0,
          parentId: -1,
          title: "root",
          closeable: false,
          status: "none",
          tags: {},
          type: "tab",
          childIds: [],
          activeId: null,
        },
      ],
    ])
  );
  const [history, setHistory] = useState<number[]>([]);

  // Construct the necessary minimal actions for the context to function.
  const getView = useCallback(
    (id: number): View | null => {
      return views.get(id) || null;
    },
    [views]
  );
  const setView = useCallback(
    (id: number, view: View | ((view: View) => View)) => {
      setViews((views) => {
        // This allows us to pass in a function to update the view.
        if (typeof view === "function") {
          // Get the view if it exists.
          const existView = views.get(id);
          if (!existView) return views;
          const newView = view(existView);

          if (newView === existView) return views;
          else view = newView;
        }

        // Prepare the view to be set.
        view = { ...view };
        view.currentId = id;

        // Copy the view map and add the new view.
        const newViews = new Map(views);
        newViews.set(id, view);

        // We need to check if the view contains a parent ID and update the parent.
        if (view.parentId !== null) {
          // We remove the parent from the new view if not valid.
          // Otherwise, we also update the parent view.
          let parent = views.get(view.parentId);
          if (parent === undefined || !isContainerView(parent))
            view.parentId = null;
          else {
            parent = {
              ...parent,
              childIds: parent.childIds.includes(view.currentId)
                ? parent.childIds
                : [...parent.childIds, view.currentId],
            };
            newViews.set(view.parentId, parent);
          }
        }

        // We need to check if the view contains children IDs and update the children.
        if (isContainerView(view)) {
          for (const childId of view.childIds) {
            // We remove the child from the new view if not valid.
            // Otherwise, we also update the child view.
            let child = views.get(childId);
            if (
              child === undefined ||
              (child.parentId !== null && child.parentId !== view.currentId)
            ) {
              view.childIds = view.childIds.filter((id) => id !== childId);
            } else {
              child = {
                ...child,
                parentId: view.currentId,
              };
              newViews.set(childId, child);
            }
          }
        }

        return newViews;
      });
    },
    []
  );
  const addView = useCallback(
    (view: View) => {
      const id = ++idRef.current;

      setView(id, view);
      return id;
    },
    [setView]
  );
  const removeView = useCallback(
    (id: number) => {
      setViews((views) => {
        // Cancel the operation if the view does not exist.
        const view = views.get(id);
        if (!view) return views;

        // Copy the view map.
        const newViews = new Map(views);

        // We need to remove the view as a child from its parent.
        if (view.parentId !== null) {
          let parent = views.get(view.parentId);
          if (parent && isContainerView(parent)) {
            // If the parent is a tab view and this view is the active view,
            // we need to use the history to find the next active view and set it.
            if (parent.type === "tab" && parent.activeId === id) {
              let nextActiveId: number | null = null;
              for (let k = history.length - 1; k >= 0; k--) {
                const historyId = history[k];
                if (historyId === id) continue;
                if (parent.childIds.includes(historyId)) {
                  nextActiveId = historyId;
                  break;
                }
              }
              parent = {
                ...parent,
                activeId: nextActiveId,
              };
            }

            // Remove the view from the parent's child list.
            parent = {
              ...parent,
              childIds: parent.childIds.filter((childId) => childId !== id),
            };
            newViews.set(view.parentId, parent);
          }
        }

        // We need to remove all of the view's descendants.
        const descendantIds = [id];
        for (let k = 0; k < descendantIds.length; k++) {
          const descendantId = descendantIds[k];
          const descendant = views.get(descendantId);
          if (descendant && isContainerView(descendant)) {
            for (const childId of descendant.childIds)
              descendantIds.push(childId);
          }
        }
        for (const descendantId of descendantIds) newViews.delete(descendantId);

        // We need to remove the removed views from the history.
        setHistory((history) =>
          history.filter((id) => !descendantIds.includes(id))
        );

        return newViews;
      });
    },
    [history]
  );
  const getHistory = useCallback(() => history, [history]);
  const addHistory = useCallback((id: number) => {
    setHistory((history) => {
      const newHistory = history.filter((historyId) => id !== historyId);
      newHistory.push(id);
      return newHistory;
    });
  }, []);

  return (
    <ViewsContext.Provider
      value={{
        rootId: 0,
        viewId: 0,
        addView,
        removeView,
        getView,
        setView,
        getHistory,
        addHistory,
      }}
    >
      {children}
    </ViewsContext.Provider>
  );
};
Views.Renderer = Renderer;
Views.Container = Container;

export default Views;
export type { ViewsProps };
