import React, { FunctionComponent } from "react";
import AccordianToggle, { AccordianToggleProps } from "./AccordianToggle";

import "./accordian.css";

/** The props used for the {@link AccordianHeader} component. */
interface AccordianHeaderProps {
  /** Whether the content should be toggled. */
  toggled?: boolean;
  /** The event handler that is called whenever a toggle element in the accordian header is clicked. */
  onToggle?: () => void;
}

const AccordianHeader: FunctionComponent<AccordianHeaderProps> = ({
  toggled,
  onToggle,
  children,
}) => {
  return (
    <div className="accordian-header">
      {/* We perform special rendering for accordian toggle elements. */}
      {/* Otherwise, we should make sure to render elements in the original order. */}
      {React.Children.map(children, (child) => {
        if (
          React.isValidElement<AccordianToggleProps>(child) &&
          child.type === AccordianToggle
        ) {
          // We create a modified version of the on toggle component.
          const { onToggle: onChildToggle } = child.props;
          const handleToggle = () => {
            if (onChildToggle) onChildToggle();
            if (onToggle) onToggle();
          };

          return React.cloneElement(child, { toggled, onToggle: handleToggle });
        } else {
          return child;
        }
      })}
    </div>
  );
};

export default AccordianHeader;
export type { AccordianHeaderProps };
