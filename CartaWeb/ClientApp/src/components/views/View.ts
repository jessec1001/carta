import React from "react";

/** The base specification for any type of view. */
interface BaseView<T extends string> {
  /** The type of the view. */
  type: T;

  /** The unique identifier for the current view. */
  currentId: number;
  /** The unique identifier for the parent view or `null` if it does not exist. */
  parentId: number | null;
}

/** The specification of a view element. */
interface ElementView extends BaseView<"element"> {
  /** The element to render in the view. */
  element: React.ReactElement;
}
/** The specification of a split view container. */
interface SplitView extends BaseView<"split"> {
  /** The unique identifiers for each child of the view. */
  childIds: number[];

  /** The direction of splits in the view. */
  direction: "horizontal" | "vertical";
  /**
   * The ratio of sizes of the children in the view.
   * A size of zero indicates that the child should be visually collapsed.
   */
  sizes: number[];
}

/** The specificiation for the actions that can be on the view hierarchy. */
interface ViewActions {
  getView: (id: number) => View | null;
  getParentView: (id: number) => View | null;
  getChildViews: (id: number) => View[] | null;

  addChildElement: (
    parentId: number,
    element: React.ReactElement
  ) => number | null;
}

/** The type that a view is allowed to be. */
type View = ElementView | SplitView;

export default View;
export type { BaseView, ElementView, SplitView, ViewActions };
