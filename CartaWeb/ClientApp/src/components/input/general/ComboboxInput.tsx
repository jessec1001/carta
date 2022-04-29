import React, { ComponentProps, FunctionComponent, useState } from "react";
import { InputAugment, InputAugmentContainer } from "../augment";
import OptionSelectorInput from "./OptionSelectorInput";
import OptionInput, { OptionInputProps } from "./OptionInput";
import TextFieldInput from "./TextFieldInput";

import "./input.css";

/** The props used for the {@link ComboboxInput} component. */
interface ComboboxInputProps extends ComponentProps<"div"> {
  comparer?: (value1: any, value2: any) => boolean;
  /** The text search that this combobox currently has input. */
  text?: string;
  /** The value that this combobox currently has selected. */
  value?: any | null;

  /** The event handler for when the text entered by the user has changed. */
  onTextChanged?: (text: string) => void;
  /** The event handler for when the value selected by the user has changed. */
  onValueChanged?: (value: any | null) => void;
}

/** A component that defines a combobox search and dropdown combination input. */
const ComboboxInput: FunctionComponent<ComboboxInputProps> = ({
  comparer,
  text,
  value,
  onTextChanged,
  onValueChanged,
  children,
  ...props
}) => {
  // We use a toggled state to circumstantially display the options for the combobox.
  const [toggled, setToggled] = useState(false);

  // We try to find the component with corresponding value to display exactly what was specified.
  const valueStrings = React.Children.toArray(children)
    .filter(
      (child) =>
        React.isValidElement(child) &&
        child.type === OptionInput &&
        (comparer
          ? comparer((child.props as OptionInputProps).value, value)
          : (child.props as OptionInputProps).value === value)
    )
    .map((child) => ((child as any).props as OptionInputProps).alias);
  const valueString = valueStrings[0] ?? "";

  // Event handlers for focussing and blurring the input component.
  // When the component is focussed, its previous value is lost.
  const handleFocus = () => {
    setToggled(true);
    onValueChanged && onValueChanged(null);
  };
  const handleBlur = () => {
    setToggled(false);
  };

  return (
    <OptionSelectorInput toggled={toggled} onSelect={onValueChanged} {...props}>
      {/* This text field will end up in the header of the option selector input. */}
      {/* When this text input is focused or blurred, the option visibility is toggled on or off respectively. */}
      <InputAugmentContainer side="right">
        <TextFieldInput
          value={value === null ? text : valueString}
          onChange={onTextChanged}
          onFocus={handleFocus}
          onBlur={handleBlur}
          className="input-combobox"
        />
        {value !== null && (
          <InputAugment style={{ color: "var(--color-primary)" }}>
            ✓
          </InputAugment>
        )}
      </InputAugmentContainer>
      {children}
    </OptionSelectorInput>
  );
};

export default ComboboxInput;
export type { ComboboxInputProps };
