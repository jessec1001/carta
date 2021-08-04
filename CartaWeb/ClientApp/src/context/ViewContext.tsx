import { createContext, FunctionComponent } from "react";

/**
 * Represents a view that contains subviews within its own heirarchy.
 */
interface View {
  /** The function componnet that specifies how to render the view. */
  render: FunctionComponent;

  /** The unique identifier of the view. */
  id: ViewId;
  /** The parent view of the view. */
  parent: View | null;
  /** The child views of the view. */
  children: View[];
}
type ViewId = string | number;
/**
 * Represents the actions that can be performed on a view container.
 */
interface ViewActions {
  /**
   * Adds a new view with a specified renderer to an existing view.
   * @param view The identifier of the view to add the new view as a child onto.
   * @param render The component used to render the view.
   * @returns A unique identifier for the new view if added successfully; otherwise, `null`.
   */
  add: (render: FunctionComponent, view?: ViewId) => ViewId | null;
  /**
   * Removes an existing view from the view hierarchy.
   * @param view The identifier of the view remove.
   */
  remove: (view: ViewId) => void;
  /**
   * Determines if a particular view exists in the view heirarchy.
   * @param view The identifier of the view to check.
   * @returns Whether the view with the specified identifier exists.
   */
  has: (view: ViewId) => boolean;
}

/** The type of value of {@link ViewContext}. */
interface ViewContextValue {
  /** The container of the current view. */
  container: View | null;
  /** The root view. */
  root: View | null;
  /** The current view. */
  view: View | null;

  /** The actions that can be performed on the view. */
  actions: ViewActions | null;
}

/** A context for a view that could contain numerous child views. */
const ViewContext = createContext<ViewContextValue>({
  container: null,
  root: null,
  view: null,
  actions: null,
});

export default ViewContext;
export type { View, ViewId, ViewActions };
export type { ViewContextValue };
