import { Tabs } from "components/tabs";
import { FunctionComponent, useContext } from "react";
import ViewContext from "./ViewContext";
import ViewRenderer from "./ViewRenderer";

const ViewTabRenderer: FunctionComponent = () => {
  const { viewId, rootId, activeId, actions } = useContext(ViewContext);
  const children = actions.getChildViews(viewId);

  // If we could not get the children views, we stop attempting to render.
  if (children === null) return null;

  return (
    // TODO: Incorporate tab focus.
    <Tabs>
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
    </Tabs>
  );
};

export default ViewTabRenderer;
