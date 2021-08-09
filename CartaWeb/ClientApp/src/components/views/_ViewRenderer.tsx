import React, { FunctionComponent, useContext } from "react";
import ViewContext from "./ViewContext";
import ViewSplitRenderer from "./ViewSplitRenderer";

const ViewRenderer: FunctionComponent = () => {
  const { viewId, actions } = useContext(ViewContext);
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
      default:
        content = <React.Fragment />;
        break;
    }
    return (
      <ViewContext.Provider
        value={{ viewId: view.currentId, actions: actions }}
      >
        {content}
      </ViewContext.Provider>
    );
  } else {
    return null;
  }
};

export default ViewRenderer;
