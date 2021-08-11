import { FunctionComponent, useContext } from "react";
import { SplitView } from "./View";
import { SplitArea } from "components/containers";
import ViewContext from "./ViewContext";
import ViewRenderer from "./ViewRenderer";

/** A component that renders a split view and its children within a split area container. */
const ViewSplitRenderer: FunctionComponent = () => {
  const { viewId, rootId, activeId, actions } = useContext(ViewContext);
  const view = actions.getView(viewId) as SplitView;
  const children = actions.getChildViews(viewId);

  // If we could not get the children views, we stop attempting to render.
  if (children === null) return null;

  return (
    // TODO: Incorporate view sizes.
    <SplitArea direction={view.direction}>
      {children.map((child) => {
        return (
          <ViewContext.Provider
            key={child.currentId}
            value={{
              viewId: child.currentId,
              rootId: rootId,
              activeId: activeId,
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

export default ViewSplitRenderer;
