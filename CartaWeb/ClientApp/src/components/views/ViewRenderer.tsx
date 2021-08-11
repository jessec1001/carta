import React, { FunctionComponent, useContext } from "react";
import ViewContext from "./ViewContext";
import ViewSplitRenderer from "./ViewSplitRenderer";
import ViewTabRenderer from "./ViewTabRenderer";

const ViewRenderer: FunctionComponent = () => {
  const { viewId, rootId, activeId, actions } = useContext(ViewContext);
  const view = actions.getView(viewId);

  if (view) {
    let content: React.ReactElement;
    switch (view.type) {
      case "element":
        content = view.element;
        break;
      case "split":
        content = <ViewSplitRenderer />;
        break;
      case "tab":
        content = <ViewTabRenderer />;
        break;
      default:
        content = <React.Fragment />;
        break;
    }
    return (
      <ViewContext.Provider
        value={{
          viewId: view.currentId,
          rootId: rootId,
          activeId: activeId,
          actions: actions,
        }}
      >
        {content}
      </ViewContext.Provider>
    );
  } else {
    return null;
  }
};

export default ViewRenderer;
