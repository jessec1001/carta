import { FunctionComponent } from "react";
import { CaretIcon } from "components/icons";
import { useAccordian } from "./Context";

import "./Accordian.css";

/** The props used for the {@link Toggle} component. */
interface ToggleProps {
  /** Whether or not the toggler should include a standard caret icon. */
  caret?: boolean;
}

/** A component that toggles an accordian component and can optionally display as a caret icon. */
const Toggle: FunctionComponent<ToggleProps> = ({ caret, children }) => {
  // Get the state of the accordian and the actions to operate on it.
  const { toggled, actions } = useAccordian();

  return (
    <div className="accordian-toggle" onClick={actions.toggle}>
      {/* Render the children of this component normally. */}
      {children}

      {/* If the caret flag has been set, we use a caret icon to  */}
      {caret && <CaretIcon direction={toggled ? "down" : "right"} />}
    </div>
  );
};

export default Toggle;
export type { ToggleProps };
