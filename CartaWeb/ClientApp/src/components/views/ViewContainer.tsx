import React, { FunctionComponent, useMemo, useRef, useState } from "react";
import View, {
  ElementView,
  isContainerView,
  TabView,
  ViewActions,
} from "./View";
import ViewContext from "./ViewContext";

/**
 * A component that represents a container for views that automatically constructs a tree hierarchy with
 * split containers and tab containers to represent a useful workspace-like layout.
 */
const ViewContainer: FunctionComponent = ({ children }) => {
  // We use a counter to keep track of the last identifier that was assigned to an element.
  // We initialize the views to contain a single split container which is the root which all other elements will live
  // inside of.
  const idRef = useRef<number>(0);
  const [views, setViews] = useState<Map<number, View>>(
    new Map([
      [
        0,
        {
          currentId: 0,
          parentId: null,

          tags: {},

          type: "split",
          childIds: [],
          direction: "horizontal",
          sizes: [],
        },
      ],
    ])
  );
  const [activeView, setActiveView] = useState<number | null>(null);

  // We construct the actions object to provide actions to child components.
  // These actions operate at a global scale with references to view identifiers specifying structure.
  const actions: ViewActions = useMemo(() => {
    return {
      // To get a view, we simply check if it exists and return a copy if it does.
      getView(id: number): View | null {
        const view = views.get(id);
        if (!view) return null;
        else return { ...view };
      },
      // To get a parent view, we check that the view exists and then assume that the parent exists if it does not have
      // a null identifier.
      getParentView(id: number): View | null {
        const view = views.get(id);
        if (!view) return null;
        else {
          if (view.parentId === null) return null;
          const parentView = views.get(view.parentId)!;
          return { ...parentView };
        }
      },
      // To get children view, we check that the view exists and then assume that if the type is a container type, that
      // all of its children exist.
      getChildViews(id: number): View[] | null {
        const view = views.get(id);
        if (!view) return null;
        else {
          if (isContainerView(view))
            return view.childIds.map((childId) => this.getView(childId)!);
          else return null;
        }
      },

      // We basically just provide this method as a setter for the active view.
      // We perform some slight checking as to whether the view exists.
      setActiveView(id: number | null): void {
        if (id === null) setActiveView(id);
        else if (views.has(id)) {
          setActiveView(id);
        }
      },

      // The following methods provide a way to manage the tags on a particular view.
      // Typically this tag will be set or unset on the current view in order to expose data.
      setTag(id: number, key: string, value: any): void {
        setViews((views) => {
          const view = views.get(id);
          if (view) {
            // We shortcut here to avoid setting the same value repeatedly.
            if (view.tags[key] === value) return views;

            const newViews = new Map(views);
            const newTags = { ...view.tags };
            newTags[key] = value;
            newViews.set(id, {
              ...view,
              tags: newTags,
            });
            return newViews;
          } else return views;
        });
      },
      unsetTag(id: number, key: string): void {
        setViews((views) => {
          const view = views.get(id);
          if (view) {
            // We shortcut here to avoid unsetting the same value repeatedly.
            if (view.tags[key] === undefined) return views;

            const newViews = new Map(views);
            const newTags = { ...view.tags };
            delete newTags[key];
            newViews.set(id, {
              ...view,
              tags: newTags,
            });
            return newViews;
          } else return views;
        });
      },

      // TODO: Automatically add elements in the hierarchy to form a valid split -> tab -> element tree.
      // To add a child element to a view, the view has to be a container such as split.
      // We add an element by generating a new element view with a unique identifier and the specified render element.
      addElementToContainer(
        parentId: number,
        element: React.ReactElement,
        tags: Record<string, string> = {}
      ): number | null {
        let parentView = views.get(parentId);
        if (!parentView || !isContainerView(parentView)) return null;

        // Check if we are inside of a split view.
        // If we are inside of a split view, we need to create a tab view container inside of it.
        let tabView: TabView | null = null;
        if (parentView.type === "split") {
          tabView = {
            type: "tab",

            currentId: ++idRef.current,
            parentId: parentId,

            tags: {},

            childIds: [],
          };
          parentView = tabView;
        }

        // Create the new view.
        const view: ElementView = {
          type: "element",
          currentId: ++idRef.current,
          parentId: parentView.currentId,

          tags: tags,

          element: element,
        };

        // Update the view hierarchy.
        setViews((views) => {
          let parentView = views.get(parentId);
          if (!parentView || !isContainerView(parentView)) return views;

          const newViews = new Map(views);

          // Add a tab view if necessary.
          if (tabView !== null) {
            newViews.set(parentId, {
              ...parentView,
              childIds: [...parentView.childIds, tabView.currentId],
            });
            newViews.set(tabView.currentId, tabView);
            parentView = tabView;
          }

          // Update the parent view.
          newViews.set(parentView.currentId, {
            ...parentView,
            childIds: [...parentView.childIds, view.currentId],
          });

          // Update the element view.
          newViews.set(view.currentId, view);
          return newViews;
        });

        // Return the identifier for the new view.
        return view.currentId;
      },
      // To remove a child, we remove both that child and all of its children.
      // Additionally, we make sure to remove the reference to the child from its parent.
      removeElement(elementId: number): void {
        // Update the view hierarchy semi-recursively by removing all nested children elements.
        setViews((views) => {
          // Check that the specified child view exists and is an element view.
          const childView = views.get(elementId);
          if (!childView || childView.type !== "element") return views;

          // Copy the existing views into a new views mapping since we will be modifying it.
          const newViews = new Map(views);

          // Recursively remove this element from the views mapping and from the parent child list.
          // If the parent child list is empty, propagate the effect upwards.
          let currentView: View = childView;
          while (currentView.currentId !== 0) {
            // Delete the current view.
            newViews.delete(currentView.currentId);

            if (currentView.parentId !== null) {
              // If the parent exists, remove the view from its children/
              const parentView = views.get(currentView.parentId);
              if (parentView && isContainerView(parentView)) {
                const newChildIds = [...parentView.childIds];
                const childIndex = newChildIds.indexOf(currentView.currentId);
                if (childIndex >= 0) newChildIds.splice(childIndex, 1);
                newViews.set(parentView.currentId, {
                  ...parentView,
                  childIds: newChildIds,
                });

                // If the parent has no children, repeat.
                if (newChildIds.length > 0) break;
                else currentView = parentView;
              } else break;
            } else break;
          }

          return newViews;
        });
      },
    };
  }, [views]);

  // We wrap whatever children are passed in with a view context that can then be accessed efficiently.
  return (
    <ViewContext.Provider
      value={{
        // The view identifier is equal to null here to represent the container itself.
        rootId: 0,
        viewId: 0,
        activeId: activeView,
        actions: actions,
      }}
    >
      {children}
    </ViewContext.Provider>
  );
};

// TODO: Remove
/** How is the active view stored?
 *
 * 1. Active view stored as a single view.
 * 2. Active view is stored as the hierarchy of all elements in the tree of the active view.
 * 3. ?
 *
 * To match the standard of how events propagate, we would store the active view in the entire hierarchy.
 * However, this makes it less useful to get the active element explicitly.
 *
 * Additionally, there is the question of how an element gets focus associated to it.
 * Should this be:
 * 1. When the element is clicked or interacted with in any way.
 * 2. When the element itself fires some event that sets the active element.
 *
 * Since we will already expose a method to set a view to be the active view, we will allow the element to do
 * this on its own but every element can then use a utility component `View` that automatically implements this
 * functionality.
 */

export default ViewContainer;
