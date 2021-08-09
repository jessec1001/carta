import React from "react";

/** The base specification for any type of view. */
interface BaseView<T extends string> {
  /** The type of the view. */
  type: T;
  /** The unique identifier for the view. */
  id: number;
}

/** The specification of a view element. */
interface ElementView extends BaseView<"element"> {
  /** The element to render in the view. */
  element: React.ReactElement;
}
/** The specification of a split view container. */
interface SplitView extends BaseView<"split"> {
  /** The direction of splits in the view. */
  direction: "horizontal" | "vertical";
  /** The children of the view. */
  children: View[];
  /**
   * The ratio of sizes of the children in the view.
   * A size of zero indicates that the child should be visually collapsed.
   */
  sizes: number[];
}

/** The type that a view is allowed to be. */
type View = ElementView | SplitView;

export default View;
export type { BaseView, ElementView, SplitView };
