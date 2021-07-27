import React, {
  FunctionComponent,
  HTMLProps,
  useEffect,
  useRef,
  useState,
} from "react";
import { useControllableState } from "hooks";
import { Modify } from "types";
import { CaretIcon } from "components/icons";
import { InputAugment, InputAugmentContainer } from "../augment";
import OptionSelectorInput from "./OptionSelectorInput";
import OptionInput, { OptionInputProps } from "./OptionInput";

import "./input.css";

/** The props used for the {@link DropdownInput} component. */
interface DropdownInputProps {
  value?: any;
  onChange?: (value: any) => void;
}

/** A component that defines a dropdown select-like input. */
const DropdownInput: FunctionComponent<
  Modify<HTMLProps<HTMLDivElement>, DropdownInputProps>
> = ({ children, className, value, onChange, ...props }) => {
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

  // We try to find the component with corresponding value to display exactly what was specified.
  const valueComponent = React.Children.toArray(children).filter(
    (child) =>
      React.isValidElement(child) &&
      child.type === OptionInput &&
      (child.props as OptionInputProps).value === actualValue
  );

  // Render a custom component that emulates a select/option input.
  return (
    <OptionSelectorInput toggled={toggled} onSelect={setValue}>
      <InputAugmentContainer side="right">
        <div className="form-control" ref={headerElement}>
          {valueComponent}
        </div>
        <InputAugment>
          <CaretIcon />
        </InputAugment>
      </InputAugmentContainer>
      {children}
    </OptionSelectorInput>
  );
};

// Export React component and props.
export default DropdownInput;
export type { DropdownInputProps };
