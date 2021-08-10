import React, { FunctionComponent, useEffect, useRef, useState } from "react";
import classNames from "classnames";
import DropdownToggler from "./DropdownToggler";
import DropdownArea from "./DropdownArea";

import "./dropdown.css";

/** The props used for the {@link Dropdown} component. */
interface DropdownProps {
  /** The side that the dropdown area should be displayed on. */
  side: "top-left" | "top-right" | "bottom-left" | "bottom-right";

  /** Whether to ignore the hover effect on elements. */
  ignoreHover?: boolean;
}

/** A component that displays a dropdown menu mostly for navigation purposes and not for form purposes. */
const Dropdown: FunctionComponent<DropdownProps> = ({
  side,
  ignoreHover,
  children,
}) => {
  // We will assume that there is only a single toggler element that we assign a reference to.
  const [toggled, setToggled] = useState(false);
  const togglerRef = useRef<HTMLDivElement>(null);

  // Attach click handling to the dropdown.
  useEffect(() => {
    // Create a handler that toggles the dropdown when the toggler is clicked on.
    // Otherwise, the dropdown should be closed.
    const handleClick = (event: MouseEvent) => {
      if (
        togglerRef.current &&
        togglerRef.current.contains(event.target as Element)
      ) {
        setToggled((prevToggled) => !prevToggled);
      } else setToggled(false);
    };

    // Setup and cleanup handler.
    window.addEventListener("click", handleClick);
    return () => window.removeEventListener("click", handleClick);
  }, []);

  console.log(!ignoreHover);
  return (
    // We do some compound component logic here to interpret dropdown parts.
    <div
      className={classNames("dropdown", side, { toggled, hover: !ignoreHover })}
    >
      {React.Children.map(children, (child, index) => {
        if (React.isValidElement(child)) {
          switch (child.type) {
            // For a dropdown toggler, we store a reference to it to handle clicking behavior.
            case DropdownToggler:
              return (
                <div ref={togglerRef} className="dropdown-toggler">
                  {child}
                </div>
              );

            // For a dropdown area, we only show it if the dropdown is toggled.
            case DropdownArea:
              return toggled ? (
                <div className="dropdown-area">{child}</div>
              ) : null;

            // If a non-dropdown element is encountered, render it normally.
            default:
              return child;
          }
        } else return child;
      })}
    </div>
  );
};

export default Dropdown;
export type { DropdownProps };
