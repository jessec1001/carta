import { createContext } from "react";
import { ViewActions } from "./View";

type ViewContextValue = {
  rootId: number;
  viewId: number;
  activeId: number | null;
  actions: ViewActions;
};

const ViewContext = createContext<ViewContextValue>(undefined!);

export default ViewContext;
export type { ViewContextValue };
