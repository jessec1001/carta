import React, { FC, useEffect, useRef, useState } from "react";
import classNames from "classnames";
import Toggler from "./Toggler";
import Area from "./Area";
import Item from "./Item";
import styles from "./Dropdown.module.css";
import DropdownContext from "./Context";

/** The props used for the {@link Dropdown} component. */
interface DropdownProps {
  /** The side that the dropdown area should be displayed on. */
  side: "top-left" | "top-right" | "bottom-left" | "bottom-right";

  /** Whether to ignore the hover effect on elements. */
  ignoreHover?: boolean;
}

/**
 * Defines the composition of the compound {@link Dropdown} component.
 * @borrows Toggler as Toggler
 * @borrows Area as Area
 * @borrows Item as Item
 */
interface DropdownComposition {
  Toggler: typeof Toggler;
  Area: typeof Area;
  Item: typeof Item;
}

/** A component that displays a dropdown menu mostly for navigation purposes and not for form purposes. */
const Dropdown: FC<DropdownProps> & DropdownComposition = ({
  side,
  ignoreHover,
  children,
}) => {
  // We will assume that there is only a single toggler element that we assign a reference to.
  const [toggled, setToggled] = useState(false);
  const elementRef = useRef<HTMLDivElement>(null);

  // Attach click handling to the dropdown.
  useEffect(() => {
    // Create a handler that closes the dropdown when some element outside of the dropdown is clicked.
    const handleClick = (event: MouseEvent) => {
      if (
        elementRef.current &&
        !elementRef.current.contains(event.target as Element)
      )
        setToggled(false);
    };

    // Setup and cleanup handler.
    window.addEventListener("click", handleClick);
    return () => window.removeEventListener("click", handleClick);
  }, []);

  return (
    // We wrap the child elements in an dropdown context to provide the dropdown functionality.
    <DropdownContext.Provider
      value={{ toggled: toggled, setToggled: setToggled }}
    >
      <div
        className={classNames(styles.dropdown, styles[side], {
          [styles.toggled]: toggled,
          [styles.hover]: !ignoreHover,
        })}
        ref={elementRef}
      >
        {children}
      </div>
    </DropdownContext.Provider>
  );
};
Dropdown.Toggler = Toggler;
Dropdown.Area = Area;
Dropdown.Item = Item;

export default Dropdown;
export type { DropdownProps };
