import React, {
  forwardRef,
  FunctionComponent,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { View, ViewActions, ViewContext, ViewId } from "context";

/** A component that stores views within a flexible container. */
const ViewContainer: FunctionComponent<{
  actionRef: React.Ref<ViewActions>;
}> = ({ actionRef, ...props }) => {
  // We store the identifier value in the reference and increment it whenver we construct a new view.
  // We do not need this to be stateful because the children components should not be rerendered when the next
  // identifier changes.
  const id = useRef<number | null>(0);

  // These are utility functions used to modify the view hierarchy.
  /**
   * Creates a new view with empty contents under a specified parent with given properties.
   * @param parent The parent view.
   * @param render The renderer for the view.
   * @returns The newly created view.
   */
  const createView = useCallback(
    (parent: View | null, render: FunctionComponent): View => {
      const newView: View = {
        // The view properties.
        render: render,

        // The identifying information of the view.
        id: id.current!++,
        parent: parent,
        children: [],
      };
      return newView;
    },
    []
  );
  /**
   * Finds a view object by identifier within a particular segment of the view hierarchy.
   * Works by searching through each branch of the hierarchy recursively.
   * @param view The segment of the view hierarchy to search through.
   * @param viewId The identifier of the view to find.
   * @returns A view object if it exists within the hierarchy; otherwise, `null`.
   */
  const findView = useCallback((view: View, viewId: ViewId): View | null => {
    // Recursion base case: is the current view the view we are looking for?
    if (view.id === viewId) return view;

    // Recursion recursive case: are any of the children views the view we are looking for?
    for (let k = 0; k < view.children.length; k++) {
      const subview = findView(view.children[k], viewId);
      if (subview !== null) return subview;
    }

    // Failed to find the queried view.
    return null;
  }, []);
  /**
   * Clones an existing view and maintains all parent child links.
   * @param view The view to clone.
   * @returns A cloned view object.
   */
  const cloneView = useCallback((view: View): View => {
    const { children, parent, ...rest } = view;
    const clonedView: View = { ...rest, children: [], parent: parent };
    for (let k = 0; k < children.length; k++) {
      const clonedChildView = cloneView(children[k]);
      clonedChildView.parent = clonedView;
      clonedView.children.push(clonedChildView);
    }
    return clonedView;
  }, []);

  // We construct the initial state of the view.
  const [view, setView] = useState<View>(() => {
    // The container itself does not do any rendering and has no parent element.
    // The defining characteristic that the parent of the root is null allows for bottom-top recursion.
    return createView(null, ({ children }) => (
      <React.Fragment>{children}</React.Fragment>
    ));
  });

  // These are the actions that are passed on to the context to provide to children.
  const add = useCallback(
    (render: FunctionComponent, viewId: ViewId = 0): ViewId | null => {
      // Initially, create a new view with no parent.
      // We will set the parent later once we have found it.
      const newView = createView(null, render);

      setView((view) => {
        // First, find the parent view.
        const clonedView = cloneView(view);
        const foundView = findView(clonedView, viewId);

        // Second, create the new view and add it to its parent (making sure to keep immutability).
        if (foundView) {
          newView.parent = foundView;
          foundView.children.push(newView);
          setView(clonedView);
          return clonedView;
        }
        return view;
      });

      return newView.id;
    },
    [findView, cloneView, createView]
  );
  const remove = useCallback(
    (viewId: ViewId): void => {
      setView((view) => {
        // First, find the view.
        const clonedView = cloneView(view);
        const foundView = findView(clonedView, viewId);

        // Second, remove the view from its parent (making sure to keep immutability).
        if (foundView && foundView.parent) {
          foundView.parent.children = foundView.parent.children.filter(
            (child) => child.id !== viewId
          );
          return clonedView;
        }
        return view;
      });
    },
    [findView, cloneView]
  );
  const has = useCallback(
    (viewId: ViewId): boolean => {
      // We check for containment purely based on the view identifier.
      return findView(view, viewId) !== null;
    },
    [findView, view]
  );

  const ViewContainerWithRef = forwardRef<
    ViewActions,
    { actions: ViewActions }
  >(({ children, actions }, ref) => {
    // We use some magic to construct the component itself to allow for a reference to be kept to the internal view data.
    // This allows for a component to reference the view within the component that renders it.
    // If a reference is specified on this container, we make sure to set it to the current view.
    useEffect(() => {
      if (ref !== null) {
        if (typeof ref === "function") ref(actions);
        else ref.current = actions;
      }
    }, [ref, actions]);

    // We wrap a view context with a reference to this view around the children of this element.
    // We do not explicitly render anything in this component so that the rendering logic may be specified elsewhere.
    // This also helps to separate the concerns of rendering logic and container logic.
    return <React.Fragment>{children}</React.Fragment>;
  });

  const actions = useMemo(
    () => ({
      add,
      remove,
      has,
    }),
    [add, remove, has]
  );
  const contextValue = useMemo(() => {
    return {
      container: null,
      root: view,
      view: view,
      actions: actions,
    };
  }, [view, actions]);

  return (
    <ViewContext.Provider value={contextValue}>
      <ViewContainerWithRef ref={actionRef} actions={actions} {...props} />
    </ViewContext.Provider>
  );
};

export default ViewContainer;
