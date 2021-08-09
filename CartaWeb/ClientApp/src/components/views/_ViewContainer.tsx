import React, { FunctionComponent, useMemo, useRef, useState } from "react";
import View, { ElementView, ViewActions } from "./View";
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

          type: "split",
          childIds: [],
          direction: "horizontal",
          sizes: [],
        },
      ],
    ])
  );

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
          if (view.type === "split")
            return view.childIds.map((childId) => this.getView(childId)!);
          else return null;
        }
      },

      // TODO: Automatically add elements in the hierarchy to form a valid split -> tab -> element tree.
      // To add a child element to a view, the view has to be a container such as split.
      // We add an element by generating a new element view with a unique identifier and the specified render element.
      addChildElement(
        parentId: number,
        element: React.ReactElement
      ): number | null {
        const parentView = views.get(parentId);
        if (!parentView || parentView.type !== "split") return null;

        // Create the new view.
        const currentId = ++idRef.current;
        const view: ElementView = {
          type: "element",
          currentId,
          parentId,
          element,
        };

        // Update the view hierarchy.
        setViews((views) => {
          const newViews = new Map(views);

          // Update the parent view and add the child view.
          newViews.set(parentId, {
            ...parentView,
            childIds: [...parentView.childIds, currentId],
          });
          newViews.set(currentId, view);

          return newViews;
        });

        // Return the identifier for the new view.
        return view.currentId;
      },
      // To remove a child, we remove both that child and all of its children.
      // Additionally, we make sure to remove the reference to the child from its parent.
      removeChildElement(childId: number): void {
        // Update the view hierarchy semi-recursively by removing all nested children elements.
        setViews((views) => {
          // Check that the specified child view exists and is an element view.
          const childView = views.get(childId);
          if (!childView || childView.type !== "element") return views;

          // Copy the existing views into a new views mapping since we will be modifying it.
          const newViews = new Map(views);

          // Check if there is a parent that this element can be removed from.
          if (childView.parentId !== null) {
            const parentView = views.get(childView.parentId);
            if (parentView && parentView.type === "split") {
              newViews.set(parentView.currentId, {
                ...parentView,
                childIds: parentView.childIds.filter(
                  (comparisonChildId) => comparisonChildId !== childId
                ),
              });
            }
          }

          // Remove the child element view.
          newViews.delete(childId);

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
        viewId: 0,
        actions: actions,
      }}
    >
      {children}
    </ViewContext.Provider>
  );
};

export default ViewContainer;
