import React from "react";

/** The base specification for any type of view. */
interface BaseView<T extends string> {
  /** The type of the view. */
  type: T;

  /** The title of the view. */
  title: React.ReactNode;
  // TODO: Check if a view is closeable before closing it in the context actions.
  /** Whether the view is closeable. */
  closeable: boolean;
  /** The status of the view. */
  status: "none" | "modified" | "unmodified" | "info" | "warning" | "error";

  /** The unique identifier for the current view. */
  currentId: number;
  /** The unique identifier for the parent view or `null` if it does not exist. */
  parentId: number | null;

  /** A set of tags that can be set on a view that exposes some variable to other views. */
  tags: Record<string, any>;
}

// TODO: Implement a basic container view that has no special rendering.
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
/** The specification of a tab view container. */
interface TabView extends BaseView<"tab"> {
  /** The unique identifiers for each child of the view. */
  childIds: number[];

  /** The identifier for the currently active child view. */
  activeId: string | number | null;
}
interface ModalView extends BaseView<"modal"> {
  /** The position of the modal. */
  position: { x: number; y: number };

  /** The unique identifiers for each child of the view. */
  childIds: number[];
}

/**
 * Determines whether the specified view is a container view.
 * @param view The view to check.
 * @returns Whether the view is a container.
 */
const isContainerView = (
  view: View
): view is SplitView | TabView | ModalView => {
  return view.type === "split" || view.type === "tab" || view.type === "modal";
};

/** The type that a view is allowed to be. */
type View = ElementView | SplitView | TabView | ModalView;

export default View;
export { isContainerView };
export type { BaseView, ElementView, SplitView, TabView };
