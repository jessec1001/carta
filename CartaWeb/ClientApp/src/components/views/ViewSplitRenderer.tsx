import React, { FunctionComponent, useContext } from "react";
import { SplitView } from "./View";
import { SplitArea } from "components/containers";
import ViewContext from "./ViewContext";
import ViewRenderer from "./_ViewRenderer";

/** A component that renders a split view and its children within a split area container. */
const ViewSplitRenderer: FunctionComponent = () => {
  const { viewId, actions } = useContext(ViewContext);
  const view = actions.getView(viewId) as SplitView;
  const children = actions.getChildViews(viewId);

  if (children === null) return null;

  return (
    // TODO: Incorporate view sizes.
    // TODO: Handle subviews trees.
    <SplitArea direction={view.direction}>
      {children.map((child) => {
        return (
          <ViewContext.Provider
            key={child.currentId}
            value={{
              viewId: child.currentId,
              actions: actions,
            }}
          >
            <ViewRenderer />
          </ViewContext.Provider>
        );
      })}
    </SplitArea>
  );
};

/** What we need to do:
 *
 * - We need to test this split view renderer in the test page itself.
 * - We need some way to access actions associated with a split view:
 *   - Add a child to a split view container
 * - We need a context to pass information around to child components.
 * - We need to render views recursively.
 *
 */

export default ViewSplitRenderer;
