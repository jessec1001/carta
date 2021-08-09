import classNames from "classnames";
import React, { FunctionComponent } from "react";
import { SplitView } from "./View";

import "./split.css";

interface ViewSplitRendererProps {
  view: SplitView;
}
const ViewSplitRenderer: FunctionComponent<ViewSplitRendererProps> = ({
  view,
}) => {
  return null;
  // <SplitArea direction={view.direction}>
  //   {view.children.map((child) => child)}
  // </SplitArea>
};

export default ViewSplitRenderer;
