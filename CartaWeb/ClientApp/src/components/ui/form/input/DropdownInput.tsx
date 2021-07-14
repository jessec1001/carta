import {
  FunctionComponent,
  HTMLProps,
  useEffect,
  useRef,
  useState,
} from "react";
import { useControllableState } from "hooks";
import { Modify } from "types";
import classNames from "classnames";

import "components/ui/form/form.css";

/** The props used for the {@link DropdownInput} component. */
interface DropdownInputProps {
  options: any[];
  // TODO: Allow for multiple selection.
  // multiple?: boolean;

  value?: any;
  onChange?: (value: any) => void;
}

/** A component that defines a dropdown select-like input. */
const DropdownInput: FunctionComponent<
  Modify<HTMLProps<HTMLDivElement>, DropdownInputProps>
> = ({ children, className, options, value, onChange, ...props }) => {
  // We need to allow this component to be optionally controllable because we are not using a native UI element.
  const [actualValue, setValue] = useControllableState(value, value, onChange);
  const [toggled, setToggled] = useState(false);

  // We keep track of the the header element for click detection purposes.
  const headerElement = useRef<HTMLDivElement>(null);

  // We also attach some event listeners to check when the component is clicked on.
  useEffect(() => {
    // If our header contains what was clicked on, we toggle the state.
    // Otherwise, we assume that the click was one to close the dropdown.
    const handleClick = (event: MouseEvent) => {
      if (headerElement.current) {
        if (headerElement.current.contains(event.target as Element))
          setToggled((prevToggled) => !prevToggled);
        else setToggled(false);
      }
    };

    // Setup and teardown.
    window.addEventListener("click", handleClick);
    return () => window.removeEventListener("click", handleClick);
  }, []);

  // TODO: Attempt to use native input element for accessibility.

  // Render a custom component that emulates a select/option input.
  return (
    <div
      className={classNames("form-dropdown", { toggled }, className)}
      {...props}
    >
      <div className="form-control form-dropdown-header" ref={headerElement}>
        {actualValue}
        {/* TODO: Create a custom arrow for dropdowns. */}
        <span className="form-dropdown-arrow">&#x25BC;</span>
      </div>
      {toggled && (
        <ul className="form-dropdown-list">
          {options.map((option, index) => {
            // Each inner option sets the value of
            return (
              <li
                key={index}
                onClick={(event) => {
                  if (actualValue !== option) setValue(option);
                }}
                className="form-dropdown-option"
              >
                {option}
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
};

// Export React component and props.
export default DropdownInput;
export type { DropdownInputProps };
