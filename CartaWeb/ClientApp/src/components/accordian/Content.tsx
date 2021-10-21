import { FunctionComponent } from "react";
import classNames from "classnames";
import { useAccordian } from "./Context";

import "./Accordian.css";

/** The props used for the {@link Content} component. */
interface ContentProps {}

/** A component that contains the content for an {@link Accordian} component. */
const Content: FunctionComponent<ContentProps> = ({ children }) => {
  // Get the state of the accordian and the actions to operate on it.
  const { toggled } = useAccordian();

  // We return a component that should be rendered conditionally based on the toggled state.
  return (
    <div className={classNames("Accordian-Content", { toggled })}>
      {children}
    </div>
  );
};

export default Content;
export type { ContentProps };
