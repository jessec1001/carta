import { FunctionComponent } from "react";
import { CaretIcon } from "components/icons";

import "./accordian.css";

/** The props used for the {@link AccordianToggle} component. */
interface AccordianToggleProps {
  /** Whether or not the toggler should include a standard caret icon. */
  caret?: boolean;
  /** Whether or not the accordian toggler is currently toggled. */
  toggled?: boolean;

  /** The event handler that is called whenever the accordian toggler is clicked. */
  onToggle?: () => void;
}

/** A component that toggles an accordian component and can optionally display as a caret icon. */
const AccordianToggle: FunctionComponent<AccordianToggleProps> = ({
  caret,
  toggled,
  onToggle,
  children,
}) => {
  return (
    <div className="accordian-toggle" onClick={onToggle}>
      {/* Render the children of this component normally. */}
      {children}

      {/* If the caret flag has been set, we use a caret icon to  */}
      {caret && <CaretIcon direction={toggled ? "down" : "right"} />}
    </div>
  );
};

export default AccordianToggle;
export type { AccordianToggleProps };
