import { createContext } from "react";
import { ViewActions } from "./View";

type ViewContextValue = {
  viewId: number;
  actions: ViewActions;
};

const ViewContext = createContext<ViewContextValue>(undefined!);

export default ViewContext;
export type { ViewContextValue };
