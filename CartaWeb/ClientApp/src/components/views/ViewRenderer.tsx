import { ViewContext } from "context";
import { FunctionComponent, useContext } from "react";

/** A component that renders the current view by recursively passing through each child view's renderer. */
const ViewRenderer: FunctionComponent = () => {
  // We use the view context that this component is assumed to be wrapped inside of.
  const { root, view, actions } = useContext(ViewContext);

  // We check to make sure that the view is valid before attempting to render.
  if (!view) return null;

  return (
    // If a component wants to hijack rendering, it occurs here before the view context is applied.
    <view.render>
      {view.children.map((child, index) => (
        <ViewContext.Provider
          key={index}
          value={{ container: view, root: root, view: child, actions }}
        >
          {/* We perform this rendering recursively to travel down the hierarchy. */}
          <ViewRenderer />
        </ViewContext.Provider>
      ))}
    </view.render>
  );
};

export default ViewRenderer;

// TODO: How to utilize the compound view component.
/**
 * 1. Elements of the hierarchy are either split containers or leaf-node elements.
 * 2. Split containers can be specified as:
 * {
 *  type: "split",
 *  id: 0,
 *  direction: "horizontal" | "vertical",
 *  children: [...],
 *  sizes: [...],
 * }
 * 3. Elements can be specified as:
 * {
 *  type: "element"
 *  id: 1,
 *  element: <... />
 * }
 */
